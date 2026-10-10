using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

internal static class SamusSpeedBoosterPalette
{
    internal static void Apply(ISnesAddressSpace bus, SnesCgram cgram, ushort pointer,
        SamusFullBodyCycleColorCatalog? colors)
    {
        if (pointer is SpeedBoosterPaletteOverrunDefinitions.ExpansionPointer or SpeedBoosterPaletteOverrunDefinitions.GrappleCodePointer)
        {
            for (int index = 0; index < SamusPaletteRomData.Common.ColorsPerObjPalette; index++)
            {
                // The native unrolled copy uses LDA $0000..001E,X. Preserve its
                // operand-driven bus value for expansion reads, not a generic zero fallback.
                ushort word = pointer == SpeedBoosterPaletteOverrunDefinitions.ExpansionPointer
                    ? SnesCpuOperandRead.ReadAbsoluteIndexedWord(bus,
                        (byte)(SamusPaletteRomData.Banks.Palette >> 16), (ushort)(index * 2), pointer)
                    : SpeedBoosterPaletteOverrunDefinitions.GrappleInstructionWord(index);
                // Arbitrary bus words reach CGRAM through its fifteen-bit data port.
                cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + index, Bgr555.FromCgramPortWord(word));
            }
            return;
        }
        (colors ?? throw new InvalidOperationException(
            "Speed Booster palette requires installed Samus full-body cycle colors.")).Apply(cgram, pointer);
    }
}
