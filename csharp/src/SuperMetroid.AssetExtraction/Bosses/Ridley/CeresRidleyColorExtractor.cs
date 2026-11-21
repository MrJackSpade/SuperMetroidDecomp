using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

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
            Version = CeresRidleyColorFormat.Version,
            Start = ReadColors(RidleySourceDefinitions.Ceres.StartColors,
                CeresRidleyColorFormat.StartColorCount),
            EyeFade = ReadRows(EnemySourceDefinitions.CeresRidleyEyeFadeRows,
                CeresRidleyColorFormat.EyeFadeShadeCount,
                CeresRidleyColorFormat.EyeFadeColorCount),
            BodyFade = ReadRows(RidleySourceDefinitions.Ceres.BodyFadeColors,
                RidleyShadeDefinitions.BodyFadeShadeCount,
                CeresRidleyColorFormat.BodyFadeColorCount),
            Health = Rgb5Rows.KeyedInTableOrder(RidleySourceDefinitions.Ceres.HealthTableOrder, band => band.Key,
                row => ReadColors(RidleySourceDefinitions.Ceres.HealthColors + row * CeresRidleyColorFormat.HealthColorCount * sizeof(ushort),
                    CeresRidleyColorFormat.HealthColorCount)),
            Alarm = ReadRows(RidleySourceDefinitions.Ceres.AlarmColors,
                CeresRidleyColorFormat.AlarmStepCount,
                CeresRidleyColorFormat.AlarmColorCount),
            RetreatBg = ReadColors(RidleySourceDefinitions.Ceres.RetreatBgColors,
                CeresRidleyColorFormat.RetreatBgColorCount),
            RetreatShared = ReadColors(RidleySourceDefinitions.Ceres.RetreatSharedColors,
                CeresRidleyColorFormat.RetreatSharedColorCount),
            BabyShades = CeresBabyCartridgeShades.FromSteps(ReadRows(RidleySourceDefinitions.Ceres.BabyColors,
                CeresBabyCartridgeShades.BabyShadeCount,
                CeresRidleyColorFormat.BabyColorCount)),
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
                int word = LittleEndianBytes.Word(bytes[color * 2], bytes[color * 2 + 1]);
                result[color] = PaletteRgb5.From(RgbColor.FromWordIgnoringTopBit(word));
            }
            return result;
        }
    }
}
