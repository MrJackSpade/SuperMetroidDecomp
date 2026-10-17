using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.DebugRunner.Assets;
using SuperMetroid.AssetExtraction.Compression;
using SuperMetroid.AssetExtraction.ReferenceRendering;

/// <summary>Debug-runner commands for installing, extracting, previewing and replacing cartridge-derived assets.</summary>
internal static class AssetCommands
{
    /// <summary>JSON options: Indented output.</summary>
    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

/// <summary>Dispatches asset commands and reports path or malformed-asset failures to standard error with a nonzero exit code.</summary>
public static int Run(string[] args)
{
try
{
if (args.Length is 2 or 3 && args[0].Equals("install", StringComparison.OrdinalIgnoreCase))
{
    GameInstallation installed = GameAssetInstaller.Install(args[1], args.Length == 3 ? args[2] : GameAssetInstaller.DesktopRoot);
    Console.WriteLine($"ROM installed at {installed.RomPath}");
    Console.WriteLine($"Runtime audio extracted at {installed.AudioDirectory}");
    return 0;
}

if (args.Length == 3 && args[0].Equals("audio-rom", StringComparison.OrdinalIgnoreCase))
{
    using Stream source = File.OpenRead(args[1]);
    byte[] rom = SupportedCartridge.Read(source);
    AudioAssetManifest manifest = SpcAudioAssetExtractor.Extract(new CartridgeImportAddressSpace(rom), args[2]);
    Console.WriteLine($"Extracted {manifest.Uploads.Count} streams and {manifest.CanonicalSamples.Count} PCM samples.");
    return 0;
}
if (args.Length == 3 && args[0].Equals("maps", StringComparison.OrdinalIgnoreCase))
{
    string romPath = ResolveWorkspacePath(args[1], mustAlreadyExist: true);
    string mapOutputDirectory = ResolveWorkspacePath(args[2], mustAlreadyExist: false);
    int areas = MapAssetExtractor.Extract(romPath, mapOutputDirectory);
    Console.WriteLine($"Extracted {areas} area maps into {mapOutputDirectory}");
    return 0;
}

if (args.Length is 4 or 5 && args[0].Equals("audio-replace", StringComparison.OrdinalIgnoreCase))
{
    var installation = new GameInstallation(ResolveWorkspacePath(args[1], mustAlreadyExist: true));
    string replacementPath = ResolveWorkspacePath(args[3], mustAlreadyExist: true);
    long? loopStart = args.Length == 4 ? null : ParseLoopStart(args[4]);
    AudioClipOverride installed = AudioAssetOverrideInstaller.Replace(installation, args[2], replacementPath, loopStart);
    Console.WriteLine(
        $"Installed {installed.Id} from {installed.File}, loop={installed.LoopStart?.ToString() ?? "none"}; restart the game to hear it.");
    return 0;
}

if (args.Length == 2 && args[0].Equals("audio-override-init", StringComparison.OrdinalIgnoreCase))
{
    var installation = new GameInstallation(ResolveWorkspacePath(args[1], mustAlreadyExist: true));
    string overrideDirectory = AudioAssetOverrideInstaller.Initialize(installation);
    Console.WriteLine($"Audio clip override initialized at {overrideDirectory}");
    Console.WriteLine(
        "Use 'assets audio-replace <installed-app-data-directory> <clip-id> <wav> [none|loop-frame]' with a " +
        "16-bit stereo WAV at 32,040 Hz, then restart the game.");
    return 0;
}

if (args.Length == 3 && args[0].Equals("audio", StringComparison.OrdinalIgnoreCase))
{
    string rawDirectory = ResolveWorkspacePath(args[1], mustAlreadyExist: true);
    string audioDirectory = ResolveWorkspacePath(args[2], mustAlreadyExist: false);
    AudioAssetManifest manifest = SpcAudioAssetExtractorTooling.Extract(rawDirectory, audioDirectory);
    Console.WriteLine(
        $"Extracted {manifest.Uploads.Count} SPC streams and " +
        $"{manifest.Banks.Sum(bank => bank.Samples.Count)} source aliases into " +
        $"{manifest.CanonicalSamples.Count} canonical PCM WAV files at {audioDirectory}");
    return 0;
}

if (args.Length == 3 && args[0].Equals("room", StringComparison.OrdinalIgnoreCase))
{
    string rawDirectory = ResolveWorkspacePath(args[1], mustAlreadyExist: true);
    string outputPath = ResolveWorkspacePath(args[2], mustAlreadyExist: false);
    RequireRawAssetDirectory(rawDirectory);
    RenderedRoom room = RoomRenderer.Render(rawDirectory, RoomRenderer.LandingSite);
    PngWriterTooling.WriteRgba(outputPath, room.Size, room.Pixels);
    Console.WriteLine($"Rendered {RoomRenderer.LandingSite.Name} at {room.Size.X}x{room.Size.Y} to {outputPath}");
    return 0;
}

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  SuperMetroid.DebugRunner assets install <rom-path> [app-data-directory]");
    Console.Error.WriteLine("  SuperMetroid.DebugRunner assets audio-rom <rom-path> <audio-output-directory>");
    Console.Error.WriteLine("  SuperMetroid.DebugRunner assets maps <rom-path> <map-output-directory>");
    Console.Error.WriteLine("  SuperMetroid.DebugRunner assets <raw-assets-directory> <png-output-directory>");
    Console.Error.WriteLine("  SuperMetroid.DebugRunner assets audio <raw-assets-directory> <audio-output-directory>");
    Console.Error.WriteLine(
        "  SuperMetroid.DebugRunner assets audio-replace <installed-app-data-directory> <clip-id> <wav> [none|loop-frame]");
    Console.Error.WriteLine(
        "  SuperMetroid.DebugRunner assets audio-override-init <installed-app-data-directory>");
    Console.Error.WriteLine("  SuperMetroid.DebugRunner assets room <raw-assets-directory> <room.png>");
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
        IReadOnlyList<Rgba32> colors = SnesGraphicsTooling.DecodeBgr555Palette(decoded);
        byte[] indexes = Enumerable.Range(0, colors.Count).Select(i => (byte)i).ToArray();
        int width = Math.Min(16, colors.Count);
        int height = (colors.Count + width - 1) / width;
        Array.Resize(ref indexes, width * height);
        png = Path.Combine("palettes", name + ".png");
        PngWriterTooling.WriteIndexedAsRgba(Path.Combine(outputDirectory, png), new Extent16<ImagePixels>(checked((ushort)width), checked((ushort)height)), indexes, colors, scale: 8);
    }
    else if (format is not null)
    {
        // A raw graphics blob has no single "correct" color until a tilemap or spritemap
        // selects a sub-palette. These standalone sheets therefore visualize pixel indexes.
        byte[] indexes;
        Extent16<ImagePixels> size;
        int colorCount;
        if (format == TileFormat.Mode7)
        {
            indexes = SnesGraphics.DecodeMode7Tiles(decoded, 16, out size);
            colorCount = 256;
        }
        else
        {
            int bits = format == TileFormat.Planar2Bpp ? 2 : 4;
            indexes = SnesGraphics.DecodePlanarTiles(decoded, bits, 16, out size);
            colorCount = 1 << bits;
        }
        png = Path.Combine("tiles", name + ".png");
        PngWriterTooling.WriteIndexedAsRgba(
            Path.Combine(outputDirectory, png), size, indexes,
            SnesGraphics.DiagnosticPalette(colorCount));
    }

    records.Add(new AssetRecord(
        png?.Replace('\\', '/')));
}

// The manifest is deliberately human-readable: it is both an audit trail and the seed for
// replacing hardcoded filenames with generated, strongly typed asset declarations later.
var options = IndentedJson;
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

}

