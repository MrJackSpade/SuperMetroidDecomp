namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where the Shitroid sits while it drains Samus (#1620, was <c>ShitroidBehavior</c>'s "X carry"): it shakes over her
/// at its shake offset, and one pixel further right on every update her vertical speed's whole pixels read four or more
/// as a count, which is also every update her speed has been slowed past zero at the top of a rise. A drain that starts
/// while she falls fast shows the Shitroid a pixel right for that update (the drain then slows her to two pixels an
/// update), and one that starts as a rise slows past zero keeps it a pixel right while her speed stays there.
/// </summary>
/// <remarks>
/// Consulted on every update of the drain, after its speed clamp reads Samus's vertical speed and before the Shitroid is
/// placed over her. Evidence: the drain (<c>$A9:F21B</c>) compares the vertical speed with four, unsigned, then
/// branches on the sign of the difference for its clamp, and adds the shake offset to Samus's X without clearing the
/// carry that comparison left (<c>$A9:F24D</c>); the 100% movie's drain reaches a speed slowed past zero (#1562). The
/// Shitroid drain carry check runs the drain from the Big Boy room with Samus falling, rising and slowed past zero, and
/// the drain shift check compares every speed with that comparison.
/// </remarks>
[CompatibilityService("the draining Shitroid sits one pixel right of its shake while Samus's vertical speed reads four pixels or more, or slowed past zero")]
internal static class DrainingShitroidFallShift
{
    /// <summary>The whole pixels of vertical speed from which the Shitroid sits a pixel right.</summary>
    private const int ShiftingSpeed = 4;

    /// <summary>The pixels the Shitroid sits right of its shake offset: one when the speed reads four or more, else none.</summary>
    /// <param name="samusVerticalSpeedWholePixels">Samus's vertical speed's whole pixels, 0 through 65535, as her speed holds them.</param>
    /// <exception cref="ArgumentOutOfRangeException">The speed lies outside its range.</exception>
    internal static int PixelsRightOfShake(int samusVerticalSpeedWholePixels) =>
        Ensure.BetweenInclusive(0, ushort.MaxValue, samusVerticalSpeedWholePixels) >= ShiftingSpeed ? 1 : 0;
}
