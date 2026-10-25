namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The colliding projectile's or Samus's edges as <c>$A0:9A5A/$9B7F</c> compare them, whether the walker selects the shot
/// (strict) or touch (inclusive) callback (#633), and whether the room has installed enemy artwork. The corners are room
/// pixels whose sixteen-bit wrap is part of the comparison: the walker's signed differences are taken between wrapped
/// edges.
/// </summary>
/// <param name="TopLeft">The target's left and top edges.</param>
/// <param name="BottomRight">The target's right and bottom edges.</param>
/// <param name="SelectShotCallback">Whether the walker selects the shot callback with strict edges, rather than the touch callback with inclusive ones.</param>
/// <param name="ArtworkInstalled">Whether the room has installed enemy artwork.</param>
[IndependentFlag("independent probe facts: whether a shot callback is selected and whether artwork is installed")]
internal readonly record struct EnemyHitboxProbe(
    Position16<RoomPixels> TopLeft, Position16<RoomPixels> BottomRight, bool SelectShotCallback, bool ArtworkInstalled)
{
    /// <summary>
    /// The shared literal `$A0:9A5A/$9B7F` rectangle test. The shot and touch walkers have different strict/inclusive
    /// boundaries in the cartridge.
    /// </summary>
    internal bool Overlaps(int left, int top, int right, int bottom) =>
        SelectShotCallback
            ? !EnemyMath.IsNegative16(BottomRight.X - left) &&
              EnemyMath.IsNegative16(TopLeft.X - right) &&
              !EnemyMath.IsNegative16(BottomRight.Y - top) &&
              EnemyMath.IsNegative16(TopLeft.Y - bottom)
            : EnemyMath.IsNegative16(left - BottomRight.X) &&
              !EnemyMath.IsNegative16(right - TopLeft.X) &&
              EnemyMath.IsNegative16(top - BottomRight.Y) &&
              !EnemyMath.IsNegative16(bottom - TopLeft.Y);
}
