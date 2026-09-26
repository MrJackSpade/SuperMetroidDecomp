using System.Buffers.Binary;
using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the Ceres-door actor's direct tile DMA and authored RGB5 rows.</summary>
public static class CeresDoorVisualFiles
{
    public static (string TileHash, string ColorHash) Extract(ISnesAddressSpace bus,
        string directory)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] planar = RomDataReader.ReadFixedBank(bus,
            CeresDoorVisualRomData.TileSource, CeresDoorVisualRomData.TileByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            RoomCharacterAtlasFormat.TileColumns, out int width, out int height);
        using var image = new MemoryStream();
        IndexedPng.Write(image, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] tilePng = image.ToArray();
        RoomCharacterAtlas roundtrip = RoomCharacterAtlas.Load(
            new MemoryStream(tilePng, writable: false), planar.Length);
        if (!roundtrip.Transfer.Span.SequenceEqual(planar))
            throw new InvalidDataException("Ceres-door PNG changed the native tile DMA.");

        byte[] paletteJson = CeresDoorVisualCatalog.Write(new CeresDoorVisualDocument
        {
            Version = 1,
            Normal = ReadColors(bus, CeresDoorVisualRomData.NormalColors,
                CeresDoorVisualRomData.SetupColorCount),
            Escape = ReadColors(bus, CeresDoorVisualRomData.EscapeColors,
                CeresDoorVisualRomData.SetupColorCount),
            Animation = Enumerable.Range(0, CeresDoorVisualRomData.AnimationRowCount)
                .Select(row => ReadColors(bus,
                    CeresDoorVisualRomData.AnimationColors +
                        row * CeresDoorVisualRomData.AnimationRowByteStride,
                    CeresDoorVisualRomData.AnimationColorCount)).ToArray(),
            Mode7DoorFrames =
            [
                RomDataReader.ReadFixedBank(bus,
                    CeresDoorVisualRomData.Mode7FirstFrameSource,
                    CeresDoorVisualRomData.Mode7FrameByteCount)
                    .Select(value => (int)value).ToArray(),
                RomDataReader.ReadFixedBank(bus,
                    CeresDoorVisualRomData.Mode7SecondFrameSource,
                    CeresDoorVisualRomData.Mode7FrameByteCount)
                    .Select(value => (int)value).ToArray(),
            ],
        });
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, CeresDoorVisualFormat.TilesFileName), tilePng);
        File.WriteAllBytes(Path.Combine(directory, CeresDoorVisualFormat.ColorsFileName), paletteJson);
        return (Convert.ToHexString(SHA256.HashData(tilePng)),
            Convert.ToHexString(SHA256.HashData(paletteJson)));
    }

    public static CeresDoorVisualCatalog Load(byte[] tilePng, byte[] paletteJson) =>
        CeresDoorVisualCatalog.Load(new MemoryStream(tilePng, writable: false),
            new MemoryStream(paletteJson, writable: false));

    private static PaletteRgb5[] ReadColors(ISnesAddressSpace bus, int sourceAddress,
        int count)
    {
        byte[] source = RomDataReader.ReadFixedBank(bus, sourceAddress,
            count * sizeof(ushort));
        var colors = new PaletteRgb5[count];
        for (int index = 0; index < count; index++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(index * 2));
            if ((word & 0x8000) != 0)
                throw new InvalidDataException(
                    $"Ceres-door color ${sourceAddress + index * 2:X6} has an unrepresentable high bit.");
            colors[index] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        return colors;
    }
}
