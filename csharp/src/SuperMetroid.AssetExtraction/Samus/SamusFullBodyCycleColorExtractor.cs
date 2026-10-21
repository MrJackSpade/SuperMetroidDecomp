using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Exports the four cartridge-authored full-body palette cycles without their timing rules.</summary>
[CpuBusBoundary("codec: ROM Samus full-body palette cycles → full-body cycle colors")]
public static class SamusFullBodyCycleColorExtractor
{
    /// <summary>Exports Speed Booster, Screw Attack, stored-shine, and active-Shinespark full-body color families without their state/timing rules.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for bank-$9B palette rows selected by compiled family/suit/shade pointers.</param>
    /// <returns>A new UTF-8 JSON buffer with four families, three suits per family, four shades per suit, and sixteen RGB5 colors per shade.</returns>
    /// <remarks>Suits are ordered Power, Varia, Gravity; colors retain complete row order including transparent color zero, with channels 0..31 and bit 15 omitted. Extracted shades are artwork selections, not a replacement for native playback cadence.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
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
                    ushort pointer = checked((ushort)(family switch
                    {
                        SamusFullBodyCycleFamily.SpeedBooster => SamusCartridgeTables.SpeedBoosterCyclePalettes,
                        SamusFullBodyCycleFamily.StoredShine => SamusCartridgeTables.StoredShineCyclePalettes,
                        SamusFullBodyCycleFamily.ActiveShinespark => SamusCartridgeTables.ActiveShinesparkCyclePalettes,
                        SamusFullBodyCycleFamily.ScrewAttack => SamusCartridgeTables.ScrewAttackCyclePalettes,
                        _ => throw new ArgumentOutOfRangeException(nameof(family)),
                    } + suit * SamusCartridgeTables.CyclePaletteSuitStride + shade * SamusCartridgeTables.CyclePaletteShadeStride));
                    byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                        SamusPaletteRomData.Banks.Palette | pointer,
                        SamusFullBodyCycleColorFormat.ColorsPerPalette * sizeof(ushort));
                    var colors = new PaletteRgb5[SamusFullBodyCycleColorFormat.ColorsPerPalette];
                    for (int index = 0; index < colors.Length; index++)
                    {
                        ushort word = SnesLittleEndian.Word(source[index * 2], source[index * 2 + 1]);
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
