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

// WinForms normally turns exceptions from control event handlers into modal dialogs. Route
// them through the executable's console contract so failures remain copyable and searchable.
Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
Application.ThreadException += (_, eventArguments) =>
{
    Console.Error.WriteLine(eventArguments.Exception);
    Environment.ExitCode = 1;
    Application.ExitThread();
};
AppDomain.CurrentDomain.UnhandledException += (_, eventArguments) =>
{
    if (eventArguments.ExceptionObject is Exception exception)
        Console.Error.WriteLine(exception);
    else
        Console.Error.WriteLine($"Unhandled non-Exception object: {eventArguments.ExceptionObject}");
};

ApplicationConfiguration.Initialize();

try
{
    if (args.Length != 0 && args[0].Equals("--audio-audit", StringComparison.OrdinalIgnoreCase))
    {
        string[] audioRomArguments = args.Length == 2 ? [args[1]] : [];
        if (args.Length > 2)
            throw new ArgumentException("--audio-audit accepts one optional private ROM path.");
        string audioRomPath = PrivateRomPath.Resolve(audioRomArguments);
        CartridgeAudioSmokeTestResult result = CartridgeAudioSmokeTest.Run(audioRomPath);
        Console.WriteLine(
            $"Cartridge audio passed: {result.FramesGenerated} frames, " +
            $"{result.NonZeroSamples} nonzero samples, peak {result.PeakAmplitude}.");
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
    SuperMetroidGameOptions gameOptions;
    if (replay is null)
    {
        GameConfigurationFile configuration = GameConfigurationFile.LoadOrCreate(romPath);
        gameOptions = configuration.Options;
        Console.WriteLine(
            $"Loaded {GameConfigurationFile.FileName}: " +
            $"SkipOpeningCinematic={configuration.Options.SkipOpeningCinematic}, " +
            $"AudioEnabled={configuration.Options.AudioEnabled}, " +
            $"MasterVolumePercent={configuration.Options.MasterVolumePercent} " +
            $"({configuration.Path})");
    }
    else
    {
        // A replay must not inherit today's INI. Its startup option and SRAM image are part
        // of the deterministic seed, while PlayableGameControl verifies ROM identity.
        gameOptions = replay.GameOptions;
        Console.WriteLine(
            $"Replaying {Path.GetFullPath(args[1])}: {replay.ControllerInputs.Length} frames, " +
            $"SkipOpeningCinematic={gameOptions.SkipOpeningCinematic}, " +
            $"started {replay.StartedUtc:O}");
    }
    Application.Run(new GameForm(romPath, gameOptions, replay));
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
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

    private static IReadOnlyList<string> BuildBoundedSearchRoots()
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
