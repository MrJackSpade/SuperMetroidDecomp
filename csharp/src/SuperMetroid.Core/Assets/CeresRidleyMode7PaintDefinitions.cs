namespace SuperMetroid.Core.Assets;

/// <summary>
/// Zoom-linked Ridley paints $A6:B107-B224. Fourteen endpoint paints share the
/// reviewed body/eye definitions. Native $A6:B0EF-B106 copies all fifteen colors.
/// Neutral level3 is bounded copied metadata: no pixel in the entire 256-character
/// native C1:8DA9 Mode7 domain references slot15, yet its CGRAM copy is observable.
/// Its grey endpoint reuses the reviewed Golden Torizo contour; two-rate zoom exposure
/// is this exact composition,
/// not a visible-paint claim or exemption for pixels, motion or timing.
/// </summary>
internal sealed class CeresRidleyMode7PaintDefinitions
{
    /// <summary>Reviewed Ridley body endpoint colors used for the first eleven entries in each zoom row.</summary>
    private readonly CeresRidleyBodyPaintDefinitions body;
    /// <summary>Reviewed eye endpoint colors used for entries eleven through thirteen.</summary>
    private readonly CeresRidleyFadeColorDefinitions.EyePaint eyes;
    /// <summary>Sparse authored color words retained when a supplied zoom row differs from calculated endpoint scaling.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>
    /// Builds the nine zoom rows from shared body and eye endpoints while preserving any supplied
    /// color words that differ from the calculated composition.
    /// </summary>
    /// <param name="rows">Nine rows of fifteen BGR555 words, ordered from close view through retreat exposure.</param>
    internal CeresRidleyMode7PaintDefinitions(ushort[][] rows)
    {
        body = new(rows[0].AsSpan(0, 11));
        eyes = CeresRidleyFadeColorDefinitions.EyePaint.From(rows[0].AsSpan(11, 3));
        for (int row = 0; row < 9; row++) for (int color = 0; color < 15; color++)
            if (Calculate(row, color) != rows[row][color]) edits.Add(row * 15 + color, rows[row][color]);
    }

    /// <summary>$A6:B123 (also header E16D) equals the canonical Golden low contour at $84:8050; no duplicate grey input.</summary>
    private static ushort Neutral => GoldenTorizoHealthPaintDefinitions.Color(0, 15, rear: false);
    /// <summary>$A6:A9FF: the same neutral endpoint at retreat exposure1/15.</summary>
    internal static ushort RetreatNeutral => CeresRidleyFadeColorDefinitions.Scale(Neutral, 1);

    /// <summary>Returns an authored zoom-row color when present, otherwise the color calculated from the shared endpoints.</summary>
    /// <param name="row">Zoom exposure row, from zero through eight.</param>
    /// <param name="color">Color slot within the row: body, eye, or neutral slot fifteen.</param>
    /// <returns>The BGR555 paint word selected for that row and slot.</returns>
    internal ushort Resolve(int row, int color)
    {
        if ((uint)row >= 9) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)color >= 15) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(row * 15 + color, out ushort edited) ? edited : Calculate(row, color);
    }

    /// <summary>Scales the appropriate body, eye, or neutral endpoint according to the native zoom-row exposure.</summary>
    private ushort Calculate(int row, int color)
    {
        ushort endpoint = color < 11 ? body.Color(color) : color < 14 ? eyes.Color(color - 11) : Neutral;
        // Each zoom bucket reduces exposure once through bucket6, then twice per bucket.
        int exposure = 15 - Math.Min(row, 6) - 2 * Math.Max(0, row - 6);
        return CeresRidleyFadeColorDefinitions.Scale(endpoint, exposure);
    }
}
