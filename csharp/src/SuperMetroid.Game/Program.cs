using SuperMetroid.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Desktop;
using System.Runtime.InteropServices;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        // Suppress native operating-system fault dialogs while retaining full exception text on
        // stderr. This is especially important under a debugger, where a modal native dialog can
        // otherwise steal focus from both Visual Studio and the game window.
        NativeGameProcess.SetErrorMode(
            NativeGameProcess.SemFailCriticalErrors |
            NativeGameProcess.SemNoGpFaultErrorBox |
            NativeGameProcess.SemNoOpenFileErrorBox);

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
            SuperMetroidGameOptions gameOptions;
            if (replay is null)
            {
                gameOptions = configuration.Options;
                Console.WriteLine(
                    $"Loaded {GameConfigurationFile.FileName}: " +
                    $"SkipOpeningCinematic={configuration.Options.SkipOpeningCinematic}, " +
                    $"Invincibility={configuration.Options.Invincibility}, " +
                    $"InfiniteAmmo={configuration.Options.InfiniteAmmo}, " +
                    $"GrantAllEquipment={configuration.Options.GrantAllEquipment}, UnlockTourian={configuration.Options.UnlockTourian}, " +
                    $"MapReveal={configuration.Options.MapReveal}, " +
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
                gameOptions = replay.GameOptions with
                {
                    ReportErrorsToGitHub = configuration.Options.ReportErrorsToGitHub,
                    GitHubErrorRepository = configuration.Options.GitHubErrorRepository,
                };
                Console.WriteLine(
                    $"Replaying {Path.GetFullPath(args[1])}: {replay.ControllerInputs.Length} frames, " +
                    $"SkipOpeningCinematic={gameOptions.SkipOpeningCinematic}, " +
                    $"Invincibility={gameOptions.Invincibility}, " +
                    $"InfiniteAmmo={gameOptions.InfiniteAmmo}, " +
                    $"GrantAllEquipment={gameOptions.GrantAllEquipment}, UnlockTourian={gameOptions.UnlockTourian}, " +
                    $"MapReveal={gameOptions.MapReveal}, " +
                    $"ReportErrorsToGitHub={gameOptions.ReportErrorsToGitHub}, " +
                    $"started {replay.StartedUtc:O}");
            }

            if (gameOptions.ReportErrorsToGitHub)
            {
                githubErrorReporter = new GitHubErrorReporter(gameOptions.GitHubErrorRepository);
                UnhandledExceptionConsole.SetRecoverableUiErrorReporter(exception =>
                    githubErrorReporter.Report(
                        exception,
                        new GitHubErrorContext("WinForms UI callback outside the emulated-frame boundary")));
                Console.WriteLine(
                    $"Recoverable errors will be deduplicated and filed in " +
                    $"{gameOptions.GitHubErrorRepository}; gameplay will attempt the next frame.");
            }
            Application.Run(new GameForm(installation, gameOptions, replay, githubErrorReporter));
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

/// <summary>Named Win32 process-error policy for the playable executable.</summary>
static partial class NativeGameProcess
{
    internal const uint SemFailCriticalErrors = 0x0001;
    internal const uint SemNoGpFaultErrorBox = 0x0002;
    internal const uint SemNoOpenFileErrorBox = 0x8000;

    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
