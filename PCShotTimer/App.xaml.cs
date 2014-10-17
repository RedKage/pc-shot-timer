using NAudio.CoreAudioApi;
using NAudio.Wave;
using PCShotTimer.Core;
using PCShotTimer.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace PCShotTimer
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region Members

        /// <summary>Resource file name for the default embedded config.</summary>
        private const string DefaultConfigResourceName = "defaultconfig.xml";

        /// <summary>File name for the user config saved on disk.</summary>
        public const string UserConfigFileName = "config.xml";

        /// <summary>Subdirectory in the app folder that contains the sound files.</summary>
        public const string SoundsDirecoryName = "sounds";

        /// <summary>How the beep sound files shall be named.</summary>
        public const string BeepSoundsPrefix = "Beep_";

        /// <summary>How the ready/standby sound files shall be named.</summary>
        public const string ReadyStandbySoundsPrefix = "ReadyStandby_";

        /// <summary>The App assembly.</summary>
        private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        /// <summary>Location the assembly directory.</summary>
        public static string AppDirectory = Path.GetDirectoryName(Assembly.Location);

        #endregion

        #region Properties

        /// <summary>Gets the application name with its version.</summary>
        public static string AppTitle
        {
            get { return string.Format("{0} {1}", Assembly.GetName().Name, Assembly.GetName().Version); }
        }

        /// <summary>Gets the default config of this app from the app embedded resources.</summary>
        public static Stream DefaultConfig
        {
            get { return GetResource(DefaultConfigResourceName); }
        }

        /// <summary>Gets the available audio input devices.</summary>
        public IList<MMDevice> InputDevices
        {
            get { return GetAudioInputDevices(); }
        }

        #endregion

        #region Constructor

        /// <summary>
        ///     Entry point for the app.
        /// </summary>
        public App()
        {
#if DEBUG
            ConsoleManager.Create();
#else
            ConsoleManager.Attach();
#endif
            Console.Out.WriteLine();
            Info(AppTitle);

            try
            {
                // Open main window
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception e)
            {
                Error("Exception caught from App");
                DialogFatalError(e);
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        ///     Get the available audio input devices.
        ///     Thought that would be trivial shit, but this wasn't.
        ///     - We use NAudio to record the audio
        ///     - NAudio has its own audio device index
        ///     - NAudio can't get the full device name because of a M$ limitation
        ///     - So we use MMDevice instead to enumerate them devices
        ///     - However their order/index is different
        ///     - But they have a full device name
        ///     - And also they have the proper number of audio channels
        ///     - And basically MMDevice has more stuff in there
        ///     - So we use a MMDevice list of the available devices
        ///     - But we order them using the NAudio index
        ///     Also note that the use of MMDevice is only okay for Win Vista+ OSes.
        ///     No XP here because of that...
        /// </summary>
        /// <returns>A list of the available audio devices ordered by NAudio device index.</returns>
        private static IList<MMDevice> GetAudioInputDevices()
        {
            Info("Detected input devices: {0}", WaveIn.DeviceCount);

            // Enumerates audio input devices using MMDevice
            var devices = new MMDevice[WaveIn.DeviceCount];
            var enumerator = new MMDeviceEnumerator();
            foreach (var deviceInfo in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                // MMDevice has the full product name in that property
                var fullProductName = deviceInfo.FriendlyName;

                // Find this device's Index for NAudio's WaveIn
                var naudioIndex = 0;
                for (var waveInDeviceIndex = 0; waveInDeviceIndex < WaveIn.DeviceCount; waveInDeviceIndex++)
                {
                    // WaveIn only has 32bits product name lenght
                    var partialProductName = WaveIn.GetCapabilities(waveInDeviceIndex).ProductName;

                    // Okay, same device, so we grab NAudio's index
                    if (fullProductName.StartsWith(partialProductName))
                    {
                        naudioIndex = waveInDeviceIndex;
                        break;
                    }
                }

                Info("{0}:{1}, {2} channels", naudioIndex, deviceInfo.FriendlyName,
                    deviceInfo.AudioEndpointVolume.Channels.Count);
                devices[naudioIndex] = deviceInfo;
            }

            return devices.ToList();
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Get an embedded resource from the app.
        /// </summary>
        /// <param name="resourceName">The resource name. Not its full name, just it's path from /resources.</param>
        /// <returns>A stream on the wanted resource.</returns>
        public static Stream GetResource(string resourceName)
        {
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var name in names)
            {
                if (name.EndsWith(resourceName, StringComparison.InvariantCultureIgnoreCase))
                    return Assembly.GetManifestResourceStream(name);
            }
            return null;
        }

        /// <summary>
        ///     An error msgbox providing the user the option to continue the execution or the program.
        /// </summary>
        /// <param name="message">Explanation.</param>
        public static void DialogContinue(string message)
        {
            Error(message);
            message = String.Format("{0}\n{1}", message, "Do you want to continue the program execution?");
            var rc = MessageBox.Show(message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (MessageBoxResult.No == rc)
            {
                Current.Shutdown(1);
            }
        }

        /// <summary>
        ///     An error msgbox.
        /// </summary>
        /// <param name="message">Explanation.</param>
        public static void DialogWarning(string message)
        {
            Error(message);
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        ///     Fatal error msgbox and exits.
        /// </summary>
        /// <param name="exception">The exception responsible.</param>
        public static void DialogFatalError(Exception exception)
        {
            var m = new StringBuilder(String.Format("{0}:\n{1}", exception.GetType().Name, exception.Message));

            // ReSharper disable PossibleNullReferenceException
            if (typeof (FileNotFoundException) == exception.GetType())
            {
                var e = exception as FileNotFoundException;

                m.AppendFormat("\n{0}", e.FileName);
            }
            // ReSharper restore PossibleNullReferenceException

            var message = m.ToString();
            Error(message);
            Debug(message);
            MessageBox.Show(message, "Fatal error", MessageBoxButton.OK, MessageBoxImage.Stop);
            Current.Shutdown(1);
        }

        /// <summary>
        ///     Writes on the console an INFO message.
        /// </summary>
        /// <param name="stuff">Stuff to write, or String.Format string if objects are provided.</param>
        /// <param name="objects">
        ///     When present, the first arg is considered being a String.Format format string, and those guys are
        ///     the objects to inject in the string.
        /// </param>
        public static void Info(string stuff, params object[] objects)
        {
            if (!ConsoleManager.HasConsole)
                return;

            if (objects.Length >= 1)
                Console.Out.WriteLine(String.Format(@"[INFO ] {0}", stuff), objects);
            else
                Console.Out.WriteLine(@"[INFO ] {0}", stuff);
        }

        /// <summary>
        ///     Writes on the console an ERROR message.
        /// </summary>
        /// <param name="stuff">Stuff to write, or String.Format string if objects are provided.</param>
        /// <param name="objects">
        ///     When present, the first arg is considered being a String.Format format string, and those guys are
        ///     the objects to inject in the string.
        /// </param>
        public static void Error(string stuff, params object[] objects)
        {
            if (!ConsoleManager.HasConsole)
                return;

            if (objects.Length >= 1)
                Console.Error.WriteLine(String.Format(@"[ERROR] {0}", stuff), objects);
            else
                Console.Error.WriteLine(@"[ERROR] {0}", stuff);
        }

        /// <summary>
        ///     Writes on the console an DEBUG message.
        /// </summary>
        /// <param name="stuff">Stuff to write, or String.Format string if objects are provided.</param>
        /// <param name="objects">
        ///     When present, the first arg is considered being a String.Format format string, and those guys are
        ///     the objects to inject in the string.
        /// </param>
        public static void Debug(string stuff, params object[] objects)
        {
#if DEBUG
            if (!ConsoleManager.HasConsole)
                return;

            if (objects.Length >= 1)
                Console.Error.WriteLine(String.Format(@"[DEBUG] {0}", stuff), objects);
            else
                Console.Error.WriteLine(@"[DEBUG] {0}", stuff);
#endif
        }

        #endregion
    }
}