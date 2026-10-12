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
    /// <summary>Native blue tint, bevel decrement, and RGB5 ceiling used by the calculated door material.</summary>
    private const int BlueTint = 3, BevelShadeStep = 5, MaximumChannel = (1 << 5) - 1;
    /// <summary>Four independently supplied highlight and edge brightness levels shared by matching door surfaces.</summary>
    private readonly int[] highlightAndEdgeLevels;
    /// <summary>Selected bevel endpoints and middle blue channel retained from the supplied eight-color surface.</summary>
    private readonly int bevelPeak, bevelShadow, middleBevelBlue;
    /// <summary>Per-color exceptions kept when supplied art differs from the calculated shared material.</summary>
    private readonly Dictionary<int, Bgr555> edits = [];

    /// <summary>Captures the selected color parameters and preserves every independently edited supplied color.</summary>
    internal CeresDoorEscapeSurfacePaintDefinitions(ReadOnlySpan<Bgr555> colors)
    {
        Ensure.LengthEqual(colors, 8);
        highlightAndEdgeLevels = new int[4];
        for (int index = 0; index < highlightAndEdgeLevels.Length; index++) highlightAndEdgeLevels[index] = colors[index].Red;
        bevelPeak = colors[4].Red;
        bevelShadow = colors[7].Red;
        middleBevelBlue = colors[5].Blue;
        for (int index = 0; index < colors.Length; index++)
            if (Calculate(index) != colors[index]) edits.Add(index, colors[index]);
    }

    /// <summary>Returns an authored exception where present or the calculated grey-blue door shade.</summary>
    internal Bgr555 ColorAt(int index)
    {
        if ((uint)index >= 8) throw new IndexOutOfRangeException();
        return edits.TryGetValue(index, out Bgr555 edited) ? edited : Calculate(index);
    }

    /// <summary>Builds the clipped five-step bevel ramp and blue tint, preserving the selected middle-bevel blue level.</summary>
    private Bgr555 Calculate(int index)
    {
        int level = index < 4 ? highlightAndEdgeLevels[index] : Math.Max(bevelShadow, bevelPeak - BevelShadeStep * (index - 4));
        int blue = index == 0 ? level : index == 5 ? middleBevelBlue : Math.Min(MaximumChannel, level + BlueTint);
        return new(level, level, blue);
    }
}
