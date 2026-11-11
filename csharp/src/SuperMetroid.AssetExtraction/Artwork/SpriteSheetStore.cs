using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One directory of installed pre-coloured sprite sheets (#1392): for each sheet an RGBA PNG, its palette map and its
/// frame manifest, with a provenance manifest of their hashes. A player edits the installed files in place.
/// </summary>
internal sealed class SpriteSheetStore
{
    /// <summary>The provenance manifest's schema revision.</summary>
    private const int FormatVersion = 1;

    /// <summary>The directory's installed files.</summary>
    private readonly string directory;

    /// <summary>The provenance manifest's file name.</summary>
    private readonly string manifestFileName;

    /// <summary>Each installed file's hash, written or read.</summary>
    private readonly Dictionary<string, string> hashes;

    /// <summary>Opens a directory's store.</summary>
    private SpriteSheetStore(string directory, string manifestFileName, Dictionary<string, string> hashes)
    {
        this.directory = directory;
        this.manifestFileName = manifestFileName;
        this.hashes = hashes;
    }

    /// <summary>Starts writing a directory's sheets, creating it if absent.</summary>
    internal static SpriteSheetStore Create(string directory, string manifestFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        return new(directory, manifestFileName, new(StringComparer.Ordinal));
    }

    /// <summary>Opens a directory's sheets for reading, after checking its provenance manifest.</summary>
    /// <param name="directory">The installed files.</param>
    /// <param name="manifestFileName">The provenance manifest's file name.</param>
    /// <param name="sheetCount">The sheets the directory holds, three files each.</param>
    /// <exception cref="InvalidDataException">The manifest does not describe this installation's sheets.</exception>
    internal static SpriteSheetStore Open(string directory, string manifestFileName, int sheetCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, manifestFileName);
        Provenance manifest = JsonSerializer.Deserialize<Provenance>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Sprite sheet manifest {manifestPath} is empty.");
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase) ||
            manifest.Sha256 is null || manifest.Sha256.Count != sheetCount * 3)
            throw new InvalidDataException($"Sprite sheet manifest {manifestPath} does not describe this installation.");
        return new(directory, manifestFileName, manifest.Sha256);
    }

    /// <summary>
    /// Writes every sheet: their images and manifests are encoded in parallel, the slow part (PNG compression), each
    /// worker reusing one encoder's buffers for every sheet it encodes, and written in the given order, so the files and
    /// the provenance manifest are the same as writing them one by one.
    /// </summary>
    /// <param name="sheets">The sheets, by name.</param>
    /// <param name="pictures">The installation's rendered pictures, which refuse a picture rendered twice (#1635).</param>
    internal void WriteAll(IReadOnlyList<NamedComposedSheet> sheets, SheetPictureRegistry pictures)
    {
        Ensure.NotNull(sheets);
        Ensure.NotNull(pictures);
        var encoded = new EncodedSpriteSheet[sheets.Count];
        Parallel.For(0, sheets.Count, () => new SpriteSheetEncoder(pictures), (index, _, encoder) =>
        {
            ComposedSpriteSheet sheet = Ensure.NotNull(sheets[index].Sheet);
            encoded[index] = encoder.Encode(new SpriteSheetPlanes(sheet.Size, sheet.Pixels, sheet.PaletteIndices, sheet.FrontTiles),
                sheet.Manifest, sheets[index].Name);
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
        WriteFile(ImageFile(name), sheet.Image);
        WriteFile(PaletteFile(name), sheet.Palette);
        WriteFile(FrameFile(name), sheet.Frames);
    }

    /// <summary>Writes the provenance manifest of every file written.</summary>
    internal void Finish(string sourceCartridgeSha256) =>
        File.WriteAllBytes(Path.Combine(directory, manifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new Provenance(FormatVersion, sourceCartridgeSha256, hashes), JsonOptions));

    /// <summary>Loads one sheet from its installed files, as the player may have edited them.</summary>
    /// <exception cref="InvalidDataException">The manifest omits a file, or the image header or the frame manifest is invalid.</exception>
    internal SpriteSheet Read(string name)
    {
        byte[] frames = Installed(FrameFile(name));
        // The image's size is read now; its pixels are decoded when it is first drawn (#1430).
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
        return SpriteSheetFile.Load(new MemoryStream(frames, writable: false), name, size, () =>
        {
            SpriteImageLayers layers = Decode(name, out Extent16<ImagePixels> decoded);
            if (decoded != size)
                throw new InvalidDataException($"Sprite sheet {ImageFile(name)} changed size after its header was read.");
            return layers;
        });
    }

    /// <summary>Reads one sheet's image and palette map into its layers.</summary>
    /// <exception cref="InvalidDataException">The manifest omits a file, an image is malformed, or the two images differ in size.</exception>
    private SpriteImageLayers Decode(string name, out Extent16<ImagePixels> size)
    {
        byte[] image = Installed(ImageFile(name));
        byte[] palette = Installed(PaletteFile(name));
        RgbaPixel[] pixels, map;
        Extent16<ImagePixels> mapSize;
        try
        {
            pixels = RgbaPng.Read(new MemoryStream(image, writable: false), out size);
            map = RgbaPng.Read(new MemoryStream(palette, writable: false), out mapSize);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        if (mapSize != size)
            throw new InvalidDataException($"Sprite sheet {PaletteFile(name)} is not the size of {ImageFile(name)}.");
        var inks = new SpriteImageInk[map.Length];
        for (int pixel = 0; pixel < map.Length; pixel++)
            inks[pixel] = InstalledSheetColouring.InkOf(map[pixel]);
        return new SpriteImageLayers(pixels, inks);
    }

    /// <summary>A sheet's image file name.</summary>
    internal static string ImageFile(string name) => name + ".png";

    /// <summary>
    /// A sheet's palette map file name: each pixel's ink, as <see cref="InstalledSheetColouring.MapPixelOf"/> installs it
    /// (#1544): the place of its colour among the image's own colours in red, whether it lies in a front tile, is clear and
    /// blends in green, opaque where it has a colour.
    /// </summary>
    internal static string PaletteFile(string name) => name + ".palette.png";

    /// <summary>A sheet's frame manifest file name.</summary>
    internal static string FrameFile(string name) => name + ".json";

    /// <summary>Writes a new installed file and records its hash.</summary>
    private void WriteFile(string name, byte[] bytes)
    {
        using (var file = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
            file.Write(bytes);
        hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>
    /// An installed file as the installation holds it. A file a player edited in place is used as edited (#1532); it fails
    /// only where it is decoded, when it does not decode.
    /// </summary>
    private byte[] Installed(string name)
    {
        if (!hashes.ContainsKey(name))
            throw new InvalidDataException($"Sprite sheet manifest {Path.Combine(directory, manifestFileName)} omits {name}.");
        return File.ReadAllBytes(Path.Combine(directory, name));
    }

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>The stock files' provenance: schema, cartridge and each file's hash.</summary>
    private sealed record Provenance(int Version, string SourceCartridgeSha256, Dictionary<string, string> Sha256);
}
