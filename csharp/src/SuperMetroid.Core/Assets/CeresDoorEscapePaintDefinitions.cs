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
    /// <summary>Largest representable component in the five-bit SNES RGB channels used by the generated escape paint.</summary>
    private const int MaximumChannel = (1 << 5) - 1;
    /// <summary>Shared six-color warm target ramp used for escape-palette entries eight through thirteen.</summary>
    private readonly CeresDoorWarmTargetPaintDefinitions warm;
    /// <summary>Calculated highlight, edge, outline, and bevel colors for the first eight escape-palette entries.</summary>
    private readonly CeresDoorEscapeSurfacePaintDefinitions surface;
    /// <summary>Blue component retained from the separate cyan target color at palette index fourteen.</summary>
    private readonly int accentBlue;
    /// <summary>Authored palette values that differ from the material-based calculation, kept per index.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Builds escape-door paint using the warm target ramp already selected for the normal Ceres palette.</summary>
    /// <param name="colors">Fifteen packed RGB5 palette entries in native order.</param>
    /// <param name="normal">Normal Ceres paint definitions supplying the shared warm target colors.</param>
    internal CeresDoorEscapePaintDefinitions(ReadOnlySpan<ushort> colors, CeresDoorNormalPaintDefinitions normal)
        : this(colors, (normal ?? throw new ArgumentNullException(nameof(normal))).WarmTargets) { }

    /// <summary>Builds escape-door paint with a warm target ramp derived from its own palette entries.</summary>
    /// <param name="colors">Fifteen packed RGB5 palette entries in native order.</param>
    internal CeresDoorEscapePaintDefinitions(ReadOnlySpan<ushort> colors)
        : this(colors, WarmFrom(colors)) { }

    /// <summary>Extracts the six warm target entries used to reconstruct escape-palette indices eight through thirteen.</summary>
    /// <param name="colors">Native-order escape palette, which must contain fifteen entries.</param>
    /// <returns>The warm target color definitions built from entries eight through thirteen.</returns>
    /// <exception cref="ArgumentException">The palette does not contain fifteen colors.</exception>
    private static CeresDoorWarmTargetPaintDefinitions WarmFrom(ReadOnlySpan<ushort> colors)
    {
        if (colors.Length != 15) throw new ArgumentException("Escape door paint requires fifteen colors.", nameof(colors));
        return new(colors.Slice(8, 6));
    }

    /// <summary>Stores the material components and retains only palette entries that differ from their calculated values.</summary>
    /// <param name="colors">Fifteen packed RGB5 entries whose authored differences must remain independently addressable.</param>
    /// <param name="warm">Warm target ramp shared by entries eight through thirteen.</param>
    /// <exception cref="ArgumentException">The palette does not contain fifteen colors.</exception>
    private CeresDoorEscapePaintDefinitions(ReadOnlySpan<ushort> colors, CeresDoorWarmTargetPaintDefinitions warm)
    {
        if (colors.Length != 15) throw new ArgumentException("Escape door paint requires fifteen colors.", nameof(colors));
        this.warm = warm;
        surface = new(colors[..8]);
        accentBlue = colors[14] >> 10;
        for (int index = 0; index < colors.Length; index++)
            if (Calculate(index) != colors[index]) edits.Add(index, colors[index]);
    }

    /// <summary>Returns an authored override or the reconstructed color at one native escape-palette index.</summary>
    /// <param name="index">Zero-based palette entry from zero through fourteen.</param>
    /// <returns>The selected packed RGB5 color.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the fifteen-entry palette.</exception>
    internal ushort ColorAt(int index)
    {
        if ((uint)index >= 15) throw new IndexOutOfRangeException();
        return edits.TryGetValue(index, out ushort edited) ? edited : Calculate(index);
    }

    /// <summary>Reconstructs a palette entry from the surface material, warm target ramp, or cyan target accent.</summary>
    /// <param name="index">Zero-based entry in the fifteen-color palette.</param>
    /// <returns>The packed RGB5 color calculated for that entry.</returns>
    private ushort Calculate(int index)
    {
        if (index is >= 8 and < 14) return warm.ColorAt(index - 8);
        if (index == 14) return (ushort)(MaximumChannel << 5 | accentBlue << 10);
        return surface.ColorAt(index);
    }
}
