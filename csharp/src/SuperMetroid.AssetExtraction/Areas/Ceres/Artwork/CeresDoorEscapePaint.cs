using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>
/// Escape material paint. Selected highlight/edge brightness,
/// clipped inner-bevel ramp and hue choices specify the grey-blue material composition.
/// Pixels1..8 distinguish highlights, edges, outlines and inner facets. Slot15
/// is separately preserved cyan target paint, not claimed as visible door pixels.
/// The uniform target copy does not choose these material shades.
/// Six warm target colors share the normal palette; every supplied edit stays independent.
/// </summary>
internal sealed class CeresDoorEscapePaint
{
    /// <summary>Five-bit maximum used by the escape accent's saturated green channel.</summary>
    private const int MaximumChannel = (1 << 5) - 1;
    /// <summary>Six warm target colors reused from the normal Ceres door paint.</summary>
    internal readonly CeresDoorWarmTargetPaint warm;
    /// <summary>Eight grey-blue surface colors composing the escape door face.</summary>
    internal readonly CeresDoorEscapeSurfacePaint surface;
    /// <summary>Blue component of the terminal cyan target-paint slot.</summary>
    private readonly int accentBlue;
    /// <summary>Extracted colors that differ from the derived surface and warm-target paint.</summary>
    internal readonly Dictionary<PaintColour, RgbColor> edits;

    /// <summary>Keeps the derived material paint and every differing supplied ink as an edit.</summary>
    /// <param name="warm">The warm targets the escape palette shares.</param>
    /// <param name="surface">The grey-blue surface paint.</param>
    /// <param name="accentBlue">The terminal cyan slot's blue.</param>
    /// <param name="edits">The supplied inks the import found differing from the derived paint.</param>
    internal CeresDoorEscapePaint(CeresDoorWarmTargetPaint warm, CeresDoorEscapeSurfacePaint surface, int accentBlue,
        Dictionary<PaintColour, RgbColor> edits)
    {
        this.warm = warm;
        this.surface = surface;
        this.accentBlue = accentBlue;
        this.edits = edits;
    }

    /// <summary>Selects an extracted edit when present, otherwise resolves the derived surface or target color.</summary>
    internal RgbColor ColorAt(int index)
    {
        if ((uint)index >= 15) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(new PaintColour(0, index), out RgbColor edited) ? edited : Calculate(index);
    }

    /// <summary>Combines surface inks, shared warm targets, and the saturated cyan terminal slot.</summary>
    private RgbColor Calculate(int index)
    {
        if (index is >= 8 and < 14) return warm.ColorAt(index - 8);
        if (index == 14) return new FadeLevelColour(0, MaximumChannel, accentBlue).ToColour();
        return surface.ColorAt(index);
    }
}
