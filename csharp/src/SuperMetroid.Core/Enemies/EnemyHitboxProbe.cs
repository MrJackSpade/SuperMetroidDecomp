namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The colliding projectile's or Samus's edges as the frame-hitbox tests compare them, whether the walker selects the shot
/// (strict) or touch (inclusive) reaction (#633), and whether the room has installed enemy frame displays. The corners are room
/// pixels whose wrap past 65535 is part of the comparison: the walker's signed differences are taken between wrapped
/// edges.
/// </summary>
/// <param name="TopLeft">The target's left and top edges.</param>
/// <param name="BottomRight">The target's right and bottom edges.</param>
/// <param name="SelectsShot">Whether the walker selects the shot reaction with strict edges, rather than the touch reaction with inclusive ones.</param>
/// <param name="ArtworkInstalled">Whether the room has installed enemy frame displays.</param>
[IndependentFlag("independent probe facts: whether a shot reaction is selected and whether artwork is installed")]
internal readonly record struct EnemyHitboxProbe(
    Position16<RoomPixels> TopLeft, Position16<RoomPixels> BottomRight, bool SelectsShot, bool ArtworkInstalled)
{
    /// <summary>
    /// The shared literal frame-hitbox rectangle test. The shot and touch walkers have different strict/inclusive
    /// boundaries in the game.
    /// </summary>
    internal bool Overlaps(int left, int top, int right, int bottom) =>
        SelectsShot
            ? !CoordinateAxis.Precedes(BottomRight.X, left) &&
              CoordinateAxis.Precedes(TopLeft.X, right) &&
              !CoordinateAxis.Precedes(BottomRight.Y, top) &&
              CoordinateAxis.Precedes(TopLeft.Y, bottom)
            : CoordinateAxis.Precedes(left, BottomRight.X) &&
              !CoordinateAxis.Precedes(right, TopLeft.X) &&
              CoordinateAxis.Precedes(top, BottomRight.Y) &&
              !CoordinateAxis.Precedes(bottom, TopLeft.Y);
}
