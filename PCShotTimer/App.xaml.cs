using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace PCShotTimer
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>File name for the dafault config.</summary>
        public const string DefaultConfigFileName = "defaultconfig.xml";

        /// <summary>Location the assembly directory.</summary>
        public static string AppDirectory =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", "");

        /// <summary>
        ///     Entry point for the app.
        /// </summary>
        public App()
        {
            // Unhandled exceptions
            DispatcherUnhandledException += UnhandledException;

            try
            {
                // Open main window
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception e)
            {
                // OMFG11!!11!
                FatalError(e);
            }
        }

        /// <summary>
        ///     Triggered when unhandled exceptions occurs.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event</param>
        private void UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            FatalError(e.Exception);
        }

        /// <summary>
        ///     Fatal error msgbox and exits
        /// </summary>
        /// <param name="exception">The exception responsible.</param>
        private void FatalError(Exception exception)
        {
            MessageBox.Show(exception.Message, "Fatal error", MessageBoxButton.OK, MessageBoxImage.Stop);
            Shutdown(1);
        }
    }
}