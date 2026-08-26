using SuperMetroid.RoomViewer;
using System.Runtime.InteropServices;

// WinForms supplies a convenient zero-dependency debug shell on this Windows workstation.
// The game-facing code remains in SuperMetroid.Core and has no dependency on WinForms;
// replacing this shell with SDL, MonoGame, or another host will not affect ROM decoding.
//
// SEM_FAILCRITICALERRORS prevents Windows from opening a modal critical-error box. The other
// two flags suppress the general-fault and file-open boxes which can otherwise be displayed by
// native code below the CLR. Managed exceptions still retain their full diagnostic text below;
// this changes only where the operating system reports a fatal process error.
NativeViewerProcess.SetErrorMode(
    NativeViewerProcess.SemFailCriticalErrors |
    NativeViewerProcess.SemNoGpFaultErrorBox |
    NativeViewerProcess.SemNoOpenFileErrorBox);

// WinForms normally converts exceptions thrown by control event handlers into its own modal
// dialog. Route them into the same stderr/exit-code contract as console-hosted tools instead.
// Application.ExitThread unwinds the message loop after the exception has been printed, so a
// failed frame cannot leave a half-responsive viewer process behind.
Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
Application.ThreadException += (_, eventArguments) =>
{
    Console.Error.WriteLine(eventArguments.Exception);
    Environment.ExitCode = 1;
    Application.ExitThread();
};

// A non-UI worker-thread failure cannot be recovered safely, but writing it here makes the
// complete managed exception visible before the runtime terminates the process. SetErrorMode
// above ensures that termination does not hand control to a focus-stealing Windows dialog.
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
    ViewerStartupPaths paths = ViewerStartupPaths.Resolve(args);
    Application.Run(new RoomViewerForm(paths.RawAssetDirectory, paths.RomPath));
}
catch (Exception exception)
{
    // Catch every managed startup/message-loop failure at the process boundary. ToString()
    // preserves the exception type, message, inner exception, and full stack trace, while the
    // explicit exit code keeps Visual Studio, scripts, and CI aware that the run failed.
    Console.Error.WriteLine(exception);
    return 1;
}

return Environment.ExitCode;

/// <summary>Resolved private inputs for one viewer process.</summary>
readonly record struct ViewerStartupPaths(string RawAssetDirectory, string RomPath)
{
    private static readonly string[] RomFileNames =
    [
        "Super Metroid.smc",
        "Super Metroid.sfc",
        "sm.smc",
        "sm.sfc",
    ];

    /// <summary>
    /// Supports zero arguments for ordinary F5/direct execution while preserving explicit
    /// overrides for copied workspaces and unusual ROM names.
    /// </summary>
    public static ViewerStartupPaths Resolve(string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        IReadOnlyList<string> searchRoots = BuildBoundedSearchRoots();

        return arguments.Length switch
        {
            // Normal workspace use: walk only ancestors of the process working directory
            // and executable directory. This finds the repository without crawling user
            // folders, drives, or unrelated ROM collections.
            0 => ResolveAutomatic(searchRoots),

            // A single ROM override is convenient for Visual Studio's Debug Properties.
            // A single directory override is interpreted as raw assets, retaining automatic
            // ROM lookup. The historical two-argument form remains fully supported below.
            1 when File.Exists(arguments[0]) => new ViewerStartupPaths(
                FindRawAssets(searchRoots),
                Path.GetFullPath(arguments[0])),
            1 when Directory.Exists(arguments[0]) => new ViewerStartupPaths(
                ValidateRawAssets(arguments[0]),
                FindRom(searchRoots)),
            1 => throw new FileNotFoundException(
                $"The supplied RoomViewer path does not exist: {Path.GetFullPath(arguments[0])}"),

            2 => new ViewerStartupPaths(
                ValidateRawAssets(arguments[0]),
                ValidateRom(arguments[1])),

            _ => throw new ArgumentException(
                "RoomViewer accepts no parameters, one private ROM/raw-assets path, or " +
                "two paths in the order: <standalone-assets/raw> <private ROM>.")
        };
    }

    private static ViewerStartupPaths ResolveAutomatic(IReadOnlyList<string> searchRoots) =>
        new(FindRawAssets(searchRoots), FindRom(searchRoots));

    private static string FindRawAssets(IReadOnlyList<string> searchRoots)
    {
        foreach (string root in searchRoots)
        {
            // The first form is the repository/standalone export layout. The second permits
            // launching from a copied standalone-assets directory whose root contains raw/.
            string nested = Path.Combine(root, "standalone-assets", "raw");
            if (ContainsLandingSiteAssets(nested))
                return Path.GetFullPath(nested);

            string direct = Path.Combine(root, "raw");
            if (ContainsLandingSiteAssets(direct))
                return Path.GetFullPath(direct);
        }

        throw new DirectoryNotFoundException(
            "Could not locate standalone-assets/raw automatically. RoomViewer searched only " +
            "the current/executable directories and their parents. Run AssetExtractor first, " +
            "or pass the raw directory explicitly.");
    }

    private static string FindRom(IReadOnlyList<string> searchRoots)
    {
        // An environment override avoids putting a copyrighted ROM in a copied build folder.
        // It is consulted only when it names an existing file; otherwise the error below
        // remains deterministic and explains the supported explicit argument.
        string? environmentRom = Environment.GetEnvironmentVariable("SUPERMETROID_ROM");
        if (!string.IsNullOrWhiteSpace(environmentRom) && File.Exists(environmentRom))
            return Path.GetFullPath(environmentRom);

        foreach (string root in searchRoots)
        {
            foreach (string fileName in RomFileNames)
            {
                string candidate = Path.Combine(root, fileName);
                if (File.Exists(candidate))
                    return Path.GetFullPath(candidate);
            }
        }

        throw new FileNotFoundException(
            "Could not locate the private Super Metroid ROM automatically. Put " +
            "'Super Metroid.smc' beside the workspace/standalone-assets directory, set " +
            "SUPERMETROID_ROM, or pass the ROM path as the sole argument.");
    }

    private static string ValidateRawAssets(string path)
    {
        string fullPath = Path.GetFullPath(path);
        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Raw asset directory does not exist: {fullPath}");
        if (!ContainsLandingSiteAssets(fullPath))
        {
            throw new DirectoryNotFoundException(
                $"Raw asset directory is missing LevelData_LandingSite.bin: {fullPath}");
        }
        return fullPath;
    }

    private static string ValidateRom(string path)
    {
        string fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Private ROM does not exist: {fullPath}", fullPath);
        return fullPath;
    }

    private static bool ContainsLandingSiteAssets(string directory) =>
        Directory.Exists(directory) &&
        File.Exists(Path.Combine(directory, "LevelData_LandingSite.bin"));

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

/// <summary>
/// Windows process-error policy used only by this executable host. Keeping the constants named
/// avoids opaque literals at startup and documents exactly which native dialogs are disabled.
/// </summary>
static partial class NativeViewerProcess
{
    internal const uint SemFailCriticalErrors = 0x0001;
    internal const uint SemNoGpFaultErrorBox = 0x0002;
    internal const uint SemNoOpenFileErrorBox = 0x8000;

    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
