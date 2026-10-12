using System.Runtime.InteropServices;

namespace BuildPolicy;

/// <summary>Represents syslib1054.</summary>
internal static partial class Syslib1054
{
    /// <summary>get tick count.</summary>
    [LibraryImport("kernel32.dll")]
    internal static partial uint GetTickCount();
}
