using System.Runtime.InteropServices;

namespace BuildPolicy;

internal static class Syslib1054
{
    [DllImport("kernel32.dll")]
    internal static extern uint GetTickCount();
}
