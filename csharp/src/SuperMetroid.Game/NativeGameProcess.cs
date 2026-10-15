using System.Runtime.InteropServices;

namespace SuperMetroid.Game;

/// <summary>Named Win32 process-error policy for the playable executable.</summary>
static partial class NativeGameProcess
{
    /// <summary>Applies the Win32 process error mode before fallible game startup work.</summary>
    [LibraryImport("kernel32.dll")]
    internal static partial ProcessErrorModes SetErrorMode(ProcessErrorModes errorMode);
}
