using SuperMetroid.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Desktop;
using System.Runtime.InteropServices;

// Suppress native operating-system fault dialogs while retaining full exception text on
// stderr. This is especially important under a debugger, where a modal native dialog can
// otherwise steal focus from both Visual Studio and the game window.
NativeGameProcess.SetErrorMode(
    NativeGameProcess.SemFailCriticalErrors |
    NativeGameProcess.SemNoGpFaultErrorBox |
    NativeGameProcess.SemNoOpenFileErrorBox);

// Install one shared process boundary before WinForms creates a window. UI callbacks,
// background threads, startup failures, and message-loop failures therefore all retain their
// complete console diagnostic and wait for Enter instead of disappearing immediately.
UnhandledExceptionConsole.InstallWinFormsHandlers();

ApplicationConfiguration.Initialize();

GitHubErrorReporter? githubErrorReporter = null;
try
{
    if (args is ["--dpi-awareness-audit"])
    {
        if (Application.HighDpiMode != HighDpiMode.PerMonitorV2)
            throw new InvalidOperationException($"Expected PerMonitorV2; actual DPI mode is {Application.HighDpiMode}.");
        Console.WriteLine("Game entry point: PerMonitorV2 DPI awareness verified.");
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
    string romPath = installation.RomPath;
    GameConfigurationFile configuration = GameConfigurationFile.LoadOrCreate(romPath, installation.Root);
    SuperMetroidGameOptions gameOptions;
    if (replay is null)
    {
        gameOptions = configuration.Options;
        Console.WriteLine(
            $"Loaded {GameConfigurationFile.FileName}: " +
            $"SkipOpeningCinematic={configuration.Options.SkipOpeningCinematic}, " +
            $"Invincibility={configuration.Options.Invincibility}, " +
            $"InfiniteAmmo={configuration.Options.InfiniteAmmo}, " +
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
    Application.Run(new GameForm(romPath, gameOptions, replay, githubErrorReporter, installation.AudioDirectory, installation.Root));
}
catch (Exception exception)
{
    return UnhandledExceptionConsole.ReportAndWait(exception);
}
finally
{
    UnhandledExceptionConsole.SetRecoverableUiErrorReporter(null);
    githubErrorReporter?.Dispose();
}

return Environment.ExitCode;

/// <summary>Named Win32 process-error policy for the playable executable.</summary>
static partial class NativeGameProcess
{
    internal const uint SemFailCriticalErrors = 0x0001;
    internal const uint SemNoGpFaultErrorBox = 0x0002;
    internal const uint SemNoOpenFileErrorBox = 0x8000;

    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
