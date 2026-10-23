using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Applies installed Speed Booster cycle colors or native overrun words to Samus's suit palette.</summary>
internal static class SamusSpeedBoosterPalette
{
    /// <summary>
    /// Copies the selected installed cycle into CGRAM. The native expansion and grapple-code
    /// overruns instead copy their operand-driven bus words through the 15-bit CGRAM port,
    /// preserving the unrolled cartridge read behavior.
    /// </summary>
    internal static void Apply(SamusColours colours, SpeedBoosterPaletteSource source,
        SamusFullBodyCycleColorCatalog? colors, SamusOverreadCatalog? overreads)
    {
        if (source.Overrun is { } overrun)
        {
            for (int index = 0; index < SamusPaletteRomData.Common.ColorsPerObjPalette; index++)
            {
                // The native unrolled copy uses LDA $0000..001E,X; the expansion window's open
                // bus returns the operand's high byte for every word.
                ushort word = overrun switch
                {
                    SpeedBoosterPaletteOverrun.Expansion => SpeedBoosterPaletteOverrunDefinitions.ExpansionOpenBusWord,
                    SpeedBoosterPaletteOverrun.GrappleCode => (overreads ?? throw new InvalidOperationException(
                            "The Speed Booster grapple-code overrun needs the installed Samus overread catalog."))
                        .SpeedBoosterGrappleCodeWord(index),
                    _ => throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterPaletteOverrun)} {(int)overrun}."),
                };
                // Arbitrary bus words reach CGRAM through its fifteen-bit data port.
                colours.Suit.Write(index, Bgr555.FromCgramPortWord(word));
            }
            return;
        }
        (colors ?? throw new InvalidOperationException(
            "Speed Booster palette requires installed Samus full-body cycle colors.")).Apply(colours, source.Palette ??
                throw new InvalidOperationException("A Speed Booster palette source names either a cycle palette or an overrun."));
    }
}
