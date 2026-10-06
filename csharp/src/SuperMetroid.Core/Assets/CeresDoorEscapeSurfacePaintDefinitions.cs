namespace SuperMetroid.Core.Assets;

/// <summary>
/// Eight grey-blue surface shades shared by $A6:F50E-F51D and $A5:A239-A248.
/// The reviewed escape-door material owns four selected highlight/edge levels,
/// a clipped five-step bevel ramp, blue tint3 and the middle-bevel blue channel.
/// This owner centralizes the identical material; each supplied occurrence and
/// every independently edited channel remains separate. No new paint choice is added.
/// </summary>
internal sealed class CeresDoorEscapeSurfacePaintDefinitions
{
    private const int BlueTint = 3, BevelShadeStep = 5, MaximumChannel = (1 << 5) - 1;
    private readonly int[] highlightAndEdgeLevels;
    private readonly int bevelPeak, bevelShadow, middleBevelBlue;
    private readonly Dictionary<int, ushort> edits = [];

    internal CeresDoorEscapeSurfacePaintDefinitions(ReadOnlySpan<ushort> colors)
    {
        if (colors.Length != 8) throw new ArgumentException("Escape surface requires eight colors.", nameof(colors));
        highlightAndEdgeLevels = new int[4];
        for (int index = 0; index < highlightAndEdgeLevels.Length; index++) highlightAndEdgeLevels[index] = colors[index] & MaximumChannel;
        bevelPeak = colors[4] & MaximumChannel;
        bevelShadow = colors[7] & MaximumChannel;
        middleBevelBlue = colors[5] >> 10;
        for (int index = 0; index < colors.Length; index++)
            if (Calculate(index) != colors[index]) edits.Add(index, colors[index]);
    }

    internal ushort ColorAt(int index)
    {
        if ((uint)index >= 8) throw new IndexOutOfRangeException();
        return edits.TryGetValue(index, out ushort edited) ? edited : Calculate(index);
    }

    private ushort Calculate(int index)
    {
        int level = index < 4 ? highlightAndEdgeLevels[index] : Math.Max(bevelShadow, bevelPeak - BevelShadeStep * (index - 4));
        int blue = index == 0 ? level : index == 5 ? middleBevelBlue : Math.Min(MaximumChannel, level + BlueTint);
        return (ushort)(level | level << 5 | blue << 10);
    }
}