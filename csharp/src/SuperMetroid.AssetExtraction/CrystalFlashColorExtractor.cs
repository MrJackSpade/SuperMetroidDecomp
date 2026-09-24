using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Crystal Flash display colors; native duration words are not editable.</summary>
public static class CrystalFlashColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var body = new PaletteRgb5[CrystalFlashColorFormat.BodyFrameCount][];
        for (int frame = 0; frame < body.Length; frame++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(bus,
                SamusPaletteRomData.CrystalFlash.BodyRecords +
                frame * SamusPaletteRomData.CrystalFlash.BodyRecordByteCount);
            body[frame] = ReadColors(pointer, CrystalFlashColorFormat.BodyColorCount);
        }
        var bubble = new PaletteRgb5[CrystalFlashColorFormat.BubbleFrameCount][];
        for (int frame = 0; frame < bubble.Length; frame++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(bus,
                SamusPaletteRomData.CrystalFlash.BubblePointers + frame * sizeof(ushort));
            bubble[frame] = ReadColors(pointer, CrystalFlashColorFormat.BubbleColorCount);
        }
        return CrystalFlashColorCatalog.Write(new CrystalFlashColorDocument
        {
            Version = CrystalFlashColorFormat.Version,
            Body = body,
            Bubble = bubble,
        });

        PaletteRgb5[] ReadColors(ushort pointer, int count)
        {
            byte[] source = RomDataReader.ReadFixedBank(bus,
                SamusPaletteRomData.Banks.Palette | pointer, count * sizeof(ushort));
            var result = new PaletteRgb5[count];
            for (int index = 0; index < count; index++)
            {
                ushort word = (ushort)(source[index * 2] | source[index * 2 + 1] << 8);
                result[index] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
            return result;
        }
    }
}
