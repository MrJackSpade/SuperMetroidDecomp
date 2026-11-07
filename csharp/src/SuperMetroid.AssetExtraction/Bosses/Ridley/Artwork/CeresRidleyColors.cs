using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>
/// Ceres Ridley's installed colours at work: composing and resolving the retreat colours, which the import shades his
/// room and sprites in.
/// </summary>
internal static class CeresRidleyColors
{
    /// <summary>Returns one BGR555 retreat BG color, index 0..14, preserving supplied edits over the composition.</summary>
    internal static RgbColor RetreatBg(CeresRidleyColorCatalog colors, int color)
    {
        if ((uint)color >= CeresRidleyColorFormat.RetreatBgColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return colors.selection.RetreatBg.TryGetValue(new PaintColour(0, color), out RgbColor edited)
            ? edited
            : CalculateRetreatBg(colors.selection.BodyFade, colors.selection.EyeFade, color);
    }

    /// <summary>Composes the retreat background from body-fade shade one, eye-fade shade fourteen, and its neutral final color.</summary>
    internal static RgbColor CalculateRetreatBg(CeresRidleyFade bodyFade, CeresRidleyFade eyeFade, int color) =>
        color < 11 ? bodyFade.Resolve(1, color)
            : color < 14 ? eyeFade.Resolve(14, color - 11) : CeresRidleyFloorPaint.RetreatNeutral;

    /// <summary>Returns one BGR555 retreat shared color, index 0..7; unedited values reuse startup colors 1..8.</summary>
    internal static RgbColor RetreatShared(CeresRidleyColorCatalog colors, int color)
    {
        if ((uint)color >= CeresRidleyColorFormat.RetreatSharedColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return colors.selection.RetreatShared.TryGetValue(new PaintColour(0, color), out RgbColor edited) ? edited : colors.selection.Start.Resolve(color + 1);
    }
}
