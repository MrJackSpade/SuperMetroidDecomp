namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a Multiviola's heading bounces off terrain (#1607): instead of mirroring, the heading moves into the neighbouring
/// quarter turn on the far side of the surface and keeps its place within its quarter. Off a wall it crosses the vertical
/// (up-right and up-left trade places, as do down-left and down-right); off a floor or ceiling it crosses the horizontal
/// (up-right and down-right trade places, as do up-left and down-left). A heading halfway through its quarter, the
/// diagonal, bounces exactly as a mirror would; any other heading trades a shallow course for a steep one, so a
/// Multiviola started off the diagonal bounces around the room along the original's paths.
/// </summary>
/// <remarks>
/// Consulted after each blocked horizontal or vertical move of a Multiviola's update, before its next update rebuilds its
/// velocity from the heading. Evidence: the original movement toggles the angle's two quarter-turn bits on a blocked move
/// (a quarter turn's after a horizontal block, three quarters' after a vertical one) instead of subtracting it from a half
/// or a whole turn; the Multiviola bounce check compares every heading with that toggle.
/// </remarks>
[CompatibilityService("a Multiviola's heading bounces into the neighbouring quarter turn across the surface, keeping its place within the quarter")]
public static class MultiviolaQuarterBounce
{
    /// <summary>The quarter turns of a full turn.</summary>
    private const int QuartersPerTurn = 4;

    /// <summary><paramref name="heading"/> after a wall blocks the Multiviola: its horizontal direction reversed.</summary>
    public static TrigonometryAngle OffWall(TrigonometryAngle heading) =>
        InQuarter(heading, QuarterOf(heading) % 2 == 0 ? QuarterOf(heading) + 1 : QuarterOf(heading) - 1);

    /// <summary><paramref name="heading"/> after a floor or ceiling blocks the Multiviola: its vertical direction reversed.</summary>
    public static TrigonometryAngle OffFloorOrCeiling(TrigonometryAngle heading) =>
        InQuarter(heading, QuartersPerTurn - 1 - QuarterOf(heading));

    /// <summary>The quarter turn, 0 (up-right) through 3 (down-right), <paramref name="heading"/> lies in.</summary>
    private static int QuarterOf(TrigonometryAngle heading) => heading.RawValue / TrigonometryAngle.QuarterTurn.RawValue;

    /// <summary><paramref name="heading"/>'s place within its quarter turn, carried into quarter <paramref name="quarter"/>.</summary>
    private static TrigonometryAngle InQuarter(TrigonometryAngle heading, int quarter) =>
        new(quarter * TrigonometryAngle.QuarterTurn.RawValue + heading.RawValue % TrigonometryAngle.QuarterTurn.RawValue);
}
