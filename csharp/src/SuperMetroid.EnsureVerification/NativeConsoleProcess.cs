using System.Runtime.InteropServices;

namespace SuperMetroid.EnsureVerification;

/// <summary>Windows process-error policy used before verifier work can fail.</summary>
internal static partial class NativeConsoleProcess
{
    /// <summary>Suppresses Windows process error dialogs so failures reach the console boundary.</summary>
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
