namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far a Power Bomb's blast grows while it walks its window shapes (#1684, was <c>PowerBombRadius</c>'s "add unless
/// the sum carries"): each update the radius grows by its speed and the speed then changes, but an update whose growth
/// would take the radius to 256 pixels or more changes neither. The radius therefore holds at the last value it reached
/// below 256 pixels, and so does its speed, for the rest of the shapes.
/// </summary>
/// <remarks>
/// Consulted once an update by the flash's yellow shapes and by the blast's white shapes, after the update's window is
/// built and before the radius or its speed changes. Evidence: both stages add the speed to the radius and skip both
/// stores when the sum carries (<c>CLC</c>, <c>ADC</c>, <c>BCS</c> at <c>$88:922F</c>-<c>$88:9233</c> for the flash's
/// yellow shapes and <c>$88:8F3F</c>-<c>$88:8F43</c> for the blast's white shapes). Retail explosions reach it: the
/// flash grows from 146 and a half pixels by about 47 pixels an update and holds at 240 pixels on its last two yellow
/// shapes, and the blast grows from 135 pixels by its accelerating speed and holds at 250 pixels on its last three white
/// shapes, where plain growth would have passed 256 pixels. The blast's radius is the reach enemies, Draygon's goop and
/// the blocks are tested against, so the hold changes what a Power Bomb reaches. The Power Bomb growth hold check
/// compares every radius with speeds either side of the hold, and every speed with radii either side of it, with the
/// original's sixteen-bit sum; the Power Bomb radius check runs whole explosions against the original's arithmetic.
/// </remarks>
[CompatibilityService("a Power Bomb's blast stops growing short of 256 pixels while it walks its shapes: an update whose growth would reach 256 pixels leaves the radius and its speed as they were")]
internal static class PowerBombGrowthHold
{
    /// <summary>The whole pixels of radius a growth may not reach: a growth that would reach them holds instead.</summary>
    internal const int HoldingPixels = 256;

    /// <summary>
    /// Whether an update's growth that would take the radius to <paramref name="grownWholePixels"/> holds, leaving the
    /// radius and its speed as they were.
    /// </summary>
    /// <param name="grownWholePixels">The whole pixels the radius plus its speed would reach, not negative.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="grownWholePixels"/> is negative.</exception>
    internal static bool Holds(int grownWholePixels) =>
        Ensure.BetweenInclusive(0, int.MaxValue, grownWholePixels) >= HoldingPixels;
}
