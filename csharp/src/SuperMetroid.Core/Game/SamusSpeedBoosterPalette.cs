using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Installs Samus's Speed Booster palette, including the cartridge's two pointer-overrun cases.</summary>
internal static class SamusSpeedBoosterPalette
{
    /// <summary>
    /// Copies the selected colors into CGRAM. Ordinary pointers use the installed full-body
    /// cycle catalog; the expansion and Grapple pointer overruns reproduce the native
    /// operand-driven reads or instruction-word values instead.
    /// </summary>
    /// <param name="bus">Address space used for the native expansion-overrun reads.</param>
    /// <param name="cgram">CGRAM destination for the Samus object palette.</param>
    /// <param name="pointer">Resolved bank-$9B palette pointer, including either recognized overrun sentinel.</param>
    /// <param name="colors">Installed full-body cycle colors used for ordinary palette pointers.</param>
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
                cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + index, word);
            }
            return;
        }
        (colors ?? throw new InvalidOperationException(
            "Speed Booster palette requires installed Samus full-body cycle colors.")).Apply(cgram, pointer);
    }
}
