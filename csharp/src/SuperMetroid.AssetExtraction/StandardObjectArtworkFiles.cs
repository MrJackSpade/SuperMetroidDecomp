using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installs the complete, indexed 4-bpp standard OBJ sheet as editable artwork.</summary>
public static class StandardObjectArtworkFiles
{
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        byte[] native = RomDataReader.ReadFixedBank(bus, StandardObjectArtworkFormat.SourceAddress,
            StandardObjectArtworkFormat.TransferByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(native, 4,
            RoomCharacterAtlasFormat.TileColumns, out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] selected = png.ToArray();
        RoomCharacterAtlas loaded = RoomCharacterAtlas.Load(
            new MemoryStream(selected, writable: false), StandardObjectArtworkFormat.TransferByteCount);
        if (!loaded.Transfer.Span.SequenceEqual(native))
            throw new InvalidDataException("Standard OBJ PNG changed native tile bytes during extraction.");
        File.WriteAllBytes(Path.Combine(directory, StandardObjectArtworkFormat.FileName), selected);
        File.WriteAllBytes(Path.Combine(directory, StandardObjectArtworkFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new Manifest(StandardObjectArtworkFormat.Version,
                sourceCartridgeSha256, Convert.ToHexString(SHA256.HashData(selected)))));
        ValidateStock(directory);
    }

    public static RoomCharacterAtlas Load(string stockDirectory, string? overrideDirectory)
    {
        string manifestPath = Path.Combine(stockDirectory, StandardObjectArtworkFormat.ManifestFileName);
        Manifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllBytes(manifestPath)) ??
                throw new InvalidDataException($"Standard OBJ manifest {manifestPath} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid standard OBJ manifest {manifestPath}.", error);
        }
        if (manifest.Version != StandardObjectArtworkFormat.Version ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Standard OBJ manifest does not match the pinned cartridge.");
        string stockPath = Path.Combine(stockDirectory, StandardObjectArtworkFormat.FileName);
        byte[] stock = File.ReadAllBytes(stockPath);
        if (!string.Equals(manifest.PngSha256, Convert.ToHexString(SHA256.HashData(stock)),
            StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Stock standard OBJ PNG {stockPath} failed its manifest hash.");
        _ = RoomCharacterAtlas.Load(new MemoryStream(stock, writable: false),
            StandardObjectArtworkFormat.TransferByteCount);
        string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory,
            StandardObjectArtworkFormat.FileName);
        byte[] selected = overridePath is not null && File.Exists(overridePath)
            ? File.ReadAllBytes(overridePath) : stock;
        try
        {
            return RoomCharacterAtlas.Load(new MemoryStream(selected, writable: false),
                StandardObjectArtworkFormat.TransferByteCount);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid standard OBJ PNG {overridePath ?? stockPath}: {error.Message}", error);
        }
    }

    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private sealed record Manifest(int Version, string SourceCartridgeSha256, string PngSha256);
}
