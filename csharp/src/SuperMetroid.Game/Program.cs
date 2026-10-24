using SuperMetroid.Diagnostics.Recording;
using SuperMetroid.Desktop.Diagnostics;
using SuperMetroid.Desktop.Window;
using SuperMetroid.Diagnostics.Configuration;

namespace SuperMetroid.Game;

/// <summary>Starts the desktop game, optional deterministic replay, and no-dialog error reporting.</summary>
internal static class Program
{
    /// <summary>Installs the Windows error policy, loads the game or replay, and reports fatal startup failures.</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        // Suppress native operating-system fault dialogs while retaining full exception text on
        // stderr. This is especially important under a debugger, where a modal native dialog can
        // otherwise steal focus from both Visual Studio and the game window.
        NativeGameProcess.SetErrorMode(
            ProcessErrorModes.FailCriticalErrors |
            ProcessErrorModes.NoGpFaultErrorBox |
            ProcessErrorModes.NoOpenFileErrorBox);

        DesktopSessionLog? sessionLog = null;
        GitHubErrorReporter? githubErrorReporter = null;
        try
        {
            // Capture startup, frame reports, worker output and fatal boundaries independently
            // of the INI/GitHub settings. AppContext.BaseDirectory is the executable location,
            // even when a shortcut or terminal launches the game from somewhere else.
            sessionLog = DesktopSessionLog.Start(AppContext.BaseDirectory);
            UnhandledExceptionConsole.SetFatalDiagnosticCheckpoint(sessionLog.Checkpoint);
            UnhandledExceptionConsole.InstallWinFormsHandlers();
            ApplicationConfiguration.Initialize();

            if (args is ["--dpi-awareness-audit"])
            {
                if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                    throw new InvalidOperationException(
                        $"ROM file dialogs require an STA entry thread; actual apartment is {Thread.CurrentThread.GetApartmentState()}.");
                if (Application.HighDpiMode != HighDpiMode.PerMonitorV2)
                    throw new InvalidOperationException($"Expected PerMonitorV2; actual DPI mode is {Application.HighDpiMode}.");
                Console.WriteLine("Game entry point: STA file-dialog thread and PerMonitorV2 DPI awareness verified.");
                return 0;
            }
            if (args.Length != 0 && args[0].StartsWith("--", StringComparison.Ordinal) &&
                !args[0].Equals("--replay", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Unknown game option. Development audits run through SuperMetroid.DesktopVerification.");
            ControllerInputRecording? replay = null;
            string[] romArguments = args;
            if (args.Length != 0 && args[0].Equals("--replay", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length is < 2 or > 3)
                {
                    throw new ArgumentException(
                        "--replay requires a .smrec path and accepts one optional private ROM path.");
                }
                replay = ControllerInputRecording.Read(args[1]);
                romArguments = args.Length == 3 ? [args[2]] : [];
            }

            using var setup = new RomSetupForm(romArguments);
            if (setup.ShowDialog() != DialogResult.OK || setup.Installation is not { } installation) return 0;
            GameConfigurationFile configuration = GameConfigurationFile.LoadInstalled(installation.Root);
            Console.WriteLine($"Configuration source: {configuration.Source}");
            Console.WriteLine($"Configuration file: {configuration.Path}");
            HostOptions hostOptions;
            if (replay is null)
            {
                hostOptions = configuration.Options;
                Console.WriteLine(
                    $"Loaded {GameConfigurationFile.FileName}: " +
                    $"SkipOpeningCinematic={configuration.Options.Game.SkipOpeningCinematic}, " +
                    $"Invincibility={configuration.Options.Game.Invincibility}, " +
                    $"InfiniteAmmo={configuration.Options.Game.InfiniteAmmo}, " +
                    $"GrantAllEquipment={configuration.Options.Game.GrantAllEquipment}, UnlockTourian={configuration.Options.Game.UnlockTourian}, " +
                    $"MapReveal={configuration.Options.Game.MapReveal}, " +
                    $"AudioEnabled={configuration.Options.AudioEnabled}, " +
                    $"MasterVolumePercent={configuration.Options.MasterVolumePercent}, " +
                    $"ReportErrorsToGitHub={configuration.Options.ReportErrorsToGitHub}, " +
                    $"GitHubErrorRepository={configuration.Options.GitHubErrorRepository} " +
                    $"({configuration.Path})");
            }
            else
            {
                // Gameplay-affecting options and SRAM belong to the deterministic replay seed.
                // GitHub publication is a host-only diagnostic side channel, so the current INI may
                // enable or redirect it without changing a single emulated frame.
                hostOptions = new HostOptions
                {
                    Game = replay.GameOptions,
                    ReportErrorsToGitHub = configuration.Options.ReportErrorsToGitHub,
                    GitHubErrorRepository = configuration.Options.GitHubErrorRepository,
                };
                Console.WriteLine(
                    $"Replaying {Path.GetFullPath(args[1])}: {replay.ControllerInputs.Length} frames, " +
                    $"SkipOpeningCinematic={hostOptions.Game.SkipOpeningCinematic}, " +
                    $"Invincibility={hostOptions.Game.Invincibility}, " +
                    $"InfiniteAmmo={hostOptions.Game.InfiniteAmmo}, " +
                    $"GrantAllEquipment={hostOptions.Game.GrantAllEquipment}, UnlockTourian={hostOptions.Game.UnlockTourian}, " +
                    $"MapReveal={hostOptions.Game.MapReveal}, " +
                    $"ReportErrorsToGitHub={hostOptions.ReportErrorsToGitHub}, " +
                    $"started {replay.StartedUtc:O}");
            }

            if (hostOptions.ReportErrorsToGitHub)
            {
                githubErrorReporter = new GitHubErrorReporter(hostOptions.GitHubErrorRepository);
                UnhandledExceptionConsole.SetRecoverableUiErrorReporter(exception =>
                    githubErrorReporter.Report(
                        exception,
                        new GitHubErrorContext("WinForms UI callback outside the emulated-frame boundary")));
                Console.WriteLine(
                    $"Recoverable errors will be deduplicated and filed in " +
                    $"{hostOptions.GitHubErrorRepository}; gameplay will attempt the next frame.");
            }
            Application.Run(new GameForm(installation, hostOptions, replay, githubErrorReporter));
        }
        catch (Exception exception)
        {
            Environment.ExitCode = UnhandledExceptionConsole.ReportAndWait(exception);
        }
        finally
        {
            UnhandledExceptionConsole.SetRecoverableUiErrorReporter(null);
            // Finish asynchronous reporting before archiving so its success/failure text is
            // included. A shutdown failure must pass through the same no-dialog boundary.
            try { githubErrorReporter?.Dispose(); }
            catch (Exception exception)
            {
                Environment.ExitCode = UnhandledExceptionConsole.ReportAndWait(exception);
            }
            finally
            {
                sessionLog?.Dispose();
                UnhandledExceptionConsole.SetFatalDiagnosticCheckpoint(null);
            }
        }

        return Environment.ExitCode;

    }
}
