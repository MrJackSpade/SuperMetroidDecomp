namespace SuperMetroid.Core.Assets;

/// <summary>
/// Escape material paint at $A6:F50E-F52B. Selected highlight/edge brightness,
/// clipped inner-bevel ramp and hue choices specify the grey-blue material composition.
/// Native pixels1..8 distinguish highlights, edges, outlines and inner facets. Slot15
/// is separately preserved cyan target paint, not claimed as visible door pixels.
/// The uniform target copy at $A6:F717-F726 does not choose these material shades.
/// Six warm target colors share the normal palette; every supplied edit stays independent.
/// </summary>
internal sealed class CeresDoorEscapePaintDefinitions
{
    private const int BlueTint = 3;
    private const int BevelShadeStep = 5;
    private const int MaximumChannel = (1 << 5) - 1;
    private readonly CeresDoorWarmTargetPaintDefinitions warm;
    private readonly int[] highlightAndEdgeLevels;
    private readonly int bevelPeak;
    private readonly int bevelShadow;
    private readonly int middleBevelBlue;
    private readonly int accentBlue;
    private readonly Dictionary<int, ushort> edits = [];

    internal CeresDoorEscapePaintDefinitions(ReadOnlySpan<ushort> colors, CeresDoorNormalPaintDefinitions normal)
        : this(colors, (normal ?? throw new ArgumentNullException(nameof(normal))).WarmTargets) { }

    internal CeresDoorEscapePaintDefinitions(ReadOnlySpan<ushort> colors)
        : this(colors, WarmFrom(colors)) { }

    private static CeresDoorWarmTargetPaintDefinitions WarmFrom(ReadOnlySpan<ushort> colors)
    {
        if (colors.Length != 15) throw new ArgumentException("Escape door paint requires fifteen colors.", nameof(colors));
        return new(colors.Slice(8, 6));
    }

    private CeresDoorEscapePaintDefinitions(ReadOnlySpan<ushort> colors, CeresDoorWarmTargetPaintDefinitions warm)
    {
        if (colors.Length != 15) throw new ArgumentException("Escape door paint requires fifteen colors.", nameof(colors));
        this.warm = warm;
        highlightAndEdgeLevels = new int[4];
        for (int index = 0; index < highlightAndEdgeLevels.Length; index++) highlightAndEdgeLevels[index] = colors[index] & MaximumChannel;
        bevelPeak = colors[4] & MaximumChannel;
        bevelShadow = colors[7] & MaximumChannel;
        middleBevelBlue = colors[5] >> 10;
        accentBlue = colors[14] >> 10;
        for (int index = 0; index < colors.Length; index++)
            if (Calculate(index) != colors[index]) edits.Add(index, colors[index]);
    }

    internal ushort ColorAt(int index)
    {
        if ((uint)index >= 15) throw new IndexOutOfRangeException();
        return edits.TryGetValue(index, out ushort edited) ? edited : Calculate(index);
    }

    private ushort Calculate(int index)
    {
        if (index is >= 8 and < 14) return warm.ColorAt(index - 8);
        if (index == 14) return (ushort)(MaximumChannel << 5 | accentBlue << 10);
        int level = index < 4 ? highlightAndEdgeLevels[index] : Math.Max(bevelShadow, bevelPeak - BevelShadeStep * (index - 4));
        int blue = index == 0 ? level : index == 5 ? middleBevelBlue : Math.Min(MaximumChannel, level + BlueTint);
        return (ushort)(level | level << 5 | blue << 10);
    }
}
