using SuperMetroid.Core.Hardware;

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
    private readonly Dictionary<int, Bgr555> edits = [];

    internal CeresDoorEscapeSurfacePaintDefinitions(ReadOnlySpan<Bgr555> colors)
    {
        if (colors.Length != 8) throw new ArgumentException("Escape surface requires eight colors.", nameof(colors));
        highlightAndEdgeLevels = new int[4];
        for (int index = 0; index < highlightAndEdgeLevels.Length; index++) highlightAndEdgeLevels[index] = colors[index].Red;
        bevelPeak = colors[4].Red;
        bevelShadow = colors[7].Red;
        middleBevelBlue = colors[5].Blue;
        for (int index = 0; index < colors.Length; index++)
            if (Calculate(index) != colors[index]) edits.Add(index, colors[index]);
    }

    internal Bgr555 ColorAt(int index)
    {
        if ((uint)index >= 8) throw new IndexOutOfRangeException();
        return edits.TryGetValue(index, out Bgr555 edited) ? edited : Calculate(index);
    }

    private Bgr555 Calculate(int index)
    {
        int level = index < 4 ? highlightAndEdgeLevels[index] : Math.Max(bevelShadow, bevelPeak - BevelShadeStep * (index - 4));
        int blue = index == 0 ? level : index == 5 ? middleBevelBlue : Math.Min(MaximumChannel, level + BlueTint);
        return new(level, level, blue);
    }
}