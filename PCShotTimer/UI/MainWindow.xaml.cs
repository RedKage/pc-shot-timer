using NAudio.Wave;
using PCShotTimer.Core;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PCShotTimer.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Members

        /// <summary>Keep the big ass binding as we clear it when we stop and restore it on start.</summary>
        private readonly Binding _timerBinding;

        /// <summary>
        ///     Indicates whether the timer blinking animation is running.
        ///     Since M$ can't pull their fingers outta their asses and provide us with a cool property doing just that.
        ///     Yes, I am pissed.
        /// </summary>
        private bool _isTimerBlinking;

        /// <summary>Blinking animation for the Green LED</summary>
        private Storyboard _ledGreenBlinking;

        /// <summary>The options window.</summary>
        private OptionsWindow _optionsWindow;

        /// <summary>The previous program options. Used in case of error/exception requiring reverting the options.</summary>
        private OptionsData _previousOptions;

        /// <summary>Blinking animation for big ass timer.</summary>
        private Storyboard _timerBlinking;

        #endregion

        #region Properties

        /// <summary>Gets or sets the program options.</summary>
        public OptionsData Options { get; set; }

        /// <summary>Gets the ShotTimer.</summary>
        public ShotTimer ShotTimer { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Creates the main window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            var userConfig = String.Format(@"{0}\{1}", App.AppDirectory, App.UserConfigFileName);

            try
            {
                // Load the user config
                Options = new OptionsData(userConfig);
            }
            catch (FileNotFoundException)
            {
                // Load the defaults params
                App.Info("User config not found '{0}'", userConfig);
                Options = new OptionsData(App.DefaultConfig);
                App.Info("Default embedded config used");
            }
            catch (Exception e)
            {
                App.DialogFatalError(e);
            }

            // Save the (big ass) timer binding. We will have to clear it when we stop and restore it on start.
            _timerBinding = BindingOperations.GetBinding(TxtBoxTotalTime, TextBox.TextProperty);

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
            Dispatcher.Invoke((Action) (() =>
            {
                try
                {
                    App.Info("Initializing...");
                    Clear();

                    // Check input devices
                    if (0 == WaveInEvent.DeviceCount)
                        throw new Exception("No input device found. Can't continue.");

                    // Initializes the audio input device
                    var waveIn = new WaveInEvent
                    {
                        DeviceNumber = Options.InputDeviceId,
                        BufferMilliseconds = 100,
                        NumberOfBuffers = 3,
                        WaveFormat = new WaveFormat(
                            Options.InputSampleRate,
                            Options.InputSampleBits,
                            Options.InputChannels)
                    };

                    // Loading animations from XAML
                    _ledGreenBlinking = (Storyboard) Resources["GreenLedBlinking"];
                    _timerBlinking = (Storyboard) Resources["TimerBlinking"];

                    // Create the shot timer
                    ShotTimer = new ShotTimer(Options, WhenShotDetected, waveIn);

                    // For binding the TimeElapse property
                    DataContext = ShotTimer;

                    // Create the options window
                    _optionsWindow = new OptionsWindow(Options);
                }
                catch (ApplicationException exception)
                {
                    var message = String.Format("{0}\nReverting to previous settings.", exception.Message);

                    // We can stop the bleeding!
                    App.DialogContinue(message);

                    // Revert the previous config
                    Options = _previousOptions;

                    // Try again
                    var thread = new Thread(Initialize);
                    thread.Start();
                }
                catch (Exception e)
                {
                    // Let's crash that party
                    App.DialogFatalError(e);
                }
            }));
        }

        /// <summary>
        ///     Clear the Shot Timer so it is reseted to a defautl state.
        /// </summary>
        protected void Clear()
        {
            LstViewShots.Items.Clear();
            TxtBoxTotalTime.Text = ShotTimer.DefaultTimerValue;
            BtnClear.IsEnabled = false;
            if (null != ShotTimer)
                ShotTimer.Reset();

            // OMG I'm so pissed right now
            if (null != _timerBlinking && _isTimerBlinking)
            {
                _isTimerBlinking = false;
                _timerBlinking.Stop();
            }
        }

        /// <summary>
        ///     Exit the app.
        /// </summary>
        protected void Exit()
        {
            App.Info("Exit request");
            if (null != ShotTimer)
                ShotTimer.Stop();
            Application.Current.Shutdown(0);
        }

        /// <summary>
        ///     Saves the options.
        /// </summary>
        protected void SaveOptions()
        {
            try
            {
                App.Info("Saving options");
                Options.Save(String.Format(@"{0}\{1}", App.AppDirectory, App.UserConfigFileName));
            }
            catch (Exception exception)
            {
                App.DialogWarning(exception.Message);
            }
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

                    App.Info(String.Format("Shot {0} detected @ {1}. Split={2}", shotNumber, shotTime, shotSplit));

                    // Create a row
                    LstViewShots.Items.Add(new ShotTimeRow {Id = shotNumber, Time = shotTime, Split = shotSplit});

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
            App.Info("Start");
            App.Info(Options.ToString());

            // Grey out buttons and stuff
            BtnStart.IsEnabled = false;
            BtnClear.IsEnabled = false;
            BtnOption.IsEnabled = false;
            BtnStop.IsEnabled = true;
            TxtBoxTotalTime.Text = ShotTimer.DefaultTimerValue;

            // Start blinkin the LEDs
            LedGreen.Opacity = 1.0;
            LedRed.Opacity = 0.5;
            _ledGreenBlinking.Begin();

            // Stop the timer blinking
            if (_isTimerBlinking)
            {
                // Sigh...
                _isTimerBlinking = false;
                _timerBlinking.Stop();
            }

            // Activate binding for the big ass timer up there
            BindingOperations.SetBinding(TxtBoxTotalTime, TextBox.TextProperty, _timerBinding);

            // Start shot timer
            ShotTimer.Start();
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
            var thread = new Thread(ShotTimer.Stop);
            thread.Start();

            // Grey out buttons and stuff
            BtnClear.IsEnabled = true;
            BtnOption.IsEnabled = true;
            BtnStart.IsEnabled = true;

            // Stop blinkin and reset the LEDs
            _ledGreenBlinking.Stop();
            LedGreen.Opacity = 0.5;
            LedRed.Opacity = 1.0;

            // Stop the big ass timer's binding as it would continue to be updated by the property
            BindingOperations.ClearBinding(TxtBoxTotalTime, TextBox.TextProperty);

            // Show lastest time
            if (LstViewShots.Items.Count < 1)
            {
                TxtBoxTotalTime.Text = ShotTimer.DefaultTimerValue;
                return;
            }

            var latestTimeRow =
                LstViewShots.ItemContainerGenerator.ContainerFromIndex(LstViewShots.Items.Count - 1) as ListViewItem;
            if (null == latestTimeRow)
                return;

            var latestTime = (ShotTimeRow) latestTimeRow.Content;
            latestTimeRow.Background = Brushes.LightGray;
            TxtBoxTotalTime.Text = latestTime.Time;

            // Ohh look! That's what I have to do since M$ still didn't pull their SHIT outta their ASSES
            _isTimerBlinking = true;
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
            _previousOptions = Options.Clone();
            var rc = _optionsWindow.ShowDialog();

            // Null = cancel; false = OK
            if (null == rc)
            {
                // Restore the old options when canceled
                Options.Update(_previousOptions);
            }
            else
            {
                SaveOptions();
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
        ///     Triggered when the user clicks on an HUD little icon thingy stuff.
        ///     We save the config then.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="routedEventArgs">Event</param>
        private void HudElement_Toggle(object sender, RoutedEventArgs routedEventArgs)
        {
            var control = (CheckBox) sender;
            App.Info("HUD save from {0}. Value: {1}", control.Name, control.IsChecked);
            SaveOptions();
        }

        /// <summary>
        ///     Closing the window.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void WinMainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (null != ShotTimer)
                ShotTimer.Stop();
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