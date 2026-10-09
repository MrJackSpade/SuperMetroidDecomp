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
    /// <summary>Defines the five-bit channel limit, blue tint increment, and per-step bevel shade reduction.</summary>
    private const int BlueTint = 3, BevelShadeStep = 5, MaximumChannel = (1 << 5) - 1;

    /// <summary>Stores the four independently selected red/green levels for the highlight and edge shades.</summary>
    private readonly int[] highlightAndEdgeLevels;

    /// <summary>Retains the source peak and shadow levels plus the separately authored middle-bevel blue channel.</summary>
    private readonly int bevelPeak, bevelShadow, middleBevelBlue;

    /// <summary>Preserves source colors at indices where the shared surface calculation would otherwise alter them.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Extracts the shared material channels from its eight source colors and records any exact-color exceptions.</summary>
    /// <param name="colors">The eight ordered BGR555 source shades used by the escape-door surface.</param>
    /// <exception cref="ArgumentException">The source does not contain exactly eight colors.</exception>
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

    /// <summary>Returns the exact source or calculated BGR555 shade at one position in the eight-color ramp.</summary>
    /// <param name="index">Zero-based shade position from highlight through bevel shadow.</param>
    /// <returns>The supplied source color when calculation differs, otherwise the shared material result.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the eight-color ramp.</exception>
    internal ushort ColorAt(int index)
    {
        if ((uint)index >= 8) throw new IndexOutOfRangeException();
        return edits.TryGetValue(index, out ushort edited) ? edited : Calculate(index);
    }

    /// <summary>Builds a BGR555 shade from the selected surface levels, clipped bevel ramp, and blue-channel rules.</summary>
    /// <param name="index">Zero-based position in the eight-color material ramp.</param>
    /// <returns>The calculated BGR555 color word.</returns>
    private ushort Calculate(int index)
    {
        int level = index < 4 ? highlightAndEdgeLevels[index] : Math.Max(bevelShadow, bevelPeak - BevelShadeStep * (index - 4));
        int blue = index == 0 ? level : index == 5 ? middleBevelBlue : Math.Min(MaximumChannel, level + BlueTint);
        return (ushort)(level | level << 5 | blue << 10);
    }
}
