using System.ComponentModel;
using System.Windows;

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

        #endregion
    }
}