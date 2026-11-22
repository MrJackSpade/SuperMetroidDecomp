namespace SuperMetroid.ResourceAudit;

/// <summary>
/// The assembly files the audit's fixture compilations reference (#2602): the runtime's core library from the running
/// runtime's directory, and the game's simulation and import assemblies from the audit's own output directory,
/// where the build copies every project it references. Each is named by its file, never found by reading a loaded type.
/// </summary>
internal static class FixtureAssemblies
{
    /// <summary>The runtime's core library, which declares <see cref="object"/>, in the running runtime's directory.</summary>
    /// <exception cref="FileNotFoundException">The runtime's directory holds no core library.</exception>
    internal static string CoreLibrary
    {
        get
        {
            string path = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "System.Private.CoreLib.dll");
            return File.Exists(path) ? path : throw new FileNotFoundException("The runtime's directory holds no core library.", path);
        }
    }

    /// <summary>The game's simulation assembly, SuperMetroid.Core.</summary>
    /// <exception cref="FileNotFoundException">The audit's output directory holds no simulation assembly.</exception>
    internal static string Core => Beside("SuperMetroid.Core.dll");

    /// <summary>The game's import assembly, SuperMetroid.AssetExtraction.</summary>
    /// <exception cref="FileNotFoundException">The audit's output directory holds no import assembly.</exception>
    internal static string AssetExtraction => Beside("SuperMetroid.AssetExtraction.dll");

    /// <summary>The file named <paramref name="file"/> in the audit's output directory, which must exist.</summary>
    private static string Beside(string file)
    {
        string path = Path.Combine(AppContext.BaseDirectory, file);
        return File.Exists(path) ? path : throw new FileNotFoundException($"The audit's output directory holds no {file}.", path);
    }
}
