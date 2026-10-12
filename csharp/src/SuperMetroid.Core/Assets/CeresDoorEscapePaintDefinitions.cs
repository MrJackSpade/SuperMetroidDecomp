using SuperMetroid.Core.Hardware;

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
    /// <summary>Stores maximum channel.</summary>
    private const int MaximumChannel = (1 << 5) - 1;
    /// <summary>Stores warm.</summary>
    private readonly CeresDoorWarmTargetPaintDefinitions warm;
    /// <summary>Stores surface.</summary>
    private readonly CeresDoorEscapeSurfacePaintDefinitions surface;
    /// <summary>Stores accent blue.</summary>
    private readonly int accentBlue;
    /// <summary>Stores edits.</summary>
    private readonly Dictionary<int, Bgr555> edits = [];

    /// <summary>Initializes a new ceres door escape paint definitions instance.</summary>
    internal CeresDoorEscapePaintDefinitions(ReadOnlySpan<Bgr555> colors, CeresDoorNormalPaintDefinitions normal)
        : this(colors, (Ensure.NotNull(normal)).WarmTargets) { }

    /// <summary>Initializes a new ceres door escape paint definitions instance.</summary>
    internal CeresDoorEscapePaintDefinitions(ReadOnlySpan<Bgr555> colors)
        : this(colors, WarmFrom(colors)) { }

    /// <summary>Warm from.</summary>
    private static CeresDoorWarmTargetPaintDefinitions WarmFrom(ReadOnlySpan<Bgr555> colors)
    {
        if (colors.Length != 15) throw new ArgumentException("Escape door paint requires fifteen colors.", nameof(colors));
        return new(colors.Slice(8, 6));
    }

    /// <summary>Initializes a new ceres door escape paint definitions instance.</summary>
    private CeresDoorEscapePaintDefinitions(ReadOnlySpan<Bgr555> colors, CeresDoorWarmTargetPaintDefinitions warm)
    {
        if (colors.Length != 15) throw new ArgumentException("Escape door paint requires fifteen colors.", nameof(colors));
        this.warm = warm;
        surface = new(colors[..8]);
        accentBlue = colors[14].Blue;
        for (int index = 0; index < colors.Length; index++)
            if (Calculate(index) != colors[index]) edits.Add(index, colors[index]);
    }

    /// <summary>Color at.</summary>
    internal Bgr555 ColorAt(int index)
    {
        if ((uint)index >= 15) throw new IndexOutOfRangeException();
        return edits.TryGetValue(index, out Bgr555 edited) ? edited : Calculate(index);
    }

    /// <summary>Calculates.</summary>
    private Bgr555 Calculate(int index)
    {
        if (index is >= 8 and < 14) return warm.ColorAt(index - 8);
        if (index == 14) return new(0, MaximumChannel, accentBlue);
        return surface.ColorAt(index);
    }
}
