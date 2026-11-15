using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Shows a Speed Booster cycle step as Samus's suit colours (#1512).</summary>
internal static class SamusSpeedBoosterGlow
{
    /// <summary>Shows <paramref name="shade"/>, one of her suit's own Speed Booster cycle shades.</summary>
    internal static void Apply(SamusColours colours, SamusFullBodyCycleShade palette)
    {
        Ensure.NotNull(colours);
        colours.ShowSuit(SamusShades.FullBodyCycle(palette));
    }
}
