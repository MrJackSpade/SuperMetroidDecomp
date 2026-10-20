namespace SuperMetroid.Core.Areas.Ceres.Artwork;

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
    /// <summary>Five-bit maximum used by the escape accent's saturated green channel.</summary>
    private const int MaximumChannel = (1 << 5) - 1;
    /// <summary>Six warm target colors reused from the normal Ceres door paint.</summary>
    internal readonly CeresDoorWarmTargetPaintDefinitions warm;
    /// <summary>Eight grey-blue surface colors composing the escape door face.</summary>
    internal readonly CeresDoorEscapeSurfacePaintDefinitions surface;
    /// <summary>Blue component of the terminal cyan target-paint slot.</summary>
    private readonly int accentBlue;
    /// <summary>Extracted colors that differ from the derived surface and warm-target paint.</summary>
    internal readonly Dictionary<int, Bgr555> edits = [];

    /// <summary>Uses the normal door's warm targets when compiling the escape paint.</summary>
    internal CeresDoorEscapePaintDefinitions(ReadOnlySpan<Bgr555> colors, CeresDoorNormalPaintDefinitions normal)
        : this(colors, (Ensure.NotNull(normal)).WarmTargets) { }

    /// <summary>Derives warm targets from the supplied escape palette when no normal palette is provided.</summary>
    internal CeresDoorEscapePaintDefinitions(ReadOnlySpan<Bgr555> colors)
        : this(colors, WarmFrom(colors)) { }

    /// <summary>Selects six warm entries from a complete fifteen-color escape palette.</summary>
    private static CeresDoorWarmTargetPaintDefinitions WarmFrom(ReadOnlySpan<Bgr555> colors)
    {
        if (colors.Length != 15) throw new ArgumentException("Escape door paint requires fifteen colors.", nameof(colors));
        return new(colors.Slice(8, 6));
    }

    /// <summary>Captures the derived material paint and retains every differing supplied ink as an edit.</summary>
    private CeresDoorEscapePaintDefinitions(ReadOnlySpan<Bgr555> colors, CeresDoorWarmTargetPaintDefinitions warm)
    {
        if (colors.Length != 15) throw new ArgumentException("Escape door paint requires fifteen colors.", nameof(colors));
        this.warm = warm;
        surface = new(colors[..8]);
        accentBlue = colors[14].Blue;
        for (int index = 0; index < colors.Length; index++)
            if (Calculate(index) != colors[index]) edits.Add(index, colors[index]);
    }

    /// <summary>Selects an extracted edit when present, otherwise resolves the derived surface or target color.</summary>
    internal Bgr555 ColorAt(int index)
    {
        if ((uint)index >= 15) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(index, out Bgr555 edited) ? edited : Calculate(index);
    }

    /// <summary>Combines surface inks, shared warm targets, and the saturated cyan terminal slot.</summary>
    private Bgr555 Calculate(int index)
    {
        if (index is >= 8 and < 14) return warm.ColorAt(index - 8);
        if (index == 14) return new(0, MaximumChannel, accentBlue);
        return surface.ColorAt(index);
    }
}
