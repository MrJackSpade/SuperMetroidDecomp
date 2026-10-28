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
    /// <summary>Extracted colors that differ from the computed contour at a row and color index.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>Builds the shared contours from the first native row and retains only differences in later rows.</summary>
    internal CeresRidleyFloorPaint(RgbColor[][] rows)
    {
        body = new(rows[0].AsSpan(0, 11));
        eyes = CeresRidleyFade.EyePaint.From(rows[0].AsSpan(11, 3));
        for (int row = 0; row < 9; row++) for (int color = 0; color < 15; color++)
            if (Calculate(row, color) != rows[row][color]) edits.Add(new ColourCell(row, color), rows[row][color]);
    }

    /// <summary>$A6:B123 (also header E16D) equals the canonical Golden low contour at $84:8050; no duplicate grey input.</summary>
    private static RgbColor Neutral => GoldenTorizoHealthPaint.Color(0, 15, rear: false);
    /// <summary>$A6:A9FF: the same neutral endpoint at retreat exposure1/15.</summary>
    internal static RgbColor RetreatNeutral => CeresRidleyFade.Scale(Neutral, 1);

    /// <summary>Uses a reviewed native color override when present, otherwise scales the shared contour for this zoom row.</summary>
    internal RgbColor Resolve(int row, int color)
    {
        if ((uint)row >= 9) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)color >= 15) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new ColourCell(row, color), out RgbColor edited) ? edited : Calculate(row, color);
    }

    /// <summary>Applies one exposure step through bucket six and two steps for each remaining zoom bucket.</summary>
    private RgbColor Calculate(int row, int color)
    {
        RgbColor endpoint = color < 11 ? body.Color(color) : color < 14 ? eyes.Color(color - 11) : Neutral;
        // Each zoom bucket reduces exposure once through bucket6, then twice per bucket.
        int exposure = 15 - Math.Min(row, 6) - 2 * Math.Max(0, row - 6);
        return CeresRidleyFade.Scale(endpoint, exposure);
    }
}
