using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports bounded Ceres Ridley color tables without exposing AI or fade timing.</summary>
public static class CeresRidleyColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return CeresRidleyColorCatalog.Write(new CeresRidleyColorDocument
        {
            Version = CeresRidleyColorFormat.Version,
            Start = ReadColors(CeresRidleyPaletteRomData.StartColors,
                CeresRidleyPaletteRomData.StartColorCount),
            EyeFade = ReadRows(CeresRidleyPaletteRomData.EyeFadeColors,
                CeresRidleyPaletteRomData.EyeFadeRowCount,
                CeresRidleyPaletteRomData.EyeFadeColorCount),
            BodyFade = ReadRows(CeresRidleyPaletteRomData.BodyFadeColors,
                CeresRidleyPaletteRomData.BodyFadeRowCount,
                CeresRidleyPaletteRomData.BodyFadeColorCount),
            Health = ReadRows(CeresRidleyPaletteRomData.HealthColors,
                CeresRidleyPaletteRomData.HealthRowCount,
                CeresRidleyPaletteRomData.HealthColorCount),
            RetreatBg = ReadColors(CeresRidleyPaletteRomData.RetreatBgColors,
                CeresRidleyPaletteRomData.RetreatBgColorCount),
            RetreatShared = ReadColors(CeresRidleyPaletteRomData.RetreatSharedColors,
                CeresRidleyPaletteRomData.RetreatSharedColorCount),
        });

        PaletteRgb5[][] ReadRows(int source, int rows, int colorsPerRow)
        {
            var result = new PaletteRgb5[rows][];
            for (int row = 0; row < rows; row++)
                result[row] = ReadColors(source + row * colorsPerRow * sizeof(ushort),
                    colorsPerRow);
            return result;
        }

        PaletteRgb5[] ReadColors(int source, int count)
        {
            byte[] bytes = RomDataReader.ReadFixedBank(bus, source, count * sizeof(ushort));
            var result = new PaletteRgb5[count];
            for (int color = 0; color < count; color++)
            {
                ushort word = (ushort)(bytes[color * 2] | bytes[color * 2 + 1] << 8);
                result[color] = new PaletteRgb5
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
