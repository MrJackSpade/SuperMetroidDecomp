using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the four cartridge-authored full-body palette cycles without their timing rules.</summary>
public static class SamusFullBodyCycleColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return SamusFullBodyCycleColorCatalog.Write(new SamusFullBodyCycleColorDocument
        {
            Version = SamusFullBodyCycleColorFormat.Version,
            SpeedBooster = ReadFamily(SamusFullBodyCycleFamily.SpeedBooster),
            ScrewAttack = ReadFamily(SamusFullBodyCycleFamily.ScrewAttack),
            StoredShine = ReadFamily(SamusFullBodyCycleFamily.StoredShine),
            ActiveShinespark = ReadFamily(SamusFullBodyCycleFamily.ActiveShinespark),
        });

        PaletteRgb5[][][] ReadFamily(SamusFullBodyCycleFamily family)
        {
            var suits = new PaletteRgb5[SamusFullBodyCycleColorFormat.SuitCount][][];
            for (int suit = 0; suit < suits.Length; suit++)
            {
                suits[suit] = new PaletteRgb5[SamusFullBodyCycleColorFormat.ShadesPerSuit][];
                for (int shade = 0; shade < suits[suit].Length; shade++)
                {
                    ushort pointer = SamusFullBodyCycleColorFormat.Pointer(family, suit, shade);
                    byte[] source = RomDataReader.ReadFixedBank(bus,
                        SamusPaletteRomData.Banks.Palette | pointer,
                        SamusFullBodyCycleColorFormat.ColorsPerPalette * sizeof(ushort));
                    var colors = new PaletteRgb5[SamusFullBodyCycleColorFormat.ColorsPerPalette];
                    for (int index = 0; index < colors.Length; index++)
                    {
                        ushort word = (ushort)(source[index * 2] | source[index * 2 + 1] << 8);
                        colors[index] = new PaletteRgb5
                        {
                            Red = word & 31,
                            Green = word >> 5 & 31,
                            Blue = word >> 10 & 31,
                        };
                    }
                    suits[suit][shade] = colors;
                }
            }
            return suits;
        }
    }
}
