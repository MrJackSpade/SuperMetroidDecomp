using System.Runtime.InteropServices;

namespace SuperMetroid.ResourceAudit;

/// <summary>Win32 process policy for this console host; keeps OS-owned modal error boxes away.</summary>
internal static partial class NativeConsoleProcess
{
    /// <summary>Sets Win32 error mode so audit failures reach stderr rather than an operating-system dialog.</summary>
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint mode);
}
