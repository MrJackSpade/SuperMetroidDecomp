using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured sprite sheets (#1392): for each sheet an RGBA PNG and its frame manifest, with a provenance
/// manifest of their hashes. A player override of either file replaces the stock one.
/// </summary>
[CpuBusBoundary("codec: ROM sprite frames, characters and palette rows → pre-coloured sprite sheets")]
public static class SpriteSheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ManifestFileName = "sprite-sheets.json";

    /// <summary>The provenance manifest's schema revision.</summary>
    private const int FormatVersion = 1;

    /// <summary>Composes and writes every sheet, then validates the result.</summary>
    /// <param name="bus">Cartridge source for frames no installed artwork holds.</param>
    /// <param name="content">Installed stock content holding the characters, compositions and colours the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string content, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        SamusSheetInputs samus = SamusSheetInputs.Load(content);
        foreach (SpriteSheetId sheet in SpriteSheetDefinitions.Sheets)
        {
            ComposedSpriteSheet composed = Compose(bus, samus, sheet);
            using var image = new MemoryStream();
            RgbaPng.Write(image, composed.Size, composed.Pixels);
            Write(directory, ImageFile(sheet), image.ToArray(), hashes);
            using var palette = new MemoryStream();
            RgbaPng.Write(palette, composed.Size, PaletteImage(composed.PaletteIndices, composed.Priorities));
            Write(directory, PaletteFile(sheet), palette.ToArray(), hashes);
            Write(directory, FrameFile(sheet), SpriteSheet.Write(composed.Manifest), hashes);
        }
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new Manifest(FormatVersion, sourceCartridgeSha256, hashes), JsonOptions));
        _ = Load(directory, null);
    }

    /// <summary>Checks the stock files against their manifest and loads every sheet, preferring a player override of each file.</summary>
    /// <exception cref="InvalidDataException">The manifest, a stock hash, an image or a frame manifest is invalid.</exception>
    public static InstalledSpriteSheets Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        Manifest manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Sprite sheet manifest {manifestPath} is empty.");
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase) ||
            manifest.Sha256 is null || manifest.Sha256.Count != SpriteSheetDefinitions.Sheets.Count * 3)
            throw new InvalidDataException($"Sprite sheet manifest {manifestPath} does not describe this installation.");
        var sheets = new Dictionary<SpriteSheetId, SpriteSheet>();
        // The body sheets are named from the stock body tables installed beside them.
        var body = new Lazy<SamusBodyArtworkCatalog>(() => SamusBodyArtworkFiles.Load(
            Path.Combine(Path.GetDirectoryName(Path.GetFullPath(stockDirectory))!, GameInstallationLayout.SamusBodyDirectoryName), null));
        foreach (SpriteSheetId sheet in SpriteSheetDefinitions.Sheets)
        {
            byte[] image = Selected(stockDirectory, overrideDirectory, ImageFile(sheet), manifest);
            byte[] frames = Selected(stockDirectory, overrideDirectory, FrameFile(sheet), manifest);
            byte[] palette = Selected(stockDirectory, overrideDirectory, PaletteFile(sheet), manifest);
            Rgba32[] pixels, indices;
            Extent16<ImagePixels> size, indexSize;
            try
            {
                pixels = RgbaPng.Read(new MemoryStream(image, writable: false), out size);
                indices = RgbaPng.Read(new MemoryStream(palette, writable: false), out indexSize);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Sprite sheet {SpriteSheetDefinitions.FileName(sheet)}: {error.Message}", error);
            }
            if (indexSize != size)
                throw new InvalidDataException($"Sprite sheet {PaletteFile(sheet)} is not the size of {ImageFile(sheet)}.");
            SpriteSheet loaded = SpriteSheet.Load(new MemoryStream(frames, writable: false), SpriteSheetDefinitions.FileName(sheet),
                size, pixels, [.. indices.Select(index => index.R)], [.. indices.Select(index => index.G)]);
            if (!loaded.HoldsExactly(FrameNames(sheet, body)))
                throw new InvalidDataException($"Sprite sheet {FrameFile(sheet)} does not hold exactly the frames its family draws.");
            sheets.Add(sheet, loaded);
        }
        return new InstalledSpriteSheets(sheets);
    }

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Composes one sheet.</summary>
    private static ComposedSpriteSheet Compose(ISnesAddressSpace bus, SamusSheetInputs samus, SpriteSheetId sheet) => sheet switch
    {
        SpriteSheetId.EscapeTimer => EscapeTimerSheetComposer.Compose(bus),
        SpriteSheetId.Grapple => GrappleSheetComposer.Compose(samus),
        SpriteSheetId.SamusBodyTop or SpriteSheetId.SamusBodyBottom => SamusBodySheetComposer.Compose(samus, sheet),
        _ => ProjectileSheetComposer.Compose(samus, sheet),
    };

    /// <summary>The frames a sheet must hold: the ones its family draws.</summary>
    /// <param name="sheet">The sheet.</param>
    /// <param name="body">Samus's body tables, which name the body sheets' frames.</param>
    internal static IEnumerable<string> FrameNames(SpriteSheetId sheet, Lazy<SamusBodyArtworkCatalog> body) => sheet switch
    {
        SpriteSheetId.EscapeTimer => EscapeTimerPresentationDefinitions.Glyphs.Select(EscapeTimerPresentationDefinitions.FrameName),
        SpriteSheetId.Grapple => GrappleSheetDefinitions.Frames,
        SpriteSheetId.SamusBodyTop or SpriteSheetId.SamusBodyBottom => SamusBodySheetComposer.FrameNames(body.Value, sheet),
        _ => ProjectileSheetDefinitions.Frames,
    };

    /// <summary>A sheet's image file name.</summary>
    internal static string ImageFile(SpriteSheetId sheet) => SpriteSheetDefinitions.FileName(sheet) + ".png";

    /// <summary>A sheet's palette-index image file name: each pixel's palette row times sixteen plus its colour in red, its priority in green.</summary>
    internal static string PaletteFile(SpriteSheetId sheet) => SpriteSheetDefinitions.FileName(sheet) + ".palette.png";

    /// <summary>The palette-index image: each index in red and priority in green, opaque where a colour is shown.</summary>
    private static Rgba32[] PaletteImage(ReadOnlySpan<byte> indices, ReadOnlySpan<byte> priorities)
    {
        var pixels = new Rgba32[indices.Length];
        for (int pixel = 0; pixel < indices.Length; pixel++)
            pixels[pixel] = new Rgba32(indices[pixel], priorities[pixel], 0, indices[pixel] == 0 ? (byte)0 : byte.MaxValue);
        return pixels;
    }

    /// <summary>A sheet's frame manifest file name.</summary>
    internal static string FrameFile(SpriteSheetId sheet) => SpriteSheetDefinitions.FileName(sheet) + ".json";

    /// <summary>Writes a new stock file and records its hash.</summary>
    private static void Write(string directory, string name, byte[] bytes, Dictionary<string, string> hashes)
    {
        using (var file = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
            file.Write(bytes);
        hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>The player's override of a file when present, otherwise the stock file after checking its hash.</summary>
    private static byte[] Selected(string stockDirectory, string? overrideDirectory, string name, Manifest manifest)
    {
        string stockPath = Path.Combine(stockDirectory, name);
        byte[] stock = File.ReadAllBytes(stockPath);
        if (!manifest.Sha256.TryGetValue(name, out string? expected) ||
            !string.Equals(Convert.ToHexString(SHA256.HashData(stock)), expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Stock sprite sheet file {stockPath} failed its manifest hash.");
        string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
        return overridePath is not null && File.Exists(overridePath) ? File.ReadAllBytes(overridePath) : stock;
    }

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>The stock files' provenance: schema, cartridge and each file's hash.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256, Dictionary<string, string> Sha256);
}
