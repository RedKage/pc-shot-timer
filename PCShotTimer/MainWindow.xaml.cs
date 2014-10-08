using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NAudio.Wave;
using PCShotTimer.openshottimer;

namespace PCShotTimer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Events

        /// <summary>Blinking animation for the Green LED</summary>
        private Storyboard _ledGreenBlinking;

        /// <summary>Program options.</summary>
        private OptionsData _options;

        /// <summary>The options window.</summary>
        private OptionsWindow _optionsWindow;

        /// <summary>The previous program options. Used in case of error/exception requiring reverting the options.</summary>
        private OptionsData _previousOptions;

        /// <summary>Shot timer.</summary>
        private ShotTimer _shotTimer;

        /// <summary>Blinking animation for big ass timer.</summary>
        private Storyboard _timerBlinking;

        #endregion

        #region Constructors

        /// <summary>
        ///     Creates the main window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Load the defaults params         
            _options = new OptionsData(App.DefaultConfig);

            // Init in a new thread for faster startup
            var thread = new Thread(Initialize);
            thread.Start();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        ///     Initialize... stuff.
        /// </summary>
        protected void Initialize()
        {
            try
            {
                Dispatcher.Invoke((Action) (() =>
                {
                    Clear();

                    // Initializes the audio input device
                    var waveIn = new WaveInEvent
                    {
                        DeviceNumber = _options.SelectedDeviceId,
                        BufferMilliseconds = 100,
                        NumberOfBuffers = 3,
                        WaveFormat = new WaveFormat(
                            _options.InputSampleRate,
                            _options.InputSampleBits,
                            _options.InputChannels)
                    };

                    // Loading animations from XAML
                    _ledGreenBlinking = (Storyboard) Resources["GreenLedBlinking"];
                    _timerBlinking = (Storyboard) Resources["TimerBlinking"];

                    // Create the shot timer
                    _shotTimer = new ShotTimer(_options, WhenShotDetected, waveIn);

                    // For binding the TimeElapse property
                    DataContext = _shotTimer;

                    // Create the options window
                    _optionsWindow = new OptionsWindow(_options);
                }));
            }
            catch (ApplicationException exception)
            {
                var message = String.Format("{0}\nReverting to previous settings.", exception.Message);

                // We can stop the bleeding!
                App.DialogWarning(message);

                // Revert the previous config
                _options = _previousOptions;

                // Try again
                var thread = new Thread(Initialize);
                thread.Start();
            }
            catch (Exception)
            {
                // Let's crash that party
                App.Error("Unknown error from Initialize()");
                throw;
            }
        }

        /// <summary>
        ///     Clear the Shot Timer so it is reseted to a defautl state.
        /// </summary>
        protected void Clear()
        {
            LstViewShots.Items.Clear();
            Console.Clear();
            TxtBoxTotalTime.Text = ShotTimer.DEFAULT_TIMER_VALUE;
            BtnClear.IsEnabled = false;
            if (null != _shotTimer)
                _shotTimer.Reset();
            if (null != _timerBlinking)
                _timerBlinking.Stop();
        }

        /// <summary>
        ///     Exit the app.
        /// </summary>
        protected void Exit()
        {
            if (null != _shotTimer)
                _shotTimer.Stop();
            Application.Current.Shutdown(0);
        }

        /// <summary>
        ///     Triggered when a shot has been detected.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">A special ShotEventArgs that just bundles a ShotEvent array of found shots.</param>
        private void WhenShotDetected(object sender, EventArgs e)
        {
            Dispatcher.Invoke((Action) (() =>
            {
                // Yeah ugly shit, but I don't think we can do better here
                var ee = (ShotEventArgs) e;

                foreach (var shot in ee.Shots)
                {
                    // Format all the stuff for easy printing
                    var shotNumber = shot.ShotNum.ToString(CultureInfo.InvariantCulture);
                    var shotTime = ShotTimer.HumanReadableMs(shot.Time);
                    var shotSplit = ShotTimer.HumanReadableMs(shot.Split);

                    // Print in console
                    Console.AppendText(String.Format("\n* Shot {0} detected @ {1}. Split={2}", shotNumber, shotTime,
                        shotSplit));
                    Console.ScrollToEnd();

                    // Create a row
                    LstViewShots.Items.Add(new {Id = shotNumber, Time = shotTime, Split = shotSplit});

                    // Focus on last row
                    LstViewShots.SelectedItem = LstViewShots.Items.GetItemAt(LstViewShots.Items.Count - 1);
                    LstViewShots.ScrollIntoView(LstViewShots.Items[0]);
                    LstViewShots.ScrollIntoView(LstViewShots.SelectedItem);
                    LstViewShots.SelectedItem = null;
                }
            }));
        }

        #endregion

        #region Events

        /// <summary>
        ///     Start the shot timer
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            // Grey out buttons and stuff
            BtnStart.IsEnabled = false;
            BtnClear.IsEnabled = false;
            BtnOption.IsEnabled = false;
            BtnStop.IsEnabled = true;
            TxtBoxTotalTime.Text = ShotTimer.DEFAULT_TIMER_VALUE;

            // Start blinkin the LEDs
            LedGreen.Opacity = 1.0;
            LedRed.Opacity = 0.5;
            _ledGreenBlinking.Begin();
            _timerBlinking.Stop();

            // Start shot timer
            _shotTimer.Start();
        }

        /// <summary>
        ///     Stop the timer.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            // Grey out self to prevent people from clicking like crazy monkeys
            BtnStop.IsEnabled = false;

            // Stop the timer, which will wait to finish current playback, so new thread
            var thread = new Thread(_shotTimer.Stop);
            thread.Start();

            // Grey out buttons and stuff
            BtnClear.IsEnabled = true;
            BtnOption.IsEnabled = true;
            BtnStart.IsEnabled = true;

            // Stop blinkin and reset the LEDs
            _ledGreenBlinking.Stop();
            LedGreen.Opacity = 0.5;
            LedRed.Opacity = 1.0;

            // Show lastest time
            if (LstViewShots.Items.Count < 1)
                return;

            var latestTimeRow =
                LstViewShots.ItemContainerGenerator.ContainerFromIndex(LstViewShots.Items.Count - 1) as ListViewItem;
            if (null == latestTimeRow)
                return;

            var latestTime = (dynamic) latestTimeRow.Content;
            latestTimeRow.Background = Brushes.LightGray;
            TxtBoxTotalTime.Text = latestTime.Time;
            _timerBlinking.Begin();
        }

        /// <summary>
        ///     Clear the Shot Timer so it is reseted to a defautl state.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        /// <summary>
        ///     Show to options screen.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void ButtonOptions_Click(object sender, RoutedEventArgs e)
        {
            // Backup the config
            _previousOptions = _options.Clone();
            var rc = _optionsWindow.ShowDialog();

            // Null = cancel; false = OK
            if (null == rc)
            {
                // Restore the old options when canceled
                _options = _previousOptions;
            }

            // Options changed, so gotta reload everything man
            Initialize();
        }

        /// <summary>
        ///     Exits the app
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }

        /// <summary>
        ///     Closing the window.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void WinMainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (null != _shotTimer)
                _shotTimer.Stop();
        }

        /// <summary>
        ///     Window is closed.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void WinMainWindow_Closed(object sender, EventArgs e)
        {
            Exit();
        }

        #endregion
    }
}