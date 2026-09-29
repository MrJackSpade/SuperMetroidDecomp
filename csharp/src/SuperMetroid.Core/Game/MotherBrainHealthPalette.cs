using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Ports $AD:E3D5's strict health comparisons and three palette copies.</summary>
public static class MotherBrainHealthPalette
{
    public static void Apply(ISnesAddressSpace bus, SnesCgram cgram, ushort health,
        MotherBrainHealthPalettePresentation? presentation = null)
    {
        int index = health >= MotherBrainHealthPaletteRomData.FirstThreshold ? 0 :
            health >= MotherBrainHealthPaletteRomData.SecondThreshold ? 1 :
            health >= MotherBrainHealthPaletteRomData.FinalThreshold ? 2 : 3;
        (presentation ?? throw new InvalidOperationException(
            "Mother Brain health palettes require installed presentation assets."))
            .Apply(cgram, index);
    }
}
