using SuperMetroid.IntegrationVerification.Gameplay;
using SuperMetroid.IntegrationVerification.Installation;
namespace SuperMetroid.IntegrationVerification;

using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.WindowsConsole;

/// <summary>
/// Repository-only integration checks: the gitignored ROM, its shared extracted installation and checked-in fixtures.
/// </summary>
internal static class Program
{
    /// <summary>Dispatches one repository integration check, or with no arguments all of them, under a no-dialog error boundary.</summary>
    private static int Main(string[] args)
    {
        try
        {
            // Match the existing console-host policy before import, replay or any native
            // work. Failures belong in the outer stderr/nonzero boundary, never a dialog.
            if (OperatingSystem.IsWindows())
                NativeConsoleErrors.DisableDialogs();
            // Every command reads only the repository: the gitignored ROM, its shared extracted
            // installation and checked-in fixtures. File-taking tools live in SuperMetroid.DebugRunner.
            if (args is ["--extracted-validation-contract"])
                return ExtractedInstallationValidationVerification.Run(RepositoryInstallation.Installation.Root);
            if (args is ["--map-presentation-json-contracts"])
                return MapPresentationJsonContractVerification.Run(RepositoryInstallation.Installation.Root);
            if (args is ["--projectile-host-binding"])
                return ProjectileHostBindingVerification.Run();
            // CI has no ROM: --asset-import checks only ROM-independent input validation.
            if (args is ["--asset-import"])
                return AssetImportVerification.Run(romPath: null);
            if (args is ["--asset-import-rom"])
                return AssetImportVerification.Run(Path.GetFullPath("Super Metroid.smc"));
            if (args is ["--override-installation-lifecycle"])
                return InstallationOverrideLifecycleVerification.Run(Path.GetFullPath("Super Metroid.smc"));
            if (args is ["--rom-decompression"])
                return RomDecompressionVerification.Run();
            if (args.Length != 0)
                throw new ArgumentException($"Unknown integration check {string.Join(' ', args)}.");
            // Every repository check, in turn; the first failure stops the run.
            foreach (Func<int> check in new Func<int>[]
            {
                () => ExtractedInstallationValidationVerification.Run(RepositoryInstallation.Installation.Root),
                () => MapPresentationJsonContractVerification.Run(RepositoryInstallation.Installation.Root),
                ProjectileHostBindingVerification.Run,
                () => AssetImportVerification.Run(Path.GetFullPath("Super Metroid.smc")),
                () => InstallationOverrideLifecycleVerification.Run(Path.GetFullPath("Super Metroid.smc")),
                RomDecompressionVerification.Run,
            })
            {
                int exit = check();
                if (exit != 0)
                    return exit;
            }
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }
}
