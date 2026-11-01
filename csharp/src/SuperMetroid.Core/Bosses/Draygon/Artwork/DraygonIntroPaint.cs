using SuperMetroid.Core.Areas.Ceres.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Copied uniformly to colours 144..168 by InitAI_DraygonBody.
/// First16 words alias the primary material, the next is its clear
/// target, and the final8 alias escape-door surface paint.
/// These existing source identities introduce no new independent paint inputs.
/// Installed occurrences retain independent edits, including the copied clear target.
/// </summary>
internal sealed class DraygonIntroPaint
{
    /// <summary>First sixteen installed colors, shared in identity with Draygon's primary material.</summary>
    internal readonly DraygonMaterialPaint primary;
    /// <summary>Final eight colors, aliased to the Ceres escape-door surface material.</summary>
    internal readonly CeresDoorEscapeSurfacePaint surface;
    /// <summary>Independent edit of the copied clear target, or null when it retains the value.</summary>
    internal readonly RgbColor? clearEdit;

    /// <summary>Keeps the 25 copied colours as their primary, clear-target, and door-surface aliases.</summary>
    /// <param name="primary">The first sixteen colours, as Draygon's primary material.</param>
    /// <param name="surface">The final eight colours, as the Ceres escape-door surface material.</param>
    /// <param name="clearEdit">The edited clear target, or null when it keeps the material's clear target.</param>
    internal DraygonIntroPaint(DraygonMaterialPaint primary, CeresDoorEscapeSurfacePaint surface, RgbColor? clearEdit)
    {
        this.primary = primary;
        this.surface = surface;
        this.clearEdit = clearEdit;
    }

    /// <summary>Resolves a copied color through its owning material while preserving an edited clear target.</summary>
    internal RgbColor Color(int index)
    {
        if ((uint)index >= 25) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 16) return primary.Color(index);
        if (index == 16) return clearEdit ?? DraygonMaterialPaint.ClearTarget;
        return surface.ColorAt(index - 17);
    }
}
