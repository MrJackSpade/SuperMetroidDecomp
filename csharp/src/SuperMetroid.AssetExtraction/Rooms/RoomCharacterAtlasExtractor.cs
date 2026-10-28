using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Extracts the shared CRE and all distinct retail room-character sources as indexed PNGs.</summary>
public static class RoomCharacterAtlasExtractor
{
    /// <summary>Imports shared CRE characters, every distinct room graphics-set character source, and the uncompressed Tourian-statue ghost span.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying compressed room characters and the raw ghost artwork.</param>
    /// <returns>A new filename-keyed collection of indexed PNG byte arrays preserving ordered four-bit planar characters with pixel indices 0..15.</returns>
    /// <remarks>Shared sources produce one atlas. PNGs use at most 32 characters per row, zero-fill unused cells, and use diagnostic colors; recompilation must reproduce the exact source bytes.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A source is invalid, has an unsupported character count, or fails the PNG-to-planar roundtrip.</exception>
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var files = new Dictionary<string, byte[]>();
        RoomTilesetSourceMap sources = RoomTilesetSources.Read(CartridgeImportSource.Require(bus));
        Add(RoomCharacterAtlasFormat.CreFileName,
            RoomTilesetSourceDefinitions.CreCharacters);
        for (int graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            RoomCharacterSet set = RoomTilesetDefinitions.Get(graphicsSet).Characters;
            string name = RoomCharacterAtlasFormat.FileName(set);
            if (!files.ContainsKey(name)) Add(name, sources.Characters[set]);
        }
        Add(RoomCharacterAtlasFormat.FileName(RoomCharacterSet.TourianStatueGhost),
            sources.Characters[RoomCharacterSet.TourianStatueGhost], compressed: false);
        return files;

        void Add(string fileName, SnesAddress sourceAddress, bool compressed = true)
        {
            byte[] planar = compressed
                ? RomDataReader.Decompress(CartridgeImportSource.Require(bus), sourceAddress)
                : RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), sourceAddress,
                    LibraryBackgroundStagingDefinitions.TourianStatueGhostByteCount);
            int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(planar.Length);
            byte[] indexes = SnesGraphics.DecodePlanarTiles(planar, 4,
                RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, indexes, SnesGraphics.DiagnosticPalette(16));
            byte[] encoded = png.ToArray();
            RoomCharacterAtlas loaded = RoomCharacterAtlasPng.Read(new MemoryStream(encoded), planar.Length);
            if (!loaded.Characters.Span.SequenceEqual(planar))
                throw new InvalidDataException(
                    $"Room character PNG {fileName} changed source ${sourceAddress:X6} ({tileCount} tiles).");
            files.Add(fileName, encoded);
        }
    }
}
