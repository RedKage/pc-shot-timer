using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using NAudio.CoreAudioApi;
using PCShotTimer.Core;

namespace PCShotTimer.UI
{
    /// <summary>
    ///     Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow
    {
        #region Members

        /// <summary>A SoundPlayer object .</summary>
        protected SoundPlayer _beepSoundPlayer = new SoundPlayer();

        #endregion

        #region Properties

        /// <summary>The options.</summary>
        public OptionsData Options { get; set; }

        /// <summary>Gets the available beep sounds from the app subfolder.</summary>
        public IList<FileInfo> AvailableBeepSounds
        {
            get
            {
                var sounds = Directory.GetFiles(string.Format(@"{0}\{1}", App.AppDirectory, App.SoundsDirecoryName));
                return sounds
                    .Select(sound => new FileInfo(sound))
                    .Where(file => file.Name.StartsWith(App.BeepSoundsPrefix))
                    .ToList();
            }
        }

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

        #region Internal Methods

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
        }

        /// <summary>
        ///     Cancel, ShowDialog() will return null
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
        /// Triggered when a beep sound is selected. This will load it here.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the Beep combo
            var beepList = sender as ComboBox;
            if (beepList == null)
                return;

            // Retrieve full name and load
            var soundFile = beepList.SelectedValue.ToString();
            _beepSoundPlayer.SoundLocation = soundFile;
            _beepSoundPlayer.LoadAsync();
            App.Info("Beep sound loading '{0}'", soundFile);
        }

        /// <summary>
        /// Plays the Beep selected sound.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void BtnPlaySelectedBeep_Click(object sender, RoutedEventArgs e)
        {
            if (_beepSoundPlayer.IsLoadCompleted)
                _beepSoundPlayer.Play();
        }

        #endregion
    }
}