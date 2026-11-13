namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where a shinespark crash echo stands before its first update (#1590): where the shot it replaced in its place was, or
/// the room's origin when no shot held the place. The echo is released at the end of Samus's movement, after the
/// projectiles have updated, so the enemies that look for Samus's highest shot that update (a walking Space Pirate's
/// flinch) find the echo where the replaced shot was. The echo's first update places it beside Samus.
/// </summary>
/// <remarks>
/// Consulted once per echo, when the crash's finish releases it onto its place. Evidence: the original echo setup writes
/// the echo's kind, pictures, animation and angle but not its position, which stays as the place's previous shot left it
/// until the echo's update recalculates it.
/// </remarks>
[CompatibilityService("a released crash echo stands where the shot it replaced was until its first update")]
public static class ReleasedEchoStartsAtReplacedShot
{
    /// <summary>The whole position a released echo starts at.</summary>
    /// <param name="replacedShot">The position of the shot the echo replaces, or null when no shot held the place.</param>
    public static Position16<RoomPixels> Start(Position16<RoomPixels>? replacedShot) =>
        replacedShot ?? Position16<RoomPixels>.Zero;
}
