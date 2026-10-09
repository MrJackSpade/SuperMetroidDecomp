namespace SuperMetroid.Core.Assets;

/// <summary>The logo's background endpoint at $8C:F1E9 has eight evenly spaced grey
/// shades in slots1..8, rounded to the nearest RGB5 level over seven intervals.
/// Slots0 and9..15 are black. The original endpoints26 and4 produce
/// 26,23,20,17,13,10,7,4. Only the two endpoint levels are retained as the drawing's
/// chosen contrast range, confirmed against the original $99:ECC4 logo map and
/// $99:E089 tiles; an invented contrast rule would change that artwork. All intermediate
/// shades and temporal fades are computed. Endpoint values remain supplied, not defaults.</summary>
internal sealed class EndingLogoBackgroundPalette
{
    /// <summary>RGB5 grey endpoints for the ramp: <c>light</c> anchors slot one and <c>dark</c> anchors slot eight.</summary>
    private readonly int light, dark;

    /// <summary>Creates the computed grey ramp from its two retained endpoint levels.</summary>
    /// <param name="light">RGB5 channel level for slot one.</param>
    /// <param name="dark">RGB5 channel level for slot eight.</param>
    private EndingLogoBackgroundPalette(int light, int dark)
    {
        this.light = light;
        this.dark = dark;
    }

    /// <summary>Recognize every supplied color, preserving non-grey or non-linear edits
    /// as caller-owned data rather than substituting a gradient for them.</summary>
    internal static EndingLogoBackgroundPalette? TryCreate(ReadOnlySpan<ushort> colors)
    {
        if (colors.Length != 16) return null;
        var gradient = new EndingLogoBackgroundPalette(colors[1] & 31, colors[8] & 31);
        for (int color = 0; color < 16; color++)
            if (gradient.Color(color) != colors[color]) return null;
        return gradient;
    }

    /// <summary>Slots1..8 interpolate two grey endpoints with nearest rounding:
    /// (light*(8-color)+dark*(color-1)+3)/7. No half-way tie is possible with divisor7.</summary>
    internal ushort Color(int color)
    {
        if ((uint)color >= 16) throw new ArgumentOutOfRangeException(nameof(color));
        if (color is < 1 or > 8) return 0;
        int level = (light * (8 - color) + dark * (color - 1) + 3) / 7;
        return (ushort)(level | level << 5 | level << 10);
    }
}
