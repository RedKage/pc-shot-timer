using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace PCShotTimer.Core
{
    /// <summary>
    ///     Enables real console output while being a GUI win app.
    ///     Improved a little bit the version from
    ///     http://stackoverflow.com/questions/160587/no-output-to-console-from-a-wpf-application
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static class ConsoleManager
    {
        #region Members

        private const string Kernel32DllName = "kernel32.dll";

        [DllImport(Kernel32DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32DllName)]
        private static extern bool AttachConsole(int processId);

        [DllImport(Kernel32DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32DllName)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(Kernel32DllName)]
        private static extern int GetConsoleOutputCP();

        #endregion

        #region Public methods

        /// <summary>Gets whether a console exits.</summary>
        public static bool HasConsole
        {
            get { return GetConsoleWindow() != IntPtr.Zero; }
        }

        /// <summary>
        ///     Creates a new console instance if the process is not attached to a console already.
        /// </summary>
        public static void Create()
        {
            if (HasConsole)
                return;
            
            AllocConsole();
            InvalidateOutAndError();
        }

        /// <summary>
        ///     Attach the console to the parent one, if it exists.
        /// </summary>
        public static void Attach()
        {
            if (HasConsole)
                return;

            // Try to attach to the parent console
            AttachConsole(-1);
            InvalidateOutAndError();
        }

        /// <summary>
        ///     If the process has a console attached to it, it will be detached and no longer visible. Writing to the
        ///     System.Console is still possible, but no output will be shown.
        /// </summary>
        public static void Hide()
        {
            if (!HasConsole)
                return;

            SetOutAndErrorNull();
            FreeConsole();
        }

        /// <summary>
        ///     Toggle the visibility of the console.
        /// </summary>
        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Create();
            }
        }

        #endregion

        #region Internals

        /// <summary>
        ///     Removes the standard redirections.
        /// </summary>
        internal static void InvalidateOutAndError()
        {
            // ReSharper disable InconsistentNaming
            var type = typeof (Console);

            var stdOut = type.GetField("_out",
                BindingFlags.Static | BindingFlags.NonPublic);

            var stdErr = type.GetField("_error",
                BindingFlags.Static | BindingFlags.NonPublic);

            var initializeStdOutError = type.GetMethod("InitializeStdOutError",
                BindingFlags.Static | BindingFlags.NonPublic);

            Debug.Assert(stdOut != null);
            Debug.Assert(stdErr != null);
            Debug.Assert(initializeStdOutError != null);

            stdOut.SetValue(null, null);
            stdErr.SetValue(null, null);

            initializeStdOutError.Invoke(null, new object[] {true});
            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        ///     Prevent any output being done.
        /// </summary>
        internal static void SetOutAndErrorNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }

        #endregion
    }
}