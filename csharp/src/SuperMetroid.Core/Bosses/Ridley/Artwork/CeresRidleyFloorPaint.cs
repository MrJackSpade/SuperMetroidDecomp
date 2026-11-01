using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Zoom-linked Ridley paints. Fourteen endpoint paints share the
/// reviewed body/eye definitions. The getaway copies all fifteen colors.
/// Neutral level3 is bounded copied metadata: no pixel in the entire 256-character
/// rotating floor references slot15, yet its colour copy is observable.
/// Its grey endpoint reuses the reviewed Golden Torizo contour; two-rate zoom exposure
/// is this exact composition,
/// not a visible-paint claim or exemption for pixels, motion or timing.
/// </summary>
internal sealed class CeresRidleyFloorPaint
{
    /// <summary>The reviewed body contour shared by every zoom bucket.</summary>
    private readonly CeresRidleyBodyPaint body;
    /// <summary>The reviewed eye contour shared by every zoom bucket.</summary>
    private readonly CeresRidleyFade.EyePaint eyes;
    /// <summary>Extracted colors that differ from the computed contour at a step and color index.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits;

    /// <summary>Keeps the shared contours and the colors that differ from them.</summary>
    /// <param name="body">The body contour.</param>
    /// <param name="eyes">The eye contour.</param>
    /// <param name="edits">The supplied colors the import found differing from the scaled contours.</param>
    internal CeresRidleyFloorPaint(CeresRidleyBodyPaint body, CeresRidleyFade.EyePaint eyes, Dictionary<ColourCell, RgbColor> edits)
    {
        this.body = body;
        this.eyes = eyes;
        this.edits = edits;
    }

    /// <summary>The neutral floor colour (also in the header palette) equals the canonical Golden low contour; no duplicate grey input.</summary>
    private static RgbColor Neutral => GoldenTorizoHealthPaint.Color(0, 15, rear: false);
    /// <summary>The same neutral endpoint at retreat exposure1/15.</summary>
    internal static RgbColor RetreatNeutral => CeresRidleyFade.Scale(Neutral, 1);

    /// <summary>Uses a reviewed color override when present, otherwise scales the shared contour for this zoom step.</summary>
    internal RgbColor Resolve(int step, int color)
    {
        if ((uint)step >= CeresRidleyPaletteDefinitions.GetawayZoomStepCount) throw new ArgumentOutOfRangeException(nameof(step));
        if ((uint)color >= CeresRidleyPaletteDefinitions.GetawayZoomColorCount) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new ColourCell(step, color), out RgbColor edited) ? edited : Calculate(step, color);
    }

    /// <summary>Applies one exposure step through bucket six and two steps for each remaining zoom bucket.</summary>
    private RgbColor Calculate(int step, int color)
    {
        RgbColor endpoint = color < 11 ? body.Color(color) : color < 14 ? eyes.Color(color - 11) : Neutral;
        // Each zoom bucket reduces exposure once through bucket6, then twice per bucket.
        int exposure = 15 - Math.Min(step, 6) - 2 * Math.Max(0, step - 6);
        return CeresRidleyFade.Scale(endpoint, exposure);
    }
}
