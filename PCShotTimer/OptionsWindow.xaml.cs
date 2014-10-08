using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using NAudio.CoreAudioApi;

namespace PCShotTimer
{
    /// <summary>
    ///     Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow
    {
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
            App.Info(@"New audio input device selected: {0}:{1}", id, device);
        }

        #endregion
    }
}