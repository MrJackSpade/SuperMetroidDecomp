namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// Six shared warm target paints: normal door $A6:F4FE-F509, escape door
/// $A6:F51E-F529 and Norfair Ridley $A6:E1E1-E1EC. The existing selected seven
/// material channels and gold shade steps belong to one paint operation; resource
/// occurrences retain their independently supplied values. These are copied target
/// colors, not a claim about visible door pixels.
/// </summary>
internal sealed class CeresDoorWarmTargetPaintDefinitions
{
    /// <summary>Largest five-bit BGR component used by the white door highlight.</summary>
    private const int Maximum = (1 << 5) - 1;
    /// <summary>Red-channel decrement between successive computed gold shades.</summary>
    private const int GoldRedStep = 3;
    /// <summary>Green-channel decrement between successive computed gold shades.</summary>
    private const int GoldGreenStep = 6;
    /// <summary>Independent paint channels retained from the six supplied target colors so shared shade relationships survive edits.</summary>
    private readonly int highlightBlue, amberRed, amberGreen, red, goldRed, goldGreen, goldBlue;
    /// <summary>Per-color overrides for supplied targets that do not fit the shared warm-paint calculation.</summary>
    internal readonly Dictionary<int, Bgr555> edits = [];

    /// <summary>Validates six supplied target colors, captures their shared channels, and preserves independently edited colors as overrides.</summary>
    internal CeresDoorWarmTargetPaintDefinitions(ReadOnlySpan<Bgr555> colors)
    {
        Ensure.LengthEqual(colors, 6);
        highlightBlue = colors[0].Blue;
        amberRed = colors[1].Red;
        amberGreen = colors[1].Green;
        red = colors[2].Red;
        goldRed = colors[3].Red;
        goldGreen = colors[3].Green;
        goldBlue = colors[3].Blue;
        for (int index = 0; index < colors.Length; index++)
            if (Calculate(index) != colors[index]) edits.Add(index, colors[index]);
    }

    /// <summary>Returns an independently supplied target when present, otherwise computes the matching warm highlight, amber, red, or gold shade.</summary>
    internal Bgr555 ColorAt(int index)
    {
        if ((uint)index >= 6) throw new IndexOutOfRangeException();
        return edits.TryGetValue(index, out Bgr555 edited) ? edited : Calculate(index);
    }

    /// <summary>Builds the white, amber, red, and three descending gold targets from shared five-bit channel parameters.</summary>
    private Bgr555 Calculate(int index)
    {
        if (index == 0) return new(Maximum, Maximum, highlightBlue);
        if (index == 1) return new(amberRed, amberGreen, 0);
        if (index == 2) return new(red, 0, 0);
        int shade = index - 3;
        return new(Math.Max(0, goldRed - GoldRedStep * shade),
            Math.Max(0, goldGreen - GoldGreenStep * shade),
            Math.Max(0, goldBlue * (1 - shade)));
    }
}
