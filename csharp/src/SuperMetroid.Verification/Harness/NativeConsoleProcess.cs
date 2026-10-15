using System.Runtime.InteropServices;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Windows process policy used only by this console host. `SEM_FAILCRITICALERRORS`,
/// `SEM_NOGPFAULTERRORBOX`, and `SEM_NOOPENFILEERRORBOX` keep failures non-interactive;
/// .NET still writes the exception and stack trace to stderr and returns a failing code.
/// </summary>
internal static partial class NativeConsoleProcess
{
    /// <summary>Installs Windows' noninteractive error reporting flags for this console host.</summary>
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
