using System.Runtime.InteropServices;

namespace BuildPolicy;

/// <summary>SYSLIB1054 violation fixture using DllImport instead of LibraryImport.</summary>
internal static class Syslib1054
{
    /// <summary>Imports the Windows tick counter through the legacy interop attribute under test.</summary>
    [DllImport("kernel32.dll")]
    internal static extern uint GetTickCount();
}
