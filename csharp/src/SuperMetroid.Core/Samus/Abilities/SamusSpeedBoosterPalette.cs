using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Shows a Speed Booster cycle step as Samus's suit colours (#1512).</summary>
internal static class SamusSpeedBoosterPalette
{
    /// <summary>Shows <paramref name="palette"/>, one of her suit's own Speed Booster cycle palettes.</summary>
    internal static void Apply(SamusColours colours, SamusFullBodyCyclePalette palette)
    {
        Ensure.NotNull(colours);
        colours.ShowSuit(SamusShades.FullBodyCycle(palette));
    }
}
