using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Exports bounded Ceres Ridley color tables without exposing AI or fade timing.</summary>
public static class CeresRidleyColorExtractor
{
    /// <summary>Reads the bounded Ceres Ridley start, eye/body fade, health, alarm, retreat, and Baby color tables and serializes them as RGB5 rows.</summary>
    /// <param name="bus">Supported-cartridge address space containing the compiled Ceres Ridley palette tables.</param>
    /// <returns>UTF-8 JSON bytes for the versioned Ceres Ridley color catalog.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        return CeresRidleyColorFile.Write(new CeresRidleyColorDocument
        {
            Version = (int)CeresRidleyColorVersion.Current,
            Start = ReadColors(RidleySourceDefinitions.Ceres.StartColors,
                CeresRidleyPaletteDefinitions.StartColorCount),
            EyeFade = ReadRows(EnemySourceDefinitions.CeresRidleyEyeFadeRows,
                CeresRidleyPaletteDefinitions.EyeFadeRowCount,
                CeresRidleyPaletteDefinitions.EyeFadeColorCount),
            BodyFade = ReadRows(RidleySourceDefinitions.Ceres.BodyFadeColors,
                CeresRidleyPaletteDefinitions.BodyFadeRowCount,
                CeresRidleyPaletteDefinitions.BodyFadeColorCount),
            Health = ReadRows(RidleySourceDefinitions.Ceres.HealthColors,
                CeresRidleyPaletteDefinitions.HealthRowCount,
                CeresRidleyPaletteDefinitions.HealthColorCount),
            Alarm = ReadRows(RidleySourceDefinitions.Ceres.AlarmColors,
                CeresRidleyPaletteDefinitions.AlarmRowCount,
                CeresRidleyPaletteDefinitions.AlarmColorCount),
            RetreatBg = ReadColors(RidleySourceDefinitions.Ceres.RetreatBgColors,
                CeresRidleyPaletteDefinitions.RetreatBgColorCount),
            RetreatShared = ReadColors(RidleySourceDefinitions.Ceres.RetreatSharedColors,
                CeresRidleyPaletteDefinitions.RetreatSharedColorCount),
            Baby = ReadRows(RidleySourceDefinitions.Ceres.BabyColors,
                CeresRidleyPaletteDefinitions.BabyRowCount,
                CeresRidleyPaletteDefinitions.BabyColorCount),
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
            byte[] bytes = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), source, count * sizeof(ushort));
            var result = new PaletteRgb5[count];
            for (int color = 0; color < count; color++)
            {
                ushort word = SnesLittleEndian.Word(bytes[color * 2], bytes[color * 2 + 1]);
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
