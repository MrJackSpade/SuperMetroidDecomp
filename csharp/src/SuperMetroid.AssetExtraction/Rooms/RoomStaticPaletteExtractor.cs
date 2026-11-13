using SuperMetroid.Core;
using System.Buffers.Binary;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Extracts exactly the base CGRAM colors consumed by room graphics loading.</summary>
public static class RoomStaticPaletteExtractor
{
    /// <summary>Imports the first 128 base BG colors from each distinct room graphics-set palette source.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the compressed native room palettes.</param>
    /// <returns>A new filename-keyed collection of UTF-8 JSON byte arrays containing RGB5 channels in CGRAM 0..127 order.</returns>
    /// <remarks>Discards the unused high color bit and trailing source data. Shared sources are deduplicated; files are not written.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A source is invalid or contains fewer than 256 palette bytes.</exception>
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var files = new Dictionary<string, byte[]>();
        RoomTilesetSourceMap sources = RoomTilesetSources.Read(CartridgeImportSource.Require(bus));
        foreach (TilesetDefinition tileset in RoomTilesets.All)
        {
            RoomPaletteSet set = tileset.Palette;
            SnesAddress address = sources.Palettes[set];
            string name = RoomStaticPaletteFormat.FileName(set);
            if (files.ContainsKey(name)) continue;

            byte[] decompressed = RomDataReader.Decompress(CartridgeImportSource.Require(bus), address);
            if (decompressed.Length < RoomStaticPaletteFormat.ByteCount)
                throw new InvalidDataException(
                    $"Room palette {address} has only ${decompressed.Length:X} bytes.");
            var colors = new PaletteRgb5[RoomStaticPaletteFormat.ColorCount];
            for (int index = 0; index < colors.Length; index++)
            {
                int word = BinaryPrimitives.ReadUInt16LittleEndian(
                    decompressed.AsSpan(index * sizeof(ushort)));
                colors[index] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
            using var json = new MemoryStream();
            RoomStaticPaletteFile.Write(json, new RoomStaticPaletteDocument
            {
                Version = RoomStaticPaletteFormat.Version,
                Colors = colors,
            });
            files.Add(name, json.ToArray());
        }
        return files;
    }
}
