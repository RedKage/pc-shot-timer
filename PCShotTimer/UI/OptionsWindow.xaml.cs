using NAudio.CoreAudioApi;
using PCShotTimer.Core;
using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;

namespace PCShotTimer.UI
{
    /// <summary>
    ///     Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow
    {
        #region Members

        /// <summary>SoundPlayer object used to play the Beep sounds from the combobox.</summary>
        protected SoundPlayer _beepSoundPlayer = new SoundPlayer();

        /// <summary>SoundPlayer object used to play the Ready/Standby soudns from the listview.</summary>
        protected SoundPlayer _readyStandbySoundPlayer = new SoundPlayer();

        #endregion

        #region Properties

        /// <summary>The options.</summary>
        public OptionsData Options { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of OptionsWindow.
        /// </summary>
        /// <param name="options">The OptionsData to load that window with</param>
        public OptionsWindow(OptionsData options)
        {
            Options = options;
            InitializeComponent();
        }

        #endregion

        #region Events

        /// <summary>
        ///     Closing the window will be canceled, instead it will be hidden.
        ///     ShowDialog() will return null
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        /// <summary>
        ///     OK the config, ShowDialog() will return 0
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            //App.Debug("Options window: Button Okay");
        }

        /// <summary>
        ///     Cancel, ShowDialog() will return null
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            //App.Debug("Options window: Button Cancel");
        }

        /// <summary>
        ///     Triggered when the input audio device has changed.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void CbxInputDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var device = e.AddedItems[0] as MMDevice;
            if (null == device)
                return;

            var id = CbxInputDevice.SelectedIndex;
            App.Info("New audio input device selected: {0}:{1}", id, device);
        }

        /// <summary>
        ///     Triggered when a beep sound is selected. This will load it here.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Retrieve full name 
            var soundFile = CbxBeepSounds.SelectedItem as FileInfo;
            if (null == soundFile)
                return;

            // Load
            _beepSoundPlayer.SoundLocation = soundFile.FullName;
            _beepSoundPlayer.LoadAsync();
            //App.Debug("Beep sound selected: {0}", soundFile.Name);
        }

        /// <summary>
        ///     Plays the selected Beep sound.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void BtnPlaySelectedBeep_Click(object sender, RoutedEventArgs e)
        {
            if (_beepSoundPlayer.IsLoadCompleted)
                _beepSoundPlayer.Play();
        }

        /// <summary>
        ///     Click on a play button from the ready/standby listview.
        ///     Plays the related ready/standby sound to this button's "row".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlaySelectedReadyStandby_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the button: the full name is in its tag eheh
            var button = e.Source as Button;
            if (null == button)
                return;

            // Load and play
            _readyStandbySoundPlayer.SoundLocation = button.Tag.ToString();
            _readyStandbySoundPlayer.Load();
            _readyStandbySoundPlayer.Play();
        }

        /// <summary>
        ///     Triggered when a checkbox is... loaded? "BECAUSE I WAS LOADED OKAYY!!!"
        ///     Here we link a checkbox to its sound fullpath which is also stored in the OptionsData.
        ///     Here we represent the checkboxes state according to the options.
        ///     TODO Maybe I am too weak ass, but I can't figure out how to do that pure XAML style with crazy bindings.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void ChbReadyStandby_Loaded(object sender, EventArgs e)
        {
            // Retrieve the checkbox: the full name is in its tag eheh
            var checkbox = sender as CheckBox;
            if (null == checkbox)
                return;

            // Get full path
            var readyStandbyFilepath = checkbox.Tag.ToString();

            // Shorten code and make a copy
            var files = Options.SoundSelectedReadyStandbyFiles.ToArray();

            // Browse the selected sounds from the options
            foreach (var file in files)
            {
                if (readyStandbyFilepath.Equals(file, StringComparison.InvariantCultureIgnoreCase))
                {
                    checkbox.IsChecked = true;
                    //App.Debug("Selecting RUReady sound: {0}", Path.GetFileName(file));
                    break;
                }
            }
        }

        /// <summary>
        ///     Triggered when a checkbox from the Ready/Standby sound list is toggled.
        ///     Here we link a checkbox to its sound fullpath which is also stored in the OptionsData.
        ///     Here we update the data occording to the checkboxes state.
        ///     TODO Maybe I am too weak ass, but I can't figure out how to do that in pure XAML style with crazy bindings.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void ChbReadyStandby_Toggle(object sender, RoutedEventArgs e)
        {
            // Retrieve the checkbox: the full name is in its tag eheh
            var checkbox = e.Source as CheckBox;
            if (null == checkbox)
                return;

            // Get full path
            var readyStandbyFilePath = checkbox.Tag.ToString();

            // Shorten code
            var files = Options.SoundSelectedReadyStandbyFiles;

            // Checked -> add to list / Unchecked -> remove from list
            if (checkbox.IsChecked.HasValue && checkbox.IsChecked.Value)
            {
                files.Add(readyStandbyFilePath);
                //App.Debug("Adding RUReady sound: {0}", Path.GetFileName(readyStandbyFilePath));
            }
            else
            {
                files.RemoveAll(file => file.Equals(readyStandbyFilePath, StringComparison.InvariantCultureIgnoreCase));
                //App.Debug("Removing RUReady sound: {0}", Path.GetFileName(readyStandbyFilePath));
            }

            // Check whether there are no sounds left
            if (0 == files.Count)
            {
                Options.SoundPlayReadyStandby = false;
                ChkPlayReadyStandbySounds.IsEnabled = false;
            }
            else
            {
                ChkPlayReadyStandbySounds.IsEnabled = true;
            }
        }

        #endregion
    }
}