using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the starting gameplay CGRAM image and shared room-sprite palette.</summary>
public static class GameplayBasePaletteFiles
{
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

    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private static PaletteRgb5[] ReadColors(ISnesAddressSpace bus, int address, int count)
    {
        var colors = new PaletteRgb5[count];
        for (int color = 0; color < count; color++)
        {
            ushort word = (ushort)(bus.ReadByte(address + color * 2) |
                bus.ReadByte(address + color * 2 + 1) << 8);
            colors[color] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        return colors;
    }

    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256,
        string ArtworkSha256);
}
