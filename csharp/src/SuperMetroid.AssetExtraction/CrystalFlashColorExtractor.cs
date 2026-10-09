using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Crystal Flash display colors; native duration words are not editable.</summary>
public static class CrystalFlashColorExtractor
{
    /// <summary>Exports Crystal Flash's ten body-color selections and six bubble-color selections in native pointer-record order.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for body pointer/timer records at $91:DC00, bubble pointers at $91:DC28, and their bank-$9B color payloads.</param>
    /// <returns>A new UTF-8 JSON buffer with ten ten-color Body rows and six six-color Bubble rows, expressed as RGB5 channels 0..31.</returns>
    /// <remarks>The two bands partition OBJ palette six into body colors 0..9 and bubble colors 10..15. Bit 15 is omitted from channel output; adjacent duration words, phase progression, activation costs, and recovery behavior are not exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var body = new PaletteRgb5[CrystalFlashColorFormat.BodyFrameCount][];
        for (int frame = 0; frame < body.Length; frame++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                SamusPaletteRomData.CrystalFlash.BodyRecords +
                frame * SamusPaletteRomData.CrystalFlash.BodyRecordByteCount);
            body[frame] = ReadColors(pointer, CrystalFlashColorFormat.BodyColorCount);
        }
        var bubble = new PaletteRgb5[CrystalFlashColorFormat.BubbleFrameCount][];
        for (int frame = 0; frame < bubble.Length; frame++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
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
            byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
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
