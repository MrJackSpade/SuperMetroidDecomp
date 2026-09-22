using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the shared CRE and all distinct retail room-character sources as indexed PNGs.</summary>
public static class RoomCharacterAtlasExtractor
{
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var files = new Dictionary<string, byte[]>();
        Add(RoomCharacterAtlasFormat.CreFileName,
            RoomAssetRomData.Tilesets.CreCharactersAddress);
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int sourceAddress = RoomTilesetDefinitions.Get(graphicsSet).CharacterAddress;
            string name = RoomCharacterAtlasFormat.SourceFileName(sourceAddress);
            if (!files.ContainsKey(name)) Add(name, sourceAddress);
        }
        Add(RoomCharacterAtlasFormat.SourceFileName(
                RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress),
            RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress,
            compressed: false);
        return files;

        void Add(string fileName, int sourceAddress, bool compressed = true)
        {
            byte[] planar = compressed
                ? RomDataReader.Decompress(bus, sourceAddress)
                : RomDataReader.ReadFixedBank(bus, sourceAddress,
                    RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount);
            int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(planar.Length);
            byte[] indexes = SnesGraphics.DecodePlanarTiles(planar, 4,
                RoomCharacterAtlasFormat.TileColumns, out int width, out int height);
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, indexes, SnesGraphics.DiagnosticPalette(16));
            byte[] encoded = png.ToArray();
            RoomCharacterAtlas loaded = RoomCharacterAtlas.Load(new MemoryStream(encoded), planar.Length);
            if (!loaded.Transfer.Span.SequenceEqual(planar))
                throw new InvalidDataException(
                    $"Room character PNG {fileName} changed source ${sourceAddress:X6} ({tileCount} tiles).");
            files.Add(fileName, encoded);
        }
    }
}
