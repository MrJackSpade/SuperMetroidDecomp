using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the starting gameplay CGRAM image and shared room-sprite palette.</summary>
public static class GameplayBasePaletteFiles
{
    /// <summary>Writes the initial 256-color CGRAM image and 16 common-sprite colors as RGB5 JSON with a hashed manifest, then validates the resulting stock installation.</summary>
    /// <param name="bus">Cartridge source for the initial palette at $9A:8000 and common-sprite palette at $9A:FC00.</param>
    /// <param name="directory">Destination directory, created if absent; existing palette JSON and manifest are replaced.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest; the final stock check requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">The generated installation has incompatible provenance, digest, palette schema, or RGB5 content.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        var document = new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
            ReadColors(bus, GameplayBasePaletteFormat.InitialSourceAddress, SnesCgram.ColorCount),
            ReadColors(bus, GameplayBasePaletteFormat.CommonSpriteSourceAddress,
                GameplayBasePaletteFormat.SpriteColorCount));
        byte[] bytes = GameplayBasePaletteCatalog.Write(document);
        File.WriteAllBytes(Path.Combine(directory, GameplayBasePaletteFormat.ArtworkFileName), bytes);
        File.WriteAllBytes(Path.Combine(directory, GameplayBasePaletteFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new ArtworkManifest(
                GameplayBasePaletteFormat.Version, sourceCartridgeSha256,
                Convert.ToHexString(SHA256.HashData(bytes))),
                GameplayBasePaletteFormat.JsonOptions));
        _ = Load(directory, null);
    }

    /// <summary>Validates stock provenance, digest, and both complete RGB5 arrays before compiling an optional whole-document palette replacement without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the base-palette JSON and its provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory whose palette JSON replaces both stock arrays when present; an absent file retains stock.</param>
    /// <returns>An owned selected-color catalog for the initial CGRAM image and common sprites, also supplying enemy-projectile colors from the initial image.</returns>
    /// <exception cref="InvalidDataException">Stock provenance, digest, schema, or selected palette counts and RGB5 values are invalid.</exception>
    public static GameplayBasePaletteCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArtworkManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<ArtworkManifest>(File.ReadAllBytes(Path.Combine(
                stockDirectory, GameplayBasePaletteFormat.ManifestFileName)),
                GameplayBasePaletteFormat.JsonOptions) ??
                throw new InvalidDataException("Gameplay base palette manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay base palette manifest.", error);
        }
        if (manifest.Version != GameplayBasePaletteFormat.Version ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Gameplay base palette manifest does not match the pinned cartridge.");
        byte[] stock = File.ReadAllBytes(Path.Combine(stockDirectory,
            GameplayBasePaletteFormat.ArtworkFileName));
        if (!string.Equals(manifest.ArtworkSha256, Convert.ToHexString(SHA256.HashData(stock)),
            StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Stock gameplay base palette failed its manifest hash.");
        _ = GameplayBasePaletteCatalog.Load(new MemoryStream(stock, writable: false));
        string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory,
            GameplayBasePaletteFormat.ArtworkFileName);
        byte[] selected = overridePath is not null && File.Exists(overridePath)
            ? File.ReadAllBytes(overridePath) : stock;
        return GameplayBasePaletteCatalog.Load(new MemoryStream(selected, writable: false));
    }

    /// <summary>Checks stock manifest provenance and digest and validates the 256 initial and 16 common-sprite RGB5 colors, ignoring overrides.</summary>
    /// <param name="stockDirectory">Installed stock gameplay base-palette directory.</param>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Reads a contiguous native RGB5 palette and rejects words outside the representable color range.</summary>
    /// <param name="bus">Cartridge address space containing little-endian palette words.</param>
    /// <param name="address">Address of the first palette color.</param>
    /// <param name="count">Number of colors to read.</param>
    /// <returns>RGB5 colors in cartridge order.</returns>
    private static PaletteRgb5[] ReadColors(ISnesAddressSpace bus, int address, int count)
    {
        var colors = new PaletteRgb5[count];
        for (int color = 0; color < count; color++)
        {
            ushort word = (ushort)(bus.ReadCartridgeByte(address + color * 2) |
                bus.ReadCartridgeByte(address + color * 2 + 1) << 8);
            colors[color] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        return colors;
    }

    /// <summary>Stock provenance and integrity metadata for the base palette document.</summary>
    /// <param name="Version">Base palette document schema version.</param>
    /// <param name="SourceCartridgeSha256">SHA-256 of the cartridge revision used for extraction.</param>
    /// <param name="ArtworkSha256">SHA-256 of the stock palette JSON bytes.</param>
    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256,
        string ArtworkSha256);
}
