using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

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
        ArgumentNullException.ThrowIfNull(bus);
        var files = new Dictionary<string, byte[]>();
        Add(RoomMetatileFormat.CreFileName,
            RoomAssetRomData.Tilesets.CreBlockDefinitionsAddress);
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int source = RoomTilesetDefinitions.Get(graphicsSet).BlockDefinitionsAddress;
            string name = RoomMetatileFormat.SourceFileName(source);
            if (!files.ContainsKey(name)) Add(name, source);
        }
        return files;

        void Add(string name, int source)
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
            if (!compiled.Transfer.Span.SequenceEqual(native))
                throw new InvalidDataException(
                    $"Room metatile JSON {name} changed source ${source:X6} block bytes.");
            files.Add(name, serialized);
        }
    }

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
