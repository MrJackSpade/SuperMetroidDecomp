using System.Runtime.InteropServices;

namespace SuperMetroid.TestSupport.WindowsConsole;

/// <summary>Console diagnostics must report failures to stderr, never show native focus-stealing dialogs.</summary>
internal static partial class NativeConsoleErrors
{
    /// <summary>Sets the Win32 process error mode before any fallible console work begins.</summary>
    [LibraryImport("kernel32.dll")]
    private static partial uint SetErrorMode(uint mode);

    /// <summary>Suppresses critical-error, general-protection, and missing-file Windows dialogs.</summary>
    internal static void DisableDialogs() => SetErrorMode(ErrorModes.FailCriticalErrors | ErrorModes.NoGpFaultErrorBox | ErrorModes.NoOpenFileErrorBox);

}
