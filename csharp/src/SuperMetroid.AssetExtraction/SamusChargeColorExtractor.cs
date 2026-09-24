using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports charge-body and Hyper-shot colors in native playback order.</summary>
public static class SamusChargeColorExtractor
{
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
            ushort pointer = RomDataReader.ReadWordFixedBank(bus,
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
            ushort list = RomDataReader.ReadWordFixedBank(bus,
                topAddress + suit * sizeof(ushort));
            var phases = new PaletteRgb5[SamusChargeColorFormat.PhasesPerSuit][];
            for (int phase = 0; phase < phases.Length; phase++)
            {
                ushort pointer = RomDataReader.ReadWordFixedBank(bus,
                    SamusProjectileRomData.Banks.Pose | (list + phase * sizeof(ushort)));
                phases[phase] = ReadColors(pointer);
            }
            return phases;
        }

        PaletteRgb5[] ReadColors(ushort pointer)
        {
            byte[] source = RomDataReader.ReadFixedBank(bus,
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
