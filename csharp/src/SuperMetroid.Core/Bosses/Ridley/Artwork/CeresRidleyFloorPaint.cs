using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Zoom-linked Ridley paints $A6:B107-B224. Fourteen endpoint paints share the
/// reviewed body/eye definitions. Native $A6:B0EF-B106 copies all fifteen colors.
/// Neutral level3 is bounded copied metadata: no pixel in the entire 256-character
/// native C1:8DA9 Mode7 domain references slot15, yet its CGRAM copy is observable.
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
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>Builds the shared contours from the first native step and retains only differences in later steps.</summary>
    internal CeresRidleyFloorPaint(RgbColor[][] steps)
    {
        body = new(steps[0].AsSpan(0, 11));
        eyes = CeresRidleyFade.EyePaint.From(steps[0].AsSpan(11, 3));
        for (int step = 0; step < CeresRidleyPaletteDefinitions.GetawayZoomStepCount; step++)
        for (int color = 0; color < CeresRidleyPaletteDefinitions.GetawayZoomColorCount; color++)
            if (Calculate(step, color) != steps[step][color]) edits.Add(new ColourCell(step, color), steps[step][color]);
    }

    /// <summary>$A6:B123 (also header E16D) equals the canonical Golden low contour at $84:8050; no duplicate grey input.</summary>
    private static RgbColor Neutral => GoldenTorizoHealthPaint.Color(0, 15, rear: false);
    /// <summary>$A6:A9FF: the same neutral endpoint at retreat exposure1/15.</summary>
    internal static RgbColor RetreatNeutral => CeresRidleyFade.Scale(Neutral, 1);

    /// <summary>Uses a reviewed native color override when present, otherwise scales the shared contour for this zoom step.</summary>
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
