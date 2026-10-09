using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports charge-body and Hyper-shot colors in native playback order.</summary>
public static class SamusChargeColorExtractor
{
    /// <summary>Exports charged-beam and pseudo-Screw body palettes by suit, plus Hyper-shot flash palettes in native playback order.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for bank-$91 suit/phase pointer lists and their bank-$9B sixteen-color payloads.</param>
    /// <returns>A new UTF-8 JSON buffer with three suits, six phases per charge/pseudo-Screw family, and ten Hyper-shot frames; all colors use RGB5 channels 0..31.</returns>
    /// <remarks>Suit order is Power, Varia, Gravity. Hyper-shot pointers are read in descending table-offset order $14..$02, not ascending source order. Full palette rows include color zero; bit 15 is not represented. Charge thresholds, phase cadence, and projectile behavior are not exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var charged = new PaletteRgb5[SamusChargeColorFormat.SuitCount][][];
        var pseudo = new PaletteRgb5[SamusChargeColorFormat.SuitCount][][];
        for (int suit = 0; suit < SamusChargeColorFormat.SuitCount; suit++)
        {
            charged[suit] = ReadSixPhaseList(
                SamusProjectileRomData.Palettes.BeamChargePointers, suit);
            pseudo[suit] = ReadSixPhaseList(
                SamusProjectileRomData.Palettes.PseudoScrewPointers, suit);
        }
        var hyper = new PaletteRgb5[SamusChargeColorFormat.HyperFrameCount][];
        for (int frame = 0; frame < hyper.Length; frame++)
        {
            int tableOffset = SamusChargePalettePointerDefinitions.LastHyperTableByteOffset -
                frame * sizeof(ushort);
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                SamusProjectileRomData.Palettes.HyperBeamShotPointers + tableOffset);
            hyper[frame] = ReadColors(pointer);
        }
        return SamusChargeColorCatalog.Write(new SamusChargeColorDocument
        {
            Version = SamusChargeColorFormat.Version,
            ChargedBeam = charged,
            PseudoScrew = pseudo,
            HyperShot = hyper,
        });

        PaletteRgb5[][] ReadSixPhaseList(int topAddress, int suit)
        {
            ushort list = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                topAddress + suit * sizeof(ushort));
            var phases = new PaletteRgb5[SamusChargeColorFormat.PhasesPerSuit][];
            for (int phase = 0; phase < phases.Length; phase++)
            {
                ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    SamusProjectileRomData.Banks.Pose | (list + phase * sizeof(ushort)));
                phases[phase] = ReadColors(pointer);
            }
            return phases;
        }

        PaletteRgb5[] ReadColors(ushort pointer)
        {
            byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                SamusProjectileRomData.Banks.PaletteAndTrailData | pointer,
                SamusChargeColorFormat.ColorsPerPalette * sizeof(ushort));
            var result = new PaletteRgb5[SamusChargeColorFormat.ColorsPerPalette];
            for (int index = 0; index < result.Length; index++)
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
