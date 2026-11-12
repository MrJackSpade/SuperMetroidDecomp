using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Extracts the complete 4-bpp standard OBJ characters as import data (#1636): the page is written to the component's
/// import-data directory (<see cref="ImportDataDirectory.Of"/>), which the sheets compose from and the installer deletes
/// before it publishes; only its provenance manifest is installed, and the game reads neither.
/// </summary>
public static class StandardObjectArtworkFiles
{

    /// <summary>Writes the selected $2E00-byte standard OBJ span as an indexed page into the component's import data and its provenance manifest into <paramref name="directory"/>.</summary>
    /// <param name="bus">Cartridge import source for the standard OBJ artwork beginning at $9A:D200.</param>
    /// <param name="directory">The component's directory, created if absent; the page goes to its import-data directory.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        string importData = ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);
        byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), StandardObjectArtworkSourceDefinitions.Sheet,
            StandardObjectArtworkFormat.ByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(native, 4,
            RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] selected = png.ToArray();
        File.WriteAllBytes(Path.Combine(importData, StandardObjectArtworkFormat.FileName), selected);
        File.WriteAllBytes(Path.Combine(directory, StandardObjectArtworkFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new Manifest(StandardObjectArtworkFormat.Version,
                sourceCartridgeSha256)));
    }

    /// <summary>Validates the manifest's provenance and compiles the import's standard OBJ page, which only the import reads (#1636).</summary>
    /// <param name="directory">The component's directory, holding the provenance manifest.</param>
    /// <param name="importData">The component's import-data directory, holding the page.</param>
    /// <returns>The owned $2E00-byte 4-bpp character stream; the page's palette colors do not replace runtime CGRAM colors.</returns>
    /// <remarks>The page is 256x96 pixels with 368 row-major 8x8 characters; the final sixteen display cells must remain index zero.</remarks>
    /// <exception cref="InvalidDataException">The manifest's provenance fails, or the page has invalid geometry, indices, or display padding.</exception>
    public static RoomCharacterAtlas LoadForImport(string directory, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
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
        string path = Path.Combine(importData, StandardObjectArtworkFormat.FileName);
        try
        {
            return RoomCharacterAtlasPng.Read(new MemoryStream(File.ReadAllBytes(path), writable: false),
                StandardObjectArtworkFormat.ByteCount);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid standard OBJ page {path}: {error.Message}", error);
        }
    }

    /// <summary>Binds the extracted OBJ characters to their source cartridge and asset format version.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256);
}
