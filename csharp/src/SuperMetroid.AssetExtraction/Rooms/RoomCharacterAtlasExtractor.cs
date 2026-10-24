using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Extracts the shared CRE and all distinct retail room-character sources as indexed PNGs.</summary>
[CpuBusBoundary("codec: ROM compressed CRE and room character sources → indexed room character PNGs")]
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
        Add(RoomCharacterAtlasFormat.CreFileName,
            RoomAssetRomData.Tilesets.CreCharactersAddress);
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            RoomCharacterSet set = RoomTilesetDefinitions.Get(graphicsSet).Characters;
            string name = RoomCharacterAtlasFormat.FileName(set);
            if (!files.ContainsKey(name)) Add(name, RoomTilesetSources.Characters[set]);
        }
        Add(RoomCharacterAtlasFormat.FileName(RoomCharacterSet.TourianStatueGhost),
            RoomTilesetSources.Characters[RoomCharacterSet.TourianStatueGhost], compressed: false);
        return files;

        void Add(string fileName, int sourceAddress, bool compressed = true)
        {
            byte[] planar = compressed
                ? RomDataReader.Decompress(CartridgeImportSource.Require(bus), sourceAddress)
                : RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), sourceAddress,
                    LibraryBackgroundRomData.TourianStatueGhostByteCount);
            int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(planar.Length);
            byte[] indexes = SnesGraphics.DecodePlanarTiles(planar, 4,
                RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, indexes, SnesGraphics.DiagnosticPalette(16));
            byte[] encoded = png.ToArray();
            RoomCharacterAtlas loaded = RoomCharacterAtlas.Load(new MemoryStream(encoded), planar.Length);
            if (!loaded.Characters.Span.SequenceEqual(planar))
                throw new InvalidDataException(
                    $"Room character PNG {fileName} changed source ${sourceAddress:X6} ({tileCount} tiles).");
            files.Add(fileName, encoded);
        }
    }
}
