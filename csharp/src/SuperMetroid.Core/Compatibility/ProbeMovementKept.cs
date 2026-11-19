namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What an enemy keeps of a terrain probe taken away from where it stands (#2572). The original takes these probes by
/// moving the enemy itself to the probe's place, moving it there, and moving it back, so whatever the probe's move did
/// that the way back does not undo stays with the enemy:
/// <list type="bullet">
/// <item>
/// A work robot's ledge probe, beside and below its feet, returns the robot to its whole pixels only, so the robot keeps
/// the fractions of a pixel the probe's fall left, its height's included (<see cref="AfterLedgeProbe"/>).
/// </item>
/// <item>
/// A crawling Yard's lookahead probe, along its crawl, takes back only the lookahead, so the Yard keeps the whole of the
/// probe's move toward its surface (<see cref="AfterLookaheadProbe"/>).
/// </item>
/// </list>
/// </summary>
/// <remarks>
/// Consulted once per probe, in the walking robot's and the crawling Yard's update, right after the probe's move.
/// Evidence: the work robot's walk saves its whole X and Y, adds the probe's offset, runs the vertical terrain mover and
/// restores only the saved whole coordinates, leaving the fractions the mover wrote; the Yard's crawl adds its lookahead
/// to its position, runs the mover toward its surface and subtracts the lookahead. The 100% recorded movie replays both
/// enemies' walks.
/// </remarks>
[CompatibilityService("an enemy keeps what a terrain probe away from it moved that the original's way back does not undo: a work robot the fractions its ledge probe left, a crawling Yard its lookahead probe's move toward its surface")]
public static class ProbeMovementKept
{
    /// <summary>
    /// Where a work robot stands after its ledge probe: its own whole pixels, with the fractions of a pixel the probe's
    /// move left.
    /// </summary>
    /// <param name="standing">Where the robot stood before the probe.</param>
    /// <param name="probed">Where the probe stood after its move.</param>
    public static FixedPosition<RoomPixels> AfterLedgeProbe(FixedPosition<RoomPixels> standing, FixedPosition<RoomPixels> probed) =>
        probed.WithWhole(standing.Whole);

    /// <summary>Where a crawling Yard stands after its lookahead probe: where the probe ended, less the lookahead.</summary>
    /// <param name="probed">Where the probe stood after its move.</param>
    /// <param name="lookahead">How far ahead along its crawl the probe stood.</param>
    public static FixedPosition<RoomPixels> AfterLookaheadProbe(FixedPosition<RoomPixels> probed, Offset16<RoomPixels> lookahead) =>
        probed - lookahead;
}
