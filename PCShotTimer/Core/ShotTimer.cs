﻿using NAudio;
using NAudio.Wave;
using PCShotTimer.openshottimer;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading;
using System.Timers;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace PCShotTimer.Core
{
    /// <summary>
    ///     This class handles the shot timer behaviors:
    ///     - Starting the timer and managing a random delay before beeping
    ///     - Analyzing mic input to detect shots fired
    ///     - TODO Implementing PAR loops
    /// </summary>
    public class ShotTimer : INotifyPropertyChanged, IDisposable
    {
        #region Constants

        /// <summary>Default timer value returned by TimeElapse when nothing is going on.</summary>
        public const string DefaultTimerValue = "00:00:000";

        /// <summary>Indicates how many milliseconds shall pass before PropertyChanging the TimeElapse property.</summary>
        private const int TimeElapseUpdateMs = 1;

        #endregion

        #region Members

        /// <summary>A Random object.</summary>
        protected readonly Random _random = new Random();

        /// <summary>Shot detector class.</summary>
        protected readonly ShotDetector _shotDetector;

        /// <summary>A SoundPlayer object only used to play the BEEP.</summary>
        protected SoundPlayer _beepSoundPlayer = new SoundPlayer();

        /// <summary>
        ///     This DispatcherTimer is used to trigger PropertyChange every tick
        ///     (as there is no Tick event on the Stopwatch object).
        /// </summary>
        protected DispatcherTimer _dispatcherTimer = new DispatcherTimer();

        /// <summary>The _options used by the timer.</summary>
        protected OptionsData _options;

        /// <summary>The random delay Timer.</summary>
        protected Timer _randomDelay = null;

        /// <summary>A SoundPlayer object.</summary>
        protected SoundPlayer _soundPlayer = new SoundPlayer();

        /// <summary>The SFX directory used by ShotTimer.</summary>
        protected string _soundsDirectory;

        /// <summary>Indicates that a we want to Stop the time.</summary>
        protected bool _stopRequested;

        /// <summary>The shot timer Stopwatch.</summary>
        protected Stopwatch _stopWatch = new Stopwatch();

        /// <summary>Wave In device</summary>
        protected WaveInEvent _waveIn;

        #endregion

        #region Properties

        /// <summary>Gets or sets the ShotFired event that is triggered when a shot sound is detected.</summary>
        public EventHandler ShotFired { get; set; }

        /// <summary>Gets or sets the SFX directory used by ShotTimer.</summary>
        public string SoundsDirectory
        {
            get { return _soundsDirectory; }
            set
            {
                if (!Directory.Exists(value))
                    throw new DirectoryNotFoundException(value);

                _soundsDirectory = value;
            }
        }

        /// <summary>Total time elapsed since the Shot timer has been started.</summary>
        public string TimeElapsed { get; set; }

        /// <summary>PropertyChanged event used by TimeElapsed to nofity any binding that the value has changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of ShotTimerClass.
        /// </summary>
        /// <param name="options">_options used by this timer.</param>
        /// <param name="shotFiredEvent">Mehtod to call when a shot has been detected.</param>
        /// <param name="waveIn">The Wave in device that will be used to capture sound.</param>
        public ShotTimer(OptionsData options, EventHandler shotFiredEvent, WaveInEvent waveIn)
        {
            _options = options;
            ShotFired = shotFiredEvent;
            SoundsDirectory = String.Format(@"{0}\{1}", App.AppDirectory, App.SoundsDirecoryName);
            TimeElapsed = DefaultTimerValue;

            // Uses the second Core or Processor
            try
            {
                Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2);
            }
            catch (Exception e)
            {
                App.Error("Setting processor affinity failed: {0}", e.Message);
            }

            // Check
            if (String.IsNullOrEmpty(_options.SoundSelectedBeepFile))
                throw new FileNotFoundException("No 'beep' sound available. Can't continue.");

            // Load the sound now so we don't waste time later
            _beepSoundPlayer.SoundLocation = _options.SoundSelectedBeepFile;
            _beepSoundPlayer.Load();

            // Assign ReadingAvailableAudioData for reading the audio input
            _waveIn = waveIn;
            _waveIn.DataAvailable += ReadingAvailableAudioData;

            // Create the shot detector: here we use a simple noise spike detector.
            _shotDetector = new AmplitudeSpikeShotDetector(
                _options.InputSampleRate,
                _options.InputSampleBits,
                _options.DetectorDuration,
                _options.DetectorLoudness);

            // Capture the audio in
            try
            {
                _waveIn.StartRecording();
            }
            catch (MmException exception)
            {
                throw new ApplicationException(
                    String.Format("The device {0} cannot be openned... it seems.", _options.InputDeviceId), exception);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            _soundPlayer.Dispose();
            _randomDelay.Dispose();
        }

        /// <summary>
        ///     "Starts" the shot timer.
        ///     - First will ask if shooter ready.
        ///     - Then will wait a certain dealy before beeping, if applicable
        ///     - Finally, will start the real deal and detect shot and stuff
        /// </summary>
        public void Start()
        {
            _stopRequested = false;

            // Play sounds in a new thread to prevent UI freeze
            var thread = new Thread(PlayReadyStandbySound);
            thread.Start();
        }

        /// <summary>
        ///     Stops the shot timer.
        /// </summary>
        public void Stop()
        {
            _stopRequested = true;
            _soundPlayer.Stop(); // Watch out, will wait any sound being played to finish

            // Stop both stopwatch and dispatcher
            // and also randomdelay in case the dude is stopping right after starting
            _stopWatch.Stop();
            _dispatcherTimer.Stop();
            if (_randomDelay != null)
                _randomDelay.Stop();

            // Calls the tick one last time to make sure that the TimeElapsed is accurate.
            OnDispatcherTimerTick(this, EventArgs.Empty);

            // Clear the time elapsed
            TimeElapsed = DefaultTimerValue;

            // Revert process priorities
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
            Thread.CurrentThread.Priority = ThreadPriority.Normal;
        }

        /// <summary>
        ///     Resets this Shot Timer.
        /// </summary>
        public void Reset()
        {
            if (null != _stopWatch)
                _stopWatch.Reset();

            _shotDetector.Reset();
            TimeElapsed = DefaultTimerValue;
        }

        /// <summary>
        ///     Transform a number of milliseconds into a human readable format
        ///     minutes:seconds:millis
        /// </summary>
        /// <param name="timeSpan">A TimeSpan object to read</param>
        /// <returns>Human readable string.</returns>
        public static string HumanReadableMs(TimeSpan timeSpan)
        {
            return TimeSpan.Zero == timeSpan
                ? DefaultTimerValue
                : String.Format("{0:00}:{1:00}:{2:000}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }

        #endregion

        #region Internal Methods   

        /// <summary>
        ///     Plays a random shooter ready and standby sound
        ///     And then proceeds to wait a certain dealy before beeping, if applicable
        ///     Finally, start the real deal and detect shot and stuff
        /// </summary>
        protected void PlayReadyStandbySound()
        {
            if (_stopRequested)
                return;

            if (_options.SoundPlayReadyStandby)
            {
                // Careful
                if (0 != _options.SoundSelectedReadyStandbyFiles.Count)
                {
                    // Select a random ready/standby sound from the selected ones
                    var readyStandbyFiles = _options.SoundSelectedReadyStandbyFiles;
                    var randomSound = _random.Next(0, readyStandbyFiles.Count);

                    // Play Ready sound
                    _soundPlayer.SoundLocation = readyStandbyFiles[randomSound];
                    _soundPlayer.Load();
                    _soundPlayer.PlaySync();
                }
            }

            BeepDelay();
        }

        /// <summary>
        ///     Waits for a period of time before beeping.
        ///     - If random beeping is activtated then will wait between _randomStartDelayMin and _randomStartDelayMax.
        ///     - Or else the min value is applied.
        /// </summary>
        protected void BeepDelay()
        {
            if (_stopRequested)
                return;

            // Default is min delay
            var delayMs = _options.GeneralRandomStartDelayMin*1000;

            // Random delay
            if (_options.GeneralRandomDelay)
            {
                // For example between 0.6;4.3 ---> becomes 60;430
                var min = Convert.ToInt32(_options.GeneralRandomStartDelayMin*100);
                var max = Convert.ToInt32(_options.GeneralRandomStartDelayMax*100);

                // Let's say random number chosen is --> 223
                var delay = _random.Next(min, max);

                // 223 * 10 --> 2230 ms
                delayMs = delay*10;
            }

            // Starts timer to delay the beep
            _randomDelay = new Timer(delayMs);
            _randomDelay.Elapsed += RandomDelayElapsed;
            _randomDelay.Start();
        }

        /// <summary>
        ///     Starts the timer -- for real -- as this occurs after any random delay, if there was one.
        /// </summary>
        protected void StartForReal()
        {
            if (_stopRequested)
                return;

            // Play the Beep sound
            _beepSoundPlayer.Play();

            // Assing OnDispatcherTimerTick for every tick the dispatcher does
            // We need this to regulary PropertyChange our TimeElapse property
            _dispatcherTimer.Tick += OnDispatcherTimerTick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, TimeElapseUpdateMs);

            // Reset everything just before starting the time.
            // Here this will also reset the Shot Detector currentsample, so it will reset it to 0ms.
            Reset();

            // Also create a stop watch, that stuff has handy mehtods like ElapsedStuff
            _stopWatch.Start();

            // Run the dispatcher
            _dispatcherTimer.Start();

            // Prevents "Normal" processes from interrupting Threads
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
        }

        /// <summary>
        ///     Triggered once the random delay is finished.
        ///     Right after we start the real timer.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        protected void RandomDelayElapsed(object sender, ElapsedEventArgs e)
        {
            _randomDelay.Stop();

            // And now, go to town
            StartForReal();
        }

        /// <summary>
        ///     For every tick, the TimeElapsed property is notified as PropertyChanged
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Events</param>
        private void OnDispatcherTimerTick(object sender, EventArgs e)
        {
            var propertyChanged = PropertyChanged;
            if (null == propertyChanged)
                return;

            TimeElapsed = HumanReadableMs(_stopWatch.Elapsed);
            propertyChanged(this, new PropertyChangedEventArgs("TimeElapsed"));
        }

        /// <summary>
        ///     Triggered regulary with wave input data.
        ///     Here we will analyze the audio data and detect shots.
        ///     When shots are detected the ShotFired() is invoked.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">WaveInEventArgs. This one has a buffer byte array that represents audio data.</param>
        private void ReadingAvailableAudioData(object sender, WaveInEventArgs e)
        {
            // Transform the byte array to a MemoryStream
            var stream = new MemoryStream();
            stream.Write(e.Buffer, 0, e.BytesRecorded);
            stream.Position = 0;

            // Process the audio to detect shots
            var shotsFound = _shotDetector.ProcessAudio(stream);

            // The stopwatch needs to be running, or we will just discard those shots that occured before the Start 
            if (!_stopWatch.IsRunning)
                return;

            // Shots found
            if (shotsFound.Length > 0)
            {
                // Invoke the ShotFired event, that MainWindow will catch
                ShotFired.Invoke(this, new ShotEventArgs(shotsFound));
            }
        }

        #endregion
    }
}