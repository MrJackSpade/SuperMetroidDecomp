using SuperMetroid.Tooling;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// The assembly files the audit's fixture compilations reference (#2602): the runtime's core library from the host's
/// trusted platform assemblies, and the game's simulation and import assemblies from the audit's own output directory,
/// where the build copies every project it references. Each is named by its file, never found by reading a loaded type.
/// </summary>
internal static class FixtureAssemblies
{
    /// <summary>The runtime's core library, which declares <see cref="object"/>.</summary>
    /// <exception cref="InvalidOperationException">The host publishes no single core library.</exception>
    internal static string CoreLibrary => RequiredReflection.TrustedPlatformAssemblies()
        .Single(path => string.Equals(Path.GetFileName(path), "System.Private.CoreLib.dll", StringComparison.OrdinalIgnoreCase));

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