/// <summary>Resolves a CLI path from the caller's directory, anchoring repository standalone assets when needed.</summary>
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

/// <summary>Parses a clip override's loop argument: <c>none</c>, or the frame it loops back to.</summary>
static long? ParseLoopStart(string value)
{
    if (value.Equals("none", StringComparison.OrdinalIgnoreCase))
        return null;
    if (long.TryParse(value, out long frame) && frame >= 0)
        return frame;
    throw new ArgumentException($"Loop '{value}' must be 'none' or a non-negative frame index.");
}

/// <summary>Recognizes the repository-owned standalone-assets path segment in either slash style.</summary>
static bool ContainsStandaloneAssetsSegment(string path)
{
    string normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    return normalized.Equals("standalone-assets", StringComparison.OrdinalIgnoreCase)
        || normalized.Contains(
            "standalone-assets" + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);
}

/// <summary>Searches ancestors of both the process directory and executable directory for the solution marker.</summary>
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

/// <summary>Checks that a private extraction contains the Landing Site level data needed by room preview rendering.</summary>
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

/// <summary>Expands a full-file Super Metroid stream only when its output aligns to the selected palette or tile unit.</summary>
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

/// <summary>Uses annotated asset labels to distinguish pixel graphics from tables and select the native tile encoding.</summary>
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

/// <summary>Native decoded pixel layout used when creating standalone graphics previews.</summary>
internal enum TileFormat
{
    /// <summary>Two-bit planar HUD and BG3 text characters, sixteen bytes per tile.</summary>
    Planar2Bpp,
    /// <summary>Four-bit planar sprite and ordinary background characters, thirty-two bytes per tile.</summary>
    Planar4Bpp,
    /// <summary>Chunky eight-bit Mode 7 characters, sixty-four bytes per tile.</summary>
    Mode7,
}

/// <summary>Manifest entry carrying the optional generated PNG path for an extracted raw chunk.</summary>
/// <param name="Png">Preview path when the chunk's native layout can be rendered as image tiles.</param>
internal sealed record AssetRecord(
    string? Png);

}
