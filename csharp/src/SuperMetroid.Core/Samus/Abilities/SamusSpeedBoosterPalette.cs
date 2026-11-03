using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Shows a Speed Booster cycle step as Samus's suit colours (#1512).</summary>
internal static class SamusSpeedBoosterPalette
{
    /// <summary>
    /// Shows the cycle palette <paramref name="source"/> names, or for a step past Gravity's cycle, the colours the
    /// import decoded for that step.
    /// </summary>
    internal static void Apply(SamusColours colours, SpeedBoosterPaletteSource source)
    {
        Ensure.NotNull(colours);
        if (source.Overrun is { } overrun)
        {
            colours.ShowSuit(SamusShades.SpeedBoosterOverrun(overrun));
            return;
        }
        colours.ShowSuit(SamusShades.FullBodyCycle(source.Palette ??
            throw new InvalidOperationException("A Speed Booster palette source names either a cycle palette or an overrun.")));
    }
}
