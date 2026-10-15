using System.Runtime.InteropServices;

namespace SuperMetroid.DebugRunner;

/// <summary>
/// Makes the command-line tools genuinely non-interactive on Windows. The CLR retains
/// its ordinary stderr stack trace and exit code; only OS-owned modal error boxes are barred.
/// </summary>
static partial class NativeConsoleProcess
{
    /// <summary>Disables Windows error-reporting dialogs before the command dispatcher performs fallible work.</summary>
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
