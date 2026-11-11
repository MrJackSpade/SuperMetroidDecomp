using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One family's installed pre-coloured sprite sheets (#1392, #1633): in the family's directory, for each sheet an RGBA PNG and
/// its frame manifest, with a provenance manifest of every file's hash; and, apart from the pictures, each sheet's
/// recolour-region map in <c>recolour-regions/</c> and blend map in <c>blend-maps/</c> beside the family's directory, under a
/// folder of the family's name, where the sheet needs them. The provenance lists and counts the pictures and each kind of map
/// separately. A player edits the installed files in place.
/// </summary>
internal sealed class SpriteSheetStore
{
    /// <summary>
    /// The provenance manifest's schema revision: 2 lists each sheet's image and frame manifest, and apart from them its
    /// recolour-region and blend maps (#1633).
    /// </summary>
    private const int FormatVersion = 2;

    /// <summary>The directory of the family's pictures and frame manifests.</summary>
    private readonly string directory;

    /// <summary>The directory of the family's recolour-region maps.</summary>
    private readonly string regionDirectory;

    /// <summary>The directory of the family's blend maps.</summary>
    private readonly string blendDirectory;

    /// <summary>The provenance manifest's file name.</summary>
    private readonly string manifestFileName;

    /// <summary>Each installed picture's and frame manifest's hash, written or read.</summary>
    private readonly Dictionary<string, string> hashes;

    /// <summary>Each installed recolour-region map's hash, written or read.</summary>
    private readonly Dictionary<string, string> regionHashes;

    /// <summary>Each installed blend map's hash, written or read.</summary>
    private readonly Dictionary<string, string> blendHashes;

    /// <summary>Opens a family's store.</summary>
    private SpriteSheetStore(string directory, string manifestFileName, Dictionary<string, string> hashes,
        Dictionary<string, string> regionHashes, Dictionary<string, string> blendHashes)
    {
        this.directory = directory;
        this.manifestFileName = manifestFileName;
        this.hashes = hashes;
        this.regionHashes = regionHashes;
        this.blendHashes = blendHashes;
        string full = Path.GetFullPath(directory);
        string family = Path.GetFileName(full);
        string content = Path.GetDirectoryName(full)
            ?? throw new ArgumentException($"Sheet directory {directory} has no installation above it.", nameof(directory));
        regionDirectory = Path.Combine(content, GameInstallationLayout.RecolourRegionDirectoryName, family);
        blendDirectory = Path.Combine(content, GameInstallationLayout.BlendMapDirectoryName, family);
    }

    /// <summary>Starts writing a family's sheets, creating its directory if absent.</summary>
    internal static SpriteSheetStore Create(string directory, string manifestFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        return new(directory, manifestFileName, new(StringComparer.Ordinal), new(StringComparer.Ordinal), new(StringComparer.Ordinal));
    }

