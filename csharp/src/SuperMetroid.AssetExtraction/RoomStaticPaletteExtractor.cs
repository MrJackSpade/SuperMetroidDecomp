using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts exactly the base CGRAM colors consumed by room graphics loading.</summary>
public static class RoomStaticPaletteExtractor
{
    /// <summary>Imports the first 128 base BG colors from each distinct room graphics-set palette source.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the compressed native room palettes.</param>
    /// <returns>A new filename-keyed collection of UTF-8 JSON byte arrays containing RGB5 channels in CGRAM 0..127 order.</returns>
    /// <remarks>Discards the unused high color bit and trailing source data, then verifies equivalent CGRAM colors after recompilation. Shared sources are deduplicated; files are not written.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A source is invalid, contains fewer than 256 palette bytes, or fails palette or CGRAM roundtrip validation.</exception>
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var files = new Dictionary<string, byte[]>();
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int address = RoomTilesetDefinitions.Get(graphicsSet).PaletteAddress;
            string name = RoomStaticPaletteFormat.SourceFileName(address);
            if (files.ContainsKey(name)) continue;

            byte[] decompressed = RomDataReader.Decompress(CartridgeImportSource.Require(bus), address);
            if (decompressed.Length < RoomAssetRomData.GraphicsLayout.BackgroundPaletteByteCount)
                throw new InvalidDataException(
                    $"Room palette ${address:X6} has only ${decompressed.Length:X} bytes.");
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
            RoomStaticPalette.Write(json, new RoomStaticPaletteDocument
            {
                Version = RoomStaticPaletteFormat.Version,
                Colors = colors,
            });
            byte[] serialized = json.ToArray();
            RoomStaticPalette compiled = RoomStaticPalette.Load(
                new MemoryStream(serialized, writable: false));
            var nativeCgram = new SnesCgram();
            var compiledCgram = new SnesCgram();
            nativeCgram.LoadBytes(decompressed.AsSpan(
                0, RoomAssetRomData.GraphicsLayout.BackgroundPaletteByteCount));
            compiled.LoadTo(compiledCgram);
            if (!nativeCgram.Colors.SequenceEqual(compiledCgram.Colors))
                throw new InvalidDataException(
                    $"Room palette JSON {name} changed base CGRAM colors at extraction.");
            files.Add(name, serialized);
        }
        return files;
    }
}
