namespace SuperMetroid.Core.Assets;

/// <summary>
/// Six shared warm target paints: normal door $A6:F4FE-F509, escape door
/// $A6:F51E-F529 and Norfair Ridley $A6:E1E1-E1EC. The existing selected seven
/// material channels and gold shade steps belong to one paint operation; resource
/// occurrences retain their independently supplied values. These are copied target
/// colors, not a claim about visible door pixels.
/// </summary>
internal sealed class CeresDoorWarmTargetPaintDefinitions
{
    private const int Maximum = (1 << 5) - 1;
    private const int GoldRedStep = 3;
    private const int GoldGreenStep = 6;
    private readonly int highlightBlue, amberRed, amberGreen, red, goldRed, goldGreen, goldBlue;
    private readonly Dictionary<int, ushort> edits = [];

    internal CeresDoorWarmTargetPaintDefinitions(ReadOnlySpan<ushort> colors)
    {
        if (colors.Length != 6) throw new ArgumentException("Warm target paint requires six colors.", nameof(colors));
        highlightBlue = colors[0] >> 10;
        amberRed = colors[1] & Maximum;
        amberGreen = colors[1] >> 5 & Maximum;
        red = colors[2] & Maximum;
        goldRed = colors[3] & Maximum;
        goldGreen = colors[3] >> 5 & Maximum;
        goldBlue = colors[3] >> 10;
        for (int index = 0; index < colors.Length; index++)
            if (Calculate(index) != colors[index]) edits.Add(index, colors[index]);
    }

    internal ushort ColorAt(int index)
    {
        if ((uint)index >= 6) throw new IndexOutOfRangeException();
        return edits.TryGetValue(index, out ushort edited) ? edited : Calculate(index);
    }

    private ushort Calculate(int index)
    {
        if (index == 0) return (ushort)(Maximum | Maximum << 5 | highlightBlue << 10);
        if (index == 1) return (ushort)(amberRed | amberGreen << 5);
        if (index == 2) return (ushort)red;
        int shade = index - 3;
        return (ushort)(Math.Max(0, goldRed - GoldRedStep * shade)
            | Math.Max(0, goldGreen - GoldGreenStep * shade) << 5
            | Math.Max(0, goldBlue * (1 - shade)) << 10);
    }
}
