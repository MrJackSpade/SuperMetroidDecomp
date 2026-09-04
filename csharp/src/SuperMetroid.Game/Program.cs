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
    if (args.Length != 0 &&
        args[0].Equals("--unhandled-exception-console-audit", StringComparison.OrdinalIgnoreCase))
    {
        if (args.Length != 1)
        {
            throw new ArgumentException(
                "--unhandled-exception-console-audit does not accept additional arguments.");
        }
        UnhandledExceptionConsoleSmokeTestResult result = UnhandledExceptionConsoleSmokeTest.Run();
        Console.WriteLine(
            $"Unhandled-exception console passed: complete {result.ReportLength}-character " +
            "diagnostic was flushed before the acknowledgment wait.");
        return 0;
    }

    if (args.Length != 0 &&
        args[0].Equals("--keyboard-input-audit", StringComparison.OrdinalIgnoreCase))
    {
        if (args.Length != 1)
            throw new ArgumentException("--keyboard-input-audit does not accept additional arguments.");
        HostKeyboardInputSmokeTestResult result = HostKeyboardInputSmokeTest.Run();
        Console.WriteLine(
            $"Keyboard input passed: Enter=${result.EnterControllerWord:X4}, " +
            $"released=${result.ReleasedControllerWord:X4}; no Right bit was emitted.");
        return 0;
    }

    if (args.Length != 0 &&
        args[0].Equals("--github-error-reporter-audit", StringComparison.OrdinalIgnoreCase))
    {
        if (args.Length != 1)
            throw new ArgumentException("--github-error-reporter-audit does not accept arguments.");
        GitHubErrorReporterSmokeTestResult result = GitHubErrorReporterSmokeTest.Run();
        Console.WriteLine(
            $"GitHub error reporter passed: {result.Fingerprint}, " +
            $"{result.RemoteLookups} distinct lookups, {result.IssuesCreated} issue created.");
        return 0;
    }

    if (args.Length != 0 &&
        args[0].Equals("--viewport-layout-audit", StringComparison.OrdinalIgnoreCase))
    {
        if (args.Length != 1)
            throw new ArgumentException("--viewport-layout-audit does not accept additional arguments.");
        HostViewportLayoutSmokeTestResult result = HostViewportLayoutSmokeTest.Run();
        Console.WriteLine(
            $"Viewport layout passed: canvas remained {result.BeforeCanvasBounds}.");
        return 0;
    }

    if (args.Length != 0 &&
        args[0].Equals("--frame-timing-audit", StringComparison.OrdinalIgnoreCase))
    {
        if (args.Length != 1)
            throw new ArgumentException("--frame-timing-audit does not accept additional arguments.");
        FrameTimingCounterSmokeTestResult result = FrameTimingCounterSmokeTest.Run();
        RgbaBitmapSmokeTestResult bitmap = RgbaBitmapSmokeTest.Run();
        Console.WriteLine(
            $"Frame timing passed: emulation {result.EmulatedFramesPerSecond:F1} fps, " +
            $"paint {result.PaintedFramesPerSecond:F1} fps, " +
            $"step {result.AverageEmulationMilliseconds:F2}/{result.WorstEmulationMilliseconds:F2} ms, " +
            $"late {result.LateFrames:F1}; persistent {bitmap.Width}x{bitmap.Height} " +
            $"bitmap replacement preserved RGBA.");
        return 0;
    }

    if (args.Length != 0 && args[0].Equals("--state-audit", StringComparison.OrdinalIgnoreCase))
    {
        string[] stateRomArguments = args.Length == 2 ? [args[1]] : [];
        if (args.Length > 2)
            throw new ArgumentException("--state-audit accepts one optional private ROM path.");
        string stateRomPath = PrivateRomPath.Resolve(stateRomArguments);
        DebuggerSaveStateSmokeTestResult result = DebuggerSaveStateSmokeTest.Run(stateRomPath);
        Console.WriteLine(
            $"Debugger state passed: frame {result.SavedFrame}, " +
            $"{result.ContinuationFrames} deterministic continuation frames, " +
            $"{result.StateFileBytes} bytes, wrong-ROM rejection={result.WrongRomRejected}.");
        return 0;
    }

    if (args.Length != 0 &&
        args[0].Equals("--audio-input-replay-audit", StringComparison.OrdinalIgnoreCase))
    {
        if (args.Length is < 3 or > 4)
        {
            throw new ArgumentException(
                "--audio-input-replay-audit requires a .smrec path and private ROM path, " +
                "then accepts one optional capture directory.");
        }
        AudioInputReplaySmokeTestResult result = AudioInputReplaySmokeTest.Run(
            args[1],
            args[2],
            args.Length == 4 ? args[3] : null);
        Console.WriteLine(
            $"Audio replay passed in {result.FramesExecuted} frames: projectile/SFX acknowledged; " +
            $"first door ${result.SourceRoom:X4} -> ${result.DestinationRoom:X4} completed.");
        return 0;
    }

    if (args.Length != 0 && args[0].Equals("--waveout-audit", StringComparison.OrdinalIgnoreCase))
    {
        if (args.Length != 1)
            throw new ArgumentException("--waveout-audit does not accept additional arguments.");
        WaveOutAudioSmokeTestResult result = WaveOutAudioSmokeTest.Run();
        Console.WriteLine(
            $"waveOut backpressure passed: {result.BuffersSubmitted} one-frame buffers " +
            $"submitted without loss in {result.SubmissionTime.TotalMilliseconds:F0} ms.");
        return 0;
    }

    if (args.Length != 0 && args[0].Equals("--audio-audit", StringComparison.OrdinalIgnoreCase))
    {
        string[] audioRomArguments = args.Length == 2 ? [args[1]] : [];
        if (args.Length > 2)
            throw new ArgumentException("--audio-audit accepts one optional private ROM path.");
        string audioRomPath = PrivateRomPath.Resolve(audioRomArguments);
        CartridgeAudioSmokeTestResult result = CartridgeAudioSmokeTest.Run(audioRomPath);
        Console.WriteLine(
            $"Cartridge audio passed: {result.FramesGenerated} frames, " +
            $"{result.NonZeroSamples} music samples, peak {result.PeakAmplitude}; " +
            $"power-beam SFX produced {result.PowerBeamNonZeroSamples} samples " +
            "and completed its request/clear handshake.");
        return 0;
    }

    if (args.Length != 0 && args[0].Equals("--managed-audio-audit", StringComparison.OrdinalIgnoreCase))
    {
        if (args.Length != 1)
            throw new ArgumentException("--managed-audio-audit does not accept a ROM path.");
        ManagedAudioRegressionSmokeTestResult result = ManagedAudioRegressionSmokeTest.Run();
        Console.WriteLine(
            $"Managed audio regression passed: {result.Scenarios} scenarios, {result.Frames} frames, " +
            $"{result.PcmSamples} samples, {result.CanonicalSamples} canonical WAVs/" +
            $"{result.SourceAliases} source aliases; PCM {result.PcmSha256}, " +
            $"acknowledgements {result.AcknowledgementSha256}.");
        return 0;
    }

    if (args.Length != 0 && args[0].Equals("--pause-audio-audit", StringComparison.OrdinalIgnoreCase))
    {
        string[] pauseAudioRomArguments = args.Length == 2 ? [args[1]] : [];
        if (args.Length > 2)
            throw new ArgumentException("--pause-audio-audit accepts one optional private ROM path.");
        string pauseAudioRomPath = PrivateRomPath.Resolve(pauseAudioRomArguments);
        PauseAudioSmokeTestResult result = PauseAudioSmokeTest.Run(pauseAudioRomPath);
        Console.WriteLine(
            $"Pause audio passed: {result.FramesGenerated} PCM frames, " +
            $"{result.PauseFrames} pause-owned frames, {result.AudioCommandCount} APU commands; " +
            $"max adjacent delta {result.MaximumAdjacentSampleDelta}, " +
            $"max frame-boundary delta {result.MaximumFrameBoundaryDelta}.");
        return 0;
    }

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

    string romPath = PrivateRomPath.Resolve(romArguments);
    GameConfigurationFile configuration = GameConfigurationFile.LoadOrCreate(romPath);
    SuperMetroidGameOptions gameOptions;
    if (replay is null)
    {
        gameOptions = configuration.Options;
        Console.WriteLine(
            $"Loaded {GameConfigurationFile.FileName}: " +
            $"SkipOpeningCinematic={configuration.Options.SkipOpeningCinematic}, " +
            $"Invincibility={configuration.Options.Invincibility}, " +
            $"InfiniteAmmo={configuration.Options.InfiniteAmmo}, " +
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
    Application.Run(new GameForm(romPath, gameOptions, replay, githubErrorReporter));
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

/// <summary>Locates the user's private cartridge image without requiring debug arguments.</summary>
static class PrivateRomPath
{
    private static readonly string[] KnownFileNames =
    [
        "Super Metroid.smc",
        "Super Metroid.sfc",
        "sm.smc",
        "sm.sfc",
    ];

    public static string Resolve(string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return arguments.Length switch
        {
            0 => FindAutomatically(),
            1 => Validate(arguments[0]),
            _ => throw new ArgumentException(
                "SuperMetroid.Game accepts either no parameters or one private ROM path."),
        };
    }

    private static string FindAutomatically()
    {
        // An environment override keeps the copyrighted cartridge image outside copied
        // workspaces while still making ordinary F5 execution parameter-free.
        string? environmentRom = Environment.GetEnvironmentVariable("SUPERMETROID_ROM");
        if (!string.IsNullOrWhiteSpace(environmentRom) && File.Exists(environmentRom))
            return Path.GetFullPath(environmentRom);

        foreach (string root in BuildBoundedSearchRoots())
        {
            foreach (string fileName in KnownFileNames)
            {
                string candidate = Path.Combine(root, fileName);
                if (File.Exists(candidate))
                    return Path.GetFullPath(candidate);
            }
        }

        throw new FileNotFoundException(
            "Could not locate the private Super Metroid ROM automatically. Put " +
            "'Super Metroid.smc' in the workspace, set SUPERMETROID_ROM, or pass the ROM " +
            "path as the sole argument.");
    }

    private static string Validate(string path)
    {
        string fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Private ROM does not exist: {fullPath}", fullPath);
        return fullPath;
    }

    private static List<string> BuildBoundedSearchRoots()
    {
        var roots = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddAncestors(Environment.CurrentDirectory, roots, seen);
        AddAncestors(AppContext.BaseDirectory, roots, seen);
        return roots;
    }

    private static void AddAncestors(
        string startingDirectory,
        List<string> roots,
        HashSet<string> seen)
    {
        DirectoryInfo? directory = new(Path.GetFullPath(startingDirectory));
        while (directory is not null)
        {
            if (seen.Add(directory.FullName))
                roots.Add(directory.FullName);
            directory = directory.Parent;
        }
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
