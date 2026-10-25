using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One directory of installed pre-coloured sprite sheets (#1392): for each sheet an RGBA PNG, its palette map and its
/// frame manifest, with a provenance manifest of their hashes. A player override of any file replaces the stock one.
/// </summary>
internal sealed class SpriteSheetStore
{
    /// <summary>The provenance manifest's schema revision.</summary>
    private const int FormatVersion = 1;

    /// <summary>The directory's stock files.</summary>
    private readonly string stockDirectory;

    /// <summary>The player's overrides, or null when none are read.</summary>
    private readonly string? overrideDirectory;

    /// <summary>The provenance manifest's file name.</summary>
    private readonly string manifestFileName;

    /// <summary>Each stock file's hash, written or read.</summary>
    private readonly Dictionary<string, string> hashes;

    /// <summary>Opens a directory's store.</summary>
    private SpriteSheetStore(string stockDirectory, string? overrideDirectory, string manifestFileName, Dictionary<string, string> hashes)
    {
        this.stockDirectory = stockDirectory;
        this.overrideDirectory = overrideDirectory;
        this.manifestFileName = manifestFileName;
        this.hashes = hashes;
    }

    /// <summary>Starts writing a directory's sheets, creating it if absent.</summary>
    internal static SpriteSheetStore Create(string directory, string manifestFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        return new(directory, null, manifestFileName, new(StringComparer.Ordinal));
    }

    /// <summary>Opens a directory's sheets for reading, after checking its provenance manifest.</summary>
    /// <param name="stockDirectory">The stock files.</param>
    /// <param name="overrideDirectory">The player's overrides, or null.</param>
    /// <param name="manifestFileName">The provenance manifest's file name.</param>
    /// <param name="sheetCount">The sheets the directory holds, three files each.</param>
    /// <exception cref="InvalidDataException">The manifest does not describe this installation's sheets.</exception>
    internal static SpriteSheetStore Open(string stockDirectory, string? overrideDirectory, string manifestFileName, int sheetCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, manifestFileName);
        Provenance manifest = JsonSerializer.Deserialize<Provenance>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Sprite sheet manifest {manifestPath} is empty.");
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase) ||
            manifest.Sha256 is null || manifest.Sha256.Count != sheetCount * 3)
            throw new InvalidDataException($"Sprite sheet manifest {manifestPath} does not describe this installation.");
        return new(stockDirectory, overrideDirectory, manifestFileName, manifest.Sha256);
    }

    /// <summary>Writes one composed sheet's image, palette map and frame manifest.</summary>
    internal void Write(string name, ComposedSpriteSheet composed)
    {
        Ensure.NotNull(composed);
        using var image = new MemoryStream();
        RgbaPng.Write(image, composed.Size, composed.Pixels);
        WriteFile(ImageFile(name), image.ToArray());
        using var palette = new MemoryStream();
        RgbaPng.Write(palette, composed.Size, PaletteImage(composed.PaletteIndices, composed.Priorities, composed.PartRows));
        WriteFile(PaletteFile(name), palette.ToArray());
        WriteFile(FrameFile(name), SpriteSheet.Write(composed.Manifest));
    }

    /// <summary>Writes the provenance manifest of every file written.</summary>
    internal void Finish(string sourceCartridgeSha256) =>
        File.WriteAllBytes(Path.Combine(stockDirectory, manifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new Provenance(FormatVersion, sourceCartridgeSha256, hashes), JsonOptions));

    /// <summary>Loads one sheet, preferring the player's override of each of its files.</summary>
    /// <exception cref="InvalidDataException">A stock hash, an image or the frame manifest is invalid.</exception>
    internal SpriteSheet Read(string name)
    {
        byte[] image = Selected(ImageFile(name));
        byte[] frames = Selected(FrameFile(name));
        byte[] palette = Selected(PaletteFile(name));
        RgbaPixel[] pixels, indices;
        Extent16<ImagePixels> size, indexSize;
        try
        {
            pixels = RgbaPng.Read(new MemoryStream(image, writable: false), out size);
            indices = RgbaPng.Read(new MemoryStream(palette, writable: false), out indexSize);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        if (indexSize != size)
            throw new InvalidDataException($"Sprite sheet {PaletteFile(name)} is not the size of {ImageFile(name)}.");
        return SpriteSheet.Load(new MemoryStream(frames, writable: false), name, size, pixels,
            [.. indices.Select(index => index.R)], [.. indices.Select(index => index.G)], [.. indices.Select(index => index.B)]);
    }

    /// <summary>A sheet's image file name.</summary>
    internal static string ImageFile(string name) => name + ".png";

    /// <summary>
    /// A sheet's palette-index image file name: each pixel's palette row times sixteen plus its colour in red, its
    /// priority in green and its part palette row in blue.
    /// </summary>
    internal static string PaletteFile(string name) => name + ".palette.png";

    /// <summary>A sheet's frame manifest file name.</summary>
    internal static string FrameFile(string name) => name + ".json";

    /// <summary>
    /// The palette-index image: each index in red, priority in green and part palette row in blue, opaque where a colour
    /// is shown.
    /// </summary>
    private static RgbaPixel[] PaletteImage(ReadOnlySpan<byte> indices, ReadOnlySpan<byte> priorities, ReadOnlySpan<byte> partRows)
    {
        var pixels = new RgbaPixel[indices.Length];
        for (int pixel = 0; pixel < indices.Length; pixel++)
            pixels[pixel] = new RgbaPixel(indices[pixel], priorities[pixel], partRows[pixel], indices[pixel] == 0 ? (byte)0 : byte.MaxValue);
        return pixels;
    }

    /// <summary>Writes a new stock file and records its hash.</summary>
    private void WriteFile(string name, byte[] bytes)
    {
        using (var file = new FileStream(Path.Combine(stockDirectory, name), FileMode.CreateNew, FileAccess.Write))
            file.Write(bytes);
        hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>The player's override of a file when present, otherwise the stock file after checking its hash.</summary>
    private byte[] Selected(string name)
    {
        string stockPath = Path.Combine(stockDirectory, name);
        byte[] stock = File.ReadAllBytes(stockPath);
        if (!hashes.TryGetValue(name, out string? expected) ||
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
    private sealed record Provenance(int Version, string SourceCartridgeSha256, Dictionary<string, string> Sha256);
}
