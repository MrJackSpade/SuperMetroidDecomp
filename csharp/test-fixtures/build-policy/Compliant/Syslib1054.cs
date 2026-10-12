using System.Runtime.InteropServices;

namespace BuildPolicy;

internal static partial class Syslib1054
{
    [LibraryImport("kernel32.dll")]
    internal static partial uint GetTickCount();
}
