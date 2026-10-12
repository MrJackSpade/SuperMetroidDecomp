using System.Runtime.InteropServices;

namespace BuildPolicy;

/// <summary>Represents syslib1054.</summary>
internal static class Syslib1054
{
    /// <summary>get tick count.</summary>
    [DllImport("kernel32.dll")]
    internal static extern uint GetTickCount();
}
