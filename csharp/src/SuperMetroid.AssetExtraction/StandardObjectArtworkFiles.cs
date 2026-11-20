using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
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
/// before it publishes. The component installs nothing (#1693): the format and the cartridge the page was extracted from
/// are the installation receipt's, so no manifest records them.
/// </summary>
public static class StandardObjectArtworkFiles
{

    /// <summary>Writes the selected $2E00-byte standard OBJ span as an indexed page into the component's import data.</summary>
    /// <param name="bus">Cartridge import source for the standard OBJ artwork beginning at $9A:D200.</param>
    /// <param name="directory">The component's directory; the page goes to its import-data directory.</param>
    public static void Extract(ISnesAddressSpace bus, string directory)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string importData = ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);
        byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), StandardObjectArtworkSourceDefinitions.Sheet,
            StandardObjectArtworkFormat.ByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(native, 4,
            RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        File.WriteAllBytes(Path.Combine(importData, StandardObjectArtworkFormat.FileName), png.ToArray());
    }

    /// <summary>Compiles the import's standard OBJ page, which only the import reads (#1636).</summary>
    /// <param name="importData">The component's import-data directory, holding the page.</param>
    /// <returns>The owned $2E00-byte 4-bpp character stream; the page's palette colors do not replace runtime CGRAM colors.</returns>
    /// <remarks>The page is 256x96 pixels with 368 row-major 8x8 characters; the final sixteen display cells must remain index zero.</remarks>
    /// <exception cref="InvalidDataException">The page has invalid geometry, indices, or display padding.</exception>
    public static RoomCharacterAtlas LoadForImport(string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
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
}
