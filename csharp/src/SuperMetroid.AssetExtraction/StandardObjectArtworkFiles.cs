using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installs the complete, indexed 4-bpp standard OBJ sheet as editable artwork.</summary>
public static class StandardObjectArtworkFiles
{

    /// <summary>Exports the selected $2E00-byte standard OBJ span as an indexed PNG, verifies its exact planar-byte round-trip, writes its hashed manifest, and validates stock.</summary>
    /// <param name="bus">Cartridge import source for the standard OBJ artwork beginning at $9A:D200.</param>
    /// <param name="directory">Destination directory, created if absent; existing standard OBJ PNG and manifest are replaced.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), StandardObjectArtworkSourceDefinitions.Sheet,
            StandardObjectArtworkFormat.ByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(native, 4,
            RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] selected = png.ToArray();
        File.WriteAllBytes(Path.Combine(directory, StandardObjectArtworkFormat.FileName), selected);
        File.WriteAllBytes(Path.Combine(directory, StandardObjectArtworkFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new Manifest(StandardObjectArtworkFormat.Version,
                sourceCartridgeSha256)));
    }

    /// <summary>Validates the manifest's provenance and compiles the installed standard OBJ PNG, as the player may have edited it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the standard OBJ PNG and its provenance manifest.</param>
    /// <returns>The owned $2E00-byte 4-bpp character stream; PNG palette colors do not replace runtime CGRAM colors.</returns>
    /// <remarks>The PNG is 256x96 pixels with 368 row-major 8x8 characters; the final sixteen display cells must remain index zero.</remarks>
    /// <exception cref="InvalidDataException">The manifest's provenance fails, or the PNG has invalid geometry, indices, or display padding.</exception>
    public static RoomCharacterAtlas Load(string directory)
    {
        string manifestPath = Path.Combine(directory, StandardObjectArtworkFormat.ManifestFileName);
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
        string path = Path.Combine(directory, StandardObjectArtworkFormat.FileName);
        try
        {
            return RoomCharacterAtlasPng.Read(new MemoryStream(File.ReadAllBytes(path), writable: false),
                StandardObjectArtworkFormat.ByteCount);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid standard OBJ PNG {path}: {error.Message}", error);
        }
    }

    /// <summary>Binds the stock OBJ PNG digest to its source cartridge and asset format version.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256);
}
