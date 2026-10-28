using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Ridley health colors $A6:E46A-E4BD, uniformly copied by $A6:D495-D4B4.
/// The selected armor/eye color composition tints toward the existing middle
/// membrane paint at $A6:E466, with strengths2/20,3/20,4/20. Recessed armor green
/// holds its healthy shade in the first band; middle plate blue holds the preceding
/// shade in the second band. Those two chosen material/phase memberships preserve
/// the painted shading, with channel magnitudes derived from their previous shade.
/// No new paint magnitudes, health thresholds, timing or pixels are retained here.
/// </summary>
internal sealed class CeresRidleyHealthPaint
{
    /// <summary>Source healthy body and eye paints from which the three damage bands are calculated.</summary>
    private readonly CeresRidleyFade body, eyes;
    /// <summary>Authored departures from the calculated health shades, keyed by row and color.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>Records only input colors that differ from the calculated healthy-to-membrane tint.</summary>
    internal CeresRidleyHealthPaint(RgbColor[][] rows,
        CeresRidleyFade body, CeresRidleyFade eyes)
    {
        this.body = body; this.eyes = eyes;
        for (int row = 0; row < 3; row++) for (int color = 0; color < 14; color++)
            if (Calculate(row, color) != rows[row][color]) edits.Add(new ColourCell(row, color), rows[row][color]);
    }

    /// <summary>Uses an authored override where present, otherwise calculates the selected damage-band shade.</summary>
    internal RgbColor Resolve(int row, int color)
    {
        if ((uint)row >= 3) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)color >= 14) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new ColourCell(row, color), out RgbColor edited) ? edited : Calculate(row, color);
    }

    /// <summary>Tints healthy armor or eyes toward middle membrane paint, preserving two chosen channel shades.</summary>
    private RgbColor Calculate(int row, int color)
    {
        RgbColor healthy = color < 11 ? body.Resolve(15, color) : eyes.Resolve(0, color - 11);
        RgbColor target = body.Resolve(15, 9);
        int strength = 2 + row;
        return healthy.Zip(target, (channel, original, toward) =>
        {
            if (row == 0 && color == 2 && channel == ColorChannel.Green) return original;
            if (row == 1 && color == 5 && channel == ColorChannel.Blue) return Calculate(0, color).Blue;
            return (original * (20 - strength) + toward * strength) / 20;
        });
    }
}