    /// <summary>Opens a family's sheets for reading, after checking its provenance manifest.</summary>
    /// <param name="directory">The family's installed pictures and frame manifests.</param>
    /// <param name="manifestFileName">The provenance manifest's file name.</param>
    /// <param name="sheetCount">The sheets the directory holds, an image and a frame manifest each.</param>
    /// <exception cref="InvalidDataException">The manifest does not describe this installation's sheets.</exception>
    internal static SpriteSheetStore Open(string directory, string manifestFileName, int sheetCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, manifestFileName);
        Provenance manifest = JsonSerializer.Deserialize<Provenance>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Sprite sheet manifest {manifestPath} is empty.");
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase) ||
            manifest.Sha256 is null || manifest.Sha256.Count != sheetCount * 2 ||
            manifest.RecolourRegionSha256 is null || manifest.BlendMapSha256 is null)
            throw new InvalidDataException($"Sprite sheet manifest {manifestPath} does not describe this installation.");
        return new(directory, manifestFileName, manifest.Sha256, manifest.RecolourRegionSha256, manifest.BlendMapSha256);
    }

    /// <summary>
    /// Writes every sheet: their images and manifests are encoded in parallel, the slow part (PNG compression), each
    /// worker reusing one encoder's buffers for every sheet it encodes, and written in the given order, so the files and
    /// the provenance manifest are the same as writing them one by one.
    /// </summary>
    internal void WriteAll(IReadOnlyList<NamedComposedSheet> sheets)
    {
        Ensure.NotNull(sheets);
        var encoded = new EncodedSpriteSheet[sheets.Count];
        Parallel.For(0, sheets.Count, () => new SpriteSheetEncoder(), (index, _, encoder) =>
        {
            ComposedSpriteSheet sheet = Ensure.NotNull(sheets[index].Sheet);
            encoded[index] = encoder.Encode(new SpriteSheetPlanes(sheet.Size, sheet.Pixels, sheet.PaletteIndices),
                sheet.Manifest);
            return encoder;
        }, encoder => encoder.Dispose());
        for (int index = 0; index < sheets.Count; index++)
            Write(sheets[index].Name, encoded[index]);
    }

    /// <summary>Writes one encoded sheet's files and records their hashes.</summary>
    /// <param name="name">The sheet's installed name.</param>
    /// <param name="sheet">Its encoded files.</param>
    internal void Write(string name, EncodedSpriteSheet sheet)
    {
        Ensure.NotNull(sheet);
        WriteFile(directory, ImageFile(name), sheet.Image, hashes);
        WriteFile(directory, FrameFile(name), sheet.Frames, hashes);
        if (sheet.RecolourRegions is { } regions)
            WriteFile(regionDirectory, RecolourRegionFile(name), regions, regionHashes);
        if (sheet.BlendMap is { } blending)
            WriteFile(blendDirectory, BlendMapFileName(name), blending, blendHashes);
    }

    /// <summary>Writes the provenance manifest of every file written.</summary>
    internal void Finish(string sourceCartridgeSha256) =>
        File.WriteAllBytes(Path.Combine(directory, manifestFileName), JsonSerializer.SerializeToUtf8Bytes(
            new Provenance(FormatVersion, sourceCartridgeSha256, hashes, regionHashes, blendHashes), JsonOptions));

    /// <summary>Loads one sheet from its installed files, as the player may have edited them.</summary>
    /// <exception cref="InvalidDataException">The manifest omits a file, or the image header or the frame manifest is invalid.</exception>
    internal SpriteSheet Read(string name)
    {
        byte[] frames = Installed(directory, FrameFile(name), hashes);
        // The image's size is read now; its pixels and maps are decoded when it is first drawn (#1430).
        Extent16<ImagePixels> size;
        try
        {
            using FileStream header = File.OpenRead(Path.Combine(directory, ImageFile(name)));
            size = RgbaPng.ReadSize(header);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        return SpriteSheetFile.Load(new MemoryStream(frames, writable: false), name, size, document =>
        {
            SpriteImagePixels pixels = Decode(name, document, out Extent16<ImagePixels> decoded);
            if (decoded != size)
                throw new InvalidDataException($"Sprite sheet {ImageFile(name)} changed size after its header was read.");
            return pixels;
        });
    }

    /// <summary>Reads one sheet's image and the maps its manifest says it has.</summary>
    /// <exception cref="InvalidDataException">The provenance omits a file, the image is malformed, or a map is missing or malformed.</exception>
    private SpriteImagePixels Decode(string name, InstalledSheetDocument document, out Extent16<ImagePixels> size)
    {
        byte[] image = Installed(directory, ImageFile(name), hashes);
        RgbaPixel[] pixels;
        try
        {
            pixels = RgbaPng.Read(new MemoryStream(image, writable: false), out size);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        int[]? regions = document.RecolourRegions
            ? RecolourRegionMapFile.Decode(Installed(regionDirectory, RecolourRegionFile(name), regionHashes), size, RecolourRegionFile(name))
            : null;
        bool[]? blending = document.BlendMap
            ? BlendMapFile.Decode(Installed(blendDirectory, BlendMapFileName(name), blendHashes), size, BlendMapFileName(name))
            : null;
        return new SpriteImagePixels(pixels, regions, blending);
    }

    /// <summary>A sheet's image file name.</summary>
    internal static string ImageFile(string name) => name + ".png";

    /// <summary>A sheet's frame manifest file name.</summary>
    internal static string FrameFile(string name) => name + ".json";

    /// <summary>A sheet's recolour-region map file name (<see cref="RecolourRegionMapFile"/>).</summary>
    internal static string RecolourRegionFile(string name) => name + RecolourRegionMapFile.Extension;

    /// <summary>A sheet's blend map file name (<see cref="BlendMapFile"/>).</summary>
    internal static string BlendMapFileName(string name) => name + BlendMapFile.Extension;

    /// <summary>Writes a new installed file into <paramref name="folder"/> and records its hash in <paramref name="listed"/>.</summary>
    private static void WriteFile(string folder, string name, byte[] bytes, Dictionary<string, string> listed)
    {
        Directory.CreateDirectory(folder);
        using (var file = new FileStream(Path.Combine(folder, name), FileMode.CreateNew, FileAccess.Write))
            file.Write(bytes);
        listed.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>
    /// An installed file as the installation holds it. A file a player edited in place is used as edited (#1532); it fails
    /// only where it is decoded, when it does not decode.
    /// </summary>
    private byte[] Installed(string folder, string name, Dictionary<string, string> listed)
    {
        if (!listed.ContainsKey(name))
            throw new InvalidDataException($"Sprite sheet manifest {Path.Combine(directory, manifestFileName)} omits {Path.Combine(folder, name)}.");
        return File.ReadAllBytes(Path.Combine(folder, name));
    }

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>The stock files' provenance: schema, cartridge, and each file's hash, the pictures apart from each kind of map.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="SourceCartridgeSha256">The cartridge the files were extracted from.</param>
    /// <param name="Sha256">Each picture's and frame manifest's hash, by file name.</param>
    /// <param name="RecolourRegionSha256">Each recolour-region map's hash, by file name.</param>
    /// <param name="BlendMapSha256">Each blend map's hash, by file name.</param>
    private sealed record Provenance(int Version, string SourceCartridgeSha256, Dictionary<string, string> Sha256,
        Dictionary<string, string> RecolourRegionSha256, Dictionary<string, string> BlendMapSha256);
}
