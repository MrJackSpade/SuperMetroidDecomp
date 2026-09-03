using System.Text.Json;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

// The tool intentionally exposes two narrow commands instead of performing work implicitly.
// That makes Visual Studio launch profiles deterministic and gives us clean breakpoint paths:
// "room" exercises full room composition, while the two-argument form inventories everything.
// Match the two debugger-facing console hosts: missing assets must be reported on stderr, not
// handed to Windows Error Reporting as an interactive exception dialog.
if (OperatingSystem.IsWindows())
    NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);

try
{
if (args.Length == 3 && args[0].Equals("room", StringComparison.OrdinalIgnoreCase))
{
    string rawDirectory = ResolveWorkspacePath(args[1], mustAlreadyExist: true);
    string outputPath = ResolveWorkspacePath(args[2], mustAlreadyExist: false);
    RequireRawAssetDirectory(rawDirectory);
    RenderedRoom room = RoomRenderer.Render(rawDirectory, RoomRenderer.LandingSite);
    PngWriter.WriteRgba(outputPath, room.Width, room.Height, room.Pixels);
    Console.WriteLine($"Rendered {RoomRenderer.LandingSite.Name} at {room.Width}x{room.Height} to {outputPath}");
    return 0;
}

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  SuperMetroid.AssetExtractor <raw-assets-directory> <png-output-directory>");
    Console.Error.WriteLine("  SuperMetroid.AssetExtractor room <raw-assets-directory> <room.png>");
    return 2;
}

string inputDirectory = ResolveWorkspacePath(args[0], mustAlreadyExist: true);
string outputDirectory = ResolveWorkspacePath(args[1], mustAlreadyExist: false);
RequireRawAssetDirectory(inputDirectory);
Directory.CreateDirectory(outputDirectory);

// Every raw file was cut from the user's verified ROM using the annotated disassembly's
// boundaries. This loop never changes those source files; it creates replaceable previews.
var records = new List<AssetRecord>();
foreach (string path in Directory.EnumerateFiles(inputDirectory, "*.bin", SearchOption.AllDirectories).Order())
{
    string name = Path.GetFileNameWithoutExtension(path);
    byte[] stored = File.ReadAllBytes(path);
    bool isPalette = name.StartsWith("Palettes_", StringComparison.OrdinalIgnoreCase);
    TileFormat? format = ClassifyTileFormat(name);
    // The disassembly retains original compressed bytes. Detecting and expanding the stream
    // here gives the C# side exactly the bytes that the SNES would place in WRAM or VRAM.
    byte[] decoded = DecodeIfCompressed(stored, isPalette, format, out bool compressed);
    string? png = null;

    if (isPalette && decoded.Length >= 2)
    {
        // Palette previews use real BGR555 colors. Enlarging each one-pixel swatch to 8x8
        // makes all eight 16-color sub-palettes readable without modifying their order.
        IReadOnlyList<Rgba32> colors = SnesGraphics.DecodeBgr555Palette(decoded);
        byte[] indexes = Enumerable.Range(0, colors.Count).Select(i => (byte)i).ToArray();
        int width = Math.Min(16, colors.Count);
        int height = (colors.Count + width - 1) / width;
        Array.Resize(ref indexes, width * height);
        png = Path.Combine("palettes", name + ".png");
        PngWriter.WriteIndexedAsRgba(Path.Combine(outputDirectory, png), width, height, indexes, colors, scale: 8);
    }
    else if (format is not null)
    {
        // A raw graphics blob has no single "correct" color until a tilemap or spritemap
        // selects a sub-palette. These standalone sheets therefore visualize pixel indexes.
        byte[] indexes;
        int width, height, colorCount;
        if (format == TileFormat.Mode7)
        {
            indexes = SnesGraphics.DecodeMode7Tiles(decoded, 16, out width, out height);
            colorCount = 256;
        }
        else
        {
            int bits = format == TileFormat.Planar2Bpp ? 2 : 4;
            indexes = SnesGraphics.DecodePlanarTiles(decoded, bits, 16, out width, out height);
            colorCount = 1 << bits;
        }
        png = Path.Combine("tiles", name + ".png");
        PngWriter.WriteIndexedAsRgba(
            Path.Combine(outputDirectory, png), width, height, indexes,
            SnesGraphics.DiagnosticPalette(colorCount));
    }

    records.Add(new AssetRecord(
        Path.GetRelativePath(inputDirectory, path).Replace('\\', '/'),
        isPalette ? "palette" : format is null ? "data" : "tiles",
        format?.ToString(), compressed, stored.Length, decoded.Length,
        png?.Replace('\\', '/')));
}

// The manifest is deliberately human-readable: it is both an audit trail and the seed for
// replacing hardcoded filenames with generated, strongly typed asset declarations later.
var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDirectory, "manifest.json"), JsonSerializer.Serialize(records, options));
Console.WriteLine($"Inventoried {records.Count} chunks; wrote {records.Count(r => r.Png is not null)} PNG previews to {outputDirectory}");
return 0;
}
catch (Exception exception)
{
    // Preserve the full managed stack trace while guaranteeing a non-interactive failure.
    // This specifically makes path and malformed-asset errors useful from CLI and VS output.
    Console.Error.WriteLine(exception);
    return 1;
}

