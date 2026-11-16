using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Exports Samus's death-explosion characters into one indexed PNG.</summary>
public static class SamusDeathTileArtworkFiles
{
    /// <summary>Exports the five 32-character death-explosion pages as a 64-by-160 indexed PNG, with no manifest (#1693).</summary>
    /// <param name="bus">Cartridge source for the bank-$9B explosion characters, written in the atlas's page order.</param>
    /// <param name="importData">The import-data directory receiving the page (#1636), which only the import reads.</param>
    /// <remarks>Pixel indices represent four-bit OBJ pens, not runtime colors; animation timing and OAM placement are not exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is null, empty, or whitespace.</exception>
    /// <exception cref="IOException">Filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string importData)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        Directory.CreateDirectory(importData);
        var planar = new byte[SamusDeathTileAtlasFormat.TotalByteCount];
        for (int index = 0; index < planar.Length; index++)
            planar[index] = bus.ReadCartridgeByte(SamusSourceDefinitions.DeathExplosionCharacters.Value +
                SamusDeathTileAtlasFormat.ExplosionByte(index));
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,
            SamusDeathTileAtlasFormat.BitsPerPixel, SamusDeathTileAtlasFormat.Width / 8,
            out Extent16<ImagePixels> size);
        using var output = new MemoryStream();
        IndexedPng.Write(output, size, pixels,
            SnesGraphics.DiagnosticPalette(1 << SamusDeathTileAtlasFormat.BitsPerPixel));
        byte[] png = output.ToArray();
        File.WriteAllBytes(Path.Combine(importData, SamusDeathTileAtlasFormat.ArtworkFileName), png);
    }

    /// <summary>Loads the import's death-explosion atlas (#1636).</summary>
    /// <param name="importData">The component's import-data directory, holding the death-explosion page.</param>
    /// <returns>A ROM-independent atlas of the explosion characters, retaining the character pixels without changing palettes or sequence timing.</returns>
    /// <remarks>The PNG must decode as the 64-by-160 four-bit character sheet.</remarks>
    /// <exception cref="InvalidDataException">The PNG's dimensions or indices are invalid; admission errors include the source filename.</exception>
    /// <exception cref="IOException">The page cannot be read.</exception>
    public static SamusDeathTileAtlas LoadForImport(string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        return SamusArtworkFile.Read(Path.Combine(importData, SamusDeathTileAtlasFormat.ArtworkFileName))
            .Compile(SamusDeathTileAtlas.Load);
    }
}
