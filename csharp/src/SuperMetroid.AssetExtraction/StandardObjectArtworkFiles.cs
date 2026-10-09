using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installs the complete, indexed 4-bpp standard OBJ sheet as editable artwork.</summary>
public static class StandardObjectArtworkFiles
{
    /// <summary>Exports the selected $2E00-byte standard OBJ span as an indexed PNG, verifies its exact planar-byte round-trip, writes its hashed manifest, and validates stock.</summary>
    /// <param name="bus">Cartridge import source for the standard OBJ artwork beginning at $9A:D200.</param>
    /// <param name="directory">Destination directory, created if absent; existing standard OBJ PNG and manifest are replaced.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest; the final stock check requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">The PNG changes native character bytes or the resulting stock installation fails provenance, digest, or atlas validation.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), StandardObjectArtworkAddresses.Source,
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

    /// <summary>Validates stock provenance, PNG digest, and atlas geometry before compiling an optional standard OBJ PNG replacement without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the standard OBJ PNG and its provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory whose standard OBJ PNG replaces stock when present; an absent file retains stock.</param>
    /// <returns>The selected owned $2E00-byte 4-bpp character stream; PNG palette colors do not replace runtime CGRAM colors.</returns>
    /// <remarks>The PNG is 256x96 pixels with 368 row-major 8x8 characters; the final sixteen display cells must remain index zero.</remarks>
    /// <exception cref="InvalidDataException">Stock provenance, digest, or atlas validation fails, or the selected PNG has invalid geometry, indices, or display padding.</exception>
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

    /// <summary>Checks stock provenance and PNG digest and compiles the exact standard OBJ transfer, including zero-only unused display cells; ignores overrides.</summary>
    /// <param name="stockDirectory">Installed stock standard OBJ artwork directory.</param>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private sealed record Manifest(int Version, string SourceCartridgeSha256, string PngSha256);
}
