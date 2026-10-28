using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Buffers.Binary;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Exports CRE and distinct area block tables as visual composition, never collision.</summary>
public static class RoomMetatileExtractor
{
    /// <summary>Imports shared CRE and distinct room graphics-set metatile tables as visual block compositions.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the compressed native block-definition tables.</param>
    /// <returns>A new filename-keyed collection of UTF-8 JSON byte arrays containing each 16x16 block's four ordered 8x8 tile references and BG attributes.</returns>
    /// <remarks>Shared sources are exported once and each document is recompiled for exact native-byte equality. Collision and block-behavior data are not included; files are not written.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">Decompression, block-count validation, or the visual document's native-byte roundtrip fails.</exception>
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var files = new Dictionary<string, byte[]>();
        RoomTilesetSourceMap sources = RoomTilesetSources.Read(CartridgeImportSource.Require(bus));
        Add(RoomMetatileFormat.CreFileName,
            RoomTilesetSourceDefinitions.CreBlocks);
        for (int graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            RoomBlockSet set = RoomTilesets.Get(graphicsSet).Blocks;
            string name = RoomMetatileFormat.FileName(set);
            if (!files.ContainsKey(name)) Add(name, sources.Blocks[set]);
        }
        return files;

        void Add(string name, SnesAddress source)
        {
            byte[] native = RomDataReader.Decompress(CartridgeImportSource.Require(bus), source);
            int count = RoomMetatileFormat.ValidateBlockCount(native.Length);
            var blocks = new RoomMetatileDefinition[count];
            for (int index = 0; index < count; index++)
            {
                ReadOnlySpan<byte> sourceBlock = native.AsSpan(index * RoomMetatileFormat.BytesPerBlock);
                blocks[index] = new RoomMetatileDefinition
                {
                    TopLeft = Cell(sourceBlock, 0), TopRight = Cell(sourceBlock, 1),
                    BottomLeft = Cell(sourceBlock, 2), BottomRight = Cell(sourceBlock, 3),
                };
            }
            using var json = new MemoryStream();
            RoomMetatileAtlas.Write(json,
                new RoomMetatileDocument { Version = RoomMetatileFormat.Version, Blocks = blocks },
                native.Length);
            byte[] serialized = json.ToArray();
            RoomMetatileAtlas compiled = RoomMetatileAtlas.Load(
                new MemoryStream(serialized, writable: false), native.Length);
            if (!BackgroundTileWords.ToBytes(compiled.BlockTiles.Span).SequenceEqual(native))
                throw new InvalidDataException(
                    $"Room metatile JSON {name} changed source ${source:X6} block bytes.");
            files.Add(name, serialized);
        }
    }

    /// <summary>Decodes one quadrant's SNES tilemap word into editable tile, palette, priority, and flip fields.</summary>
    private static RoomMetatileCell Cell(ReadOnlySpan<byte> sourceBlock, int quadrant)
    {
        var word = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(
            sourceBlock[(quadrant * sizeof(ushort))..]));
        return new RoomMetatileCell
        {
            TileColumn = word.CharacterIndex % RoomMetatileFormat.TileColumns,
            TileRow = word.CharacterIndex / RoomMetatileFormat.TileColumns,
            Palette = word.PaletteIndex,
            Priority = word.HasPriority,
            FlipX = word.FlipHorizontally,
            FlipY = word.FlipVertically,
        };
    }
}
