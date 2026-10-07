internal static partial class Program
{
    /// <summary>
    /// The retail cartridge in the repository root. Verifiers read it, and the shared extracted
    /// installation (<see cref="RepositoryInstallation"/>), rather than taking input paths.
    /// </summary>
    private static string RepositoryRomPath => Path.GetFullPath("Super Metroid.smc");
}
