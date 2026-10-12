using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Applies installed Speed Booster cycle colors or native overrun words to Samus's suit palette.</summary>
internal static class SamusSpeedBoosterPalette
{
    /// <summary>
    /// Copies the selected installed cycle into CGRAM. Native expansion and grapple-code
    /// overrun pointers instead copy their operand-driven bus words through the 15-bit
    /// CGRAM port, preserving the unrolled cartridge read behavior.
    /// </summary>
    internal static void Apply(ISnesAddressSpace bus, SnesCgram cgram, ushort pointer,
        SamusFullBodyCycleColorCatalog? colors)
    {
        if (Enum.IsDefined((SpeedBoosterPaletteOverrun)pointer))
        {
            var overrun = (SpeedBoosterPaletteOverrun)pointer;
            for (int index = 0; index < SamusPaletteRomData.Common.ColorsPerObjPalette; index++)
            {
                // The native unrolled copy uses LDA $0000..001E,X. Preserve its
                // operand-driven bus value for expansion reads, not a generic zero fallback.
                ushort word = overrun switch
                {
                    SpeedBoosterPaletteOverrun.Expansion => new SamusOverreadMemory(bus).SpeedBoosterExpansionWord(index, pointer),
                    SpeedBoosterPaletteOverrun.GrappleCode => SpeedBoosterPaletteOverrunDefinitions.GrappleInstructionWord(index),
                    _ => throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterPaletteOverrun)} {(int)overrun}."),
                };
                // Arbitrary bus words reach CGRAM through its fifteen-bit data port.
                new SamusPalettes(cgram).Suit.Write(index, Bgr555.FromCgramPortWord(word));
            }
            return;
        }
        (colors ?? throw new InvalidOperationException(
            "Speed Booster palette requires installed Samus full-body cycle colors.")).Apply(cgram, pointer);
    }
}
