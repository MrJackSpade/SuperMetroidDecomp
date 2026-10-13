using System.Runtime.InteropServices;

namespace BuildPolicy;

/// <summary>Exercises the source-generated P/Invoke shape required by SYSLIB1054.</summary>
internal static partial class Syslib1054
{
    /// <summary>Imports the Windows uptime counter through <see cref="LibraryImportAttribute"/>.</summary>
    [LibraryImport("kernel32.dll")]
    internal static partial uint GetTickCount();
}
