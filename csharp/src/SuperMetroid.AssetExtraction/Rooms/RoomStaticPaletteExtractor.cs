using SuperMetroid.Core;
using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Extracts exactly the base CGRAM colors consumed by room graphics loading.</summary>
public static class RoomStaticPaletteExtractor
{
    /// <summary>Imports the first 128 base BG colors from each distinct room graphics-set palette source.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the compressed native room palettes.</param>
    /// <returns>A new filename-keyed collection of UTF-8 JSON byte arrays containing RGB5 channels in CGRAM 0..127 order.</returns>
    /// <remarks>Discards the unused high color bit and trailing source data, then verifies equivalent CGRAM colors after recompilation. Shared sources are deduplicated; files are not written.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A source is invalid, contains fewer than 256 palette bytes, or fails palette or CGRAM roundtrip validation.</exception>
    [PpuLayoutBoundary("codec: native room palette CGDATA stream compared with the compiled palette")]
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var files = new Dictionary<string, byte[]>();
        RoomTilesetSourceMap sources = RoomTilesetSources.Read(CartridgeImportSource.Require(bus));
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            RoomPaletteSet set = RoomTilesetDefinitions.Get(graphicsSet).Palette;
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
                ushort word = BinaryPrimitives.ReadUInt16LittleEndian(
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
            byte[] serialized = json.ToArray();
            RoomStaticPalette compiled = RoomStaticPaletteFile.Read(
                new MemoryStream(serialized, writable: false));
            var nativeCgram = new SnesCgram();
            var compiledCgram = new SnesCgram();
            new CgramPalettes(nativeCgram).All.LoadPortBytes(decompressed.AsSpan(
                0, RoomStaticPaletteFormat.ByteCount));
            new CgramPalettes(compiledCgram).Background.LoadPortBytes(compiled.PackedColours.Span);
            if (!new CgramPalettes(nativeCgram).Image.SequenceEqual(new CgramPalettes(compiledCgram).Image))
                throw new InvalidDataException(
                    $"Room palette JSON {name} changed base CGRAM colors at extraction.");
            files.Add(name, serialized);
        }
        return files;
    }
}
