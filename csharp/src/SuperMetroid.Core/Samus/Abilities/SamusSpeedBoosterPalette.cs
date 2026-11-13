using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Shows a Speed Booster cycle step as Samus's suit colours (#1512).</summary>
internal static class SamusSpeedBoosterPalette
{
    /// <summary>Shows the cycle palette <paramref name="source"/> names.</summary>
    internal static void Apply(SamusColours colours, SpeedBoosterPaletteSource source)
    {
        Ensure.NotNull(colours);
        colours.ShowSuit(SamusShades.FullBodyCycle(source.Palette));
    }
}