static string ResolveWorkspacePath(string argument, bool mustAlreadyExist)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(argument);
    if (Path.IsPathRooted(argument))
        return Path.GetFullPath(argument);

    // Respect an ordinary relative argument whenever it already resolves from the caller's
    // working directory. This preserves normal CLI behavior for developers intentionally
    // operating on some other asset folder.
    string workingDirectoryCandidate = Path.GetFullPath(argument);
    if (!mustAlreadyExist || Directory.Exists(workingDirectoryCandidate) || File.Exists(workingDirectoryCandidate))
    {
        // Output defaults named "standalone-assets/..." are repository artifacts. Let the
        // repository anchor below win for those even though an output need not exist yet.
        if (!ContainsStandaloneAssetsSegment(argument))
            return workingDirectoryCandidate;
    }

    string? repositoryRoot = FindRepositoryRoot();
    if (repositoryRoot is null)
        return workingDirectoryCandidate;

    // Visual Studio, dotnet run, and a directly executed DLL can each choose a different
    // current directory. Once the recognizable standalone-assets segment appears, discard
    // any fragile leading "../" components and anchor that suffix at the repository root.
    string normalized = argument.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    string marker = "standalone-assets" + Path.DirectorySeparatorChar;
    int markerIndex = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
    if (markerIndex >= 0)
        return Path.GetFullPath(Path.Combine(repositoryRoot, normalized[markerIndex..]));
    if (normalized.Equals("standalone-assets", StringComparison.OrdinalIgnoreCase))
        return Path.Combine(repositoryRoot, "standalone-assets");

    // A non-asset relative path keeps conventional repository-root fallback semantics only
    // when its current-directory interpretation did not exist.
    return Path.GetFullPath(Path.Combine(repositoryRoot, argument));
}

static bool ContainsStandaloneAssetsSegment(string path)
{
    string normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    return normalized.Equals("standalone-assets", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains(
            "standalone-assets" + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);
}

static string? FindRepositoryRoot()
{
    // AppContext.BaseDirectory handles a directly launched Debug/Release executable;
    // Environment.CurrentDirectory handles Visual Studio and dotnet-run profiles. Search
    // both because neither location is contractually required to contain the other.
    string[] starts = [Environment.CurrentDirectory, AppContext.BaseDirectory];
    foreach (string start in starts)
    {
        for (DirectoryInfo? directory = new(Path.GetFullPath(start)); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "csharp", "SuperMetroid.slnx")))
                return directory.FullName;
        }
    }
    return null;
}

static void RequireRawAssetDirectory(string path)
{
    if (!Directory.Exists(path))
    {
        throw new DirectoryNotFoundException(
            $"Raw asset directory was not found at '{path}'. " +
            "Run the repository extraction step first or pass its absolute directory path.");
    }

    // Fail at the command boundary with a useful diagnosis rather than several stack frames
    // later in RoomRenderer.File.ReadAllBytes. This particular file is part of every valid
    // private extraction and is required by the Landing Site launch profile.
    string landingSiteLevel = Path.Combine(path, "LevelData_LandingSite.bin");
    if (!File.Exists(landingSiteLevel))
    {
        throw new FileNotFoundException(
            $"'{path}' exists but does not contain LevelData_LandingSite.bin. " +
            "The private asset extraction is missing or incomplete.",
            landingSiteLevel);
    }
}

static byte[] DecodeIfCompressed(byte[] stored, bool palette, TileFormat? format, out bool compressed)
{
    compressed = false;
    if (!SmCompression.TryDecompress(stored, out byte[] candidate, out int consumed) || consumed != stored.Length)
        return stored;

    // Reject coincidental $FF bytes in uncompressed data by requiring the expanded result
    // to end exactly at the file boundary and align to the selected native format.
    int unit = palette ? 2 : format switch
    {
        TileFormat.Planar2Bpp => 16,
        TileFormat.Planar4Bpp => 32,
        TileFormat.Mode7 => 64,
        _ => 1,
    };
    if (candidate.Length == 0 || candidate.Length % unit != 0)
        return stored;

    compressed = true;
    return candidate;
}

static TileFormat? ClassifyTileFormat(string name)
{
    // These prefixes come from semantic labels in the annotated disassembly. Tile tables,
    // level data, music, and script streams are kept as data rather than misleading PNGs.
    bool visual = name.StartsWith("Tiles_", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("SamusTiles_", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("AnimatedTiles_", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("ItemPLMGraphics_", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("CRE_", StringComparison.OrdinalIgnoreCase);
    if (!visual || name.StartsWith("TileTables_", StringComparison.OrdinalIgnoreCase))
        return null;
    // Mode 7 uses chunky 8bpp pixels; ordinary BG3 text uses 2bpp; sprites and the other
    // background layers use the SNES 4bpp planar format.
    if (name.Contains("Mode7", StringComparison.OrdinalIgnoreCase))
        return TileFormat.Mode7;
    if (name.Contains("BG3", StringComparison.OrdinalIgnoreCase))
        return TileFormat.Planar2Bpp;
    return TileFormat.Planar4Bpp;
}

internal enum TileFormat { Planar2Bpp, Planar4Bpp, Mode7 }

internal sealed record AssetRecord(
    string Source,
    string Kind,
    string? Format,
    bool Compressed,
    int StoredBytes,
    int DecodedBytes,
    string? Png);

/// <summary>
/// Configures Windows to leave process failures in the terminal. The three flags are
/// SEM_FAILCRITICALERRORS, SEM_NOGPFAULTERRORBOX, and SEM_NOOPENFILEERRORBOX respectively.
/// The managed entry point also catches exceptions; this is a second line of defense for
/// native/runtime faults that occur outside ordinary C# exception handling.
/// </summary>
static partial class NativeConsoleProcess
{
    [LibraryImport("kernel32.dll")]
    internal static partial uint SetErrorMode(uint errorMode);
}
