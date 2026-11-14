using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Calculates the map reveal's edge speeds and its moving updates from the stock label origin.</summary>
internal static class FileSelectMapWindowMotions
{
    /// <summary>
    /// Reveal edge speeds and moving updates for the six Zebes areas.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all authored values and their consumers.
    /// Let D=max(x,256-x,y,256-y) for the area's stock label position.
    /// An edge at distance d travels floor(256*d/D)/64 pixels per update, negative for
    /// left/top. Quantize the positive magnitude before negating, then promote to signed
    /// 1/65536 pixels. The farthest edge travels exactly four pixels per update. The edges move on
    /// floor(D/4)+1 updates, and the reveal completes on the last of them.
    /// The generation square is 256 high even though the edges stop at the screen's bottom.
    /// Integer numerators are at most 65536; the division comes before the speed is scaled to sixty-fourths.
    /// Stock origins intentionally remain independent of editable label positions: edits
    /// move the initial window but did not change compiled motion in the prior contract.
    /// No generated velocity table or runtime read of game data is used.
    /// </remarks>
    public static FileSelectMapWindowMotion Get(AreaId area)
    {
        Position16<ScreenPixels> origin = StockOrigin(area);
        int x = origin.X, y = origin.Y;
        int farthest = Math.Max(Math.Max(x, 256 - x), Math.Max(y, 256 - y));
        // The speed is a whole number of sixty-fourths of a pixel: four 256ths each.
        SignedSixteenSixteen Velocity(int distance, int sign) =>
            SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(sign * (256 * distance / farthest) * 4);
        return new(farthest / 4 + 1, Velocity(x, -1), Velocity(256 - x, 1),
            Velocity(y, -1), Velocity(256 - y, 1));
    }

    /// <summary>
    /// Selects the stock geographic label anchor by named Zebes area.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 as semantic area-to-anchor cases. Label
    /// drawing first resolves the area's identity, then selects its
    /// X/Y pair; window setup selects the same pair. Area numbers are
    /// categorical identities, not samples of a coordinate curve. The cases express
    /// that layout selection directly. Each coordinate field is checked against its
    /// complete authored six-value view. Ceres and other unsupported identities reject.
    /// These stock inputs determine compiled motion; independently editable installed
    /// label anchors determine the actual starting rectangle.
    /// </remarks>
    internal static Position16<ScreenPixels> StockOrigin(AreaId area) => area switch
    {
        AreaId.Crateria => new Position16<ScreenPixels>(91, 50),
        AreaId.Brinstar => new Position16<ScreenPixels>(42, 127),
        AreaId.Norfair => new Position16<ScreenPixels>(94, 181),
        AreaId.WreckedShip => new Position16<ScreenPixels>(206, 80),
        AreaId.Maridia => new Position16<ScreenPixels>(206, 159),
        AreaId.Tourian => new Position16<ScreenPixels>(135, 139),
        AreaId.Ceres => throw new ArgumentOutOfRangeException(nameof(area), area, "Only the six Zebes areas have world-map transitions."),
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, "Only the six Zebes areas have world-map transitions.")
    };
}
