namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The colliding projectile's or Samus's edges as the frame-hitbox tests compare them, whether the test selects the shot
/// (strict) or touch (inclusive) reaction (#633). The corners are room
/// pixels whose wrap past 65535 is part of the comparison: the test's signed differences are taken between wrapped
/// edges.
/// </summary>
/// <param name="TopLeft">The target's left and top edges.</param>
/// <param name="BottomRight">The target's right and bottom edges.</param>
/// <param name="SelectsShot">Whether the test selects the shot reaction with strict edges, rather than the touch reaction with inclusive ones.</param>
internal readonly record struct EnemyHitboxProbe(
    Position16<RoomPixels> TopLeft, Position16<RoomPixels> BottomRight, bool SelectsShot)
{
    /// <summary>
    /// The edges of a target at <paramref name="position"/> reaching <paramref name="radius"/> each way, for the
    /// <paramref name="contactKind"/> the test selects.
    /// </summary>
    internal static EnemyHitboxProbe Around(Position16<RoomPixels> position, Extent16<RoomPixels> radius, HitboxContactKind contactKind) =>
        new(
            TopLeft: new(CoordinateAxis.Wrap(position.X - radius.X), CoordinateAxis.Wrap(position.Y - radius.Y)),
            BottomRight: new(CoordinateAxis.Wrap(position.X + radius.X), CoordinateAxis.Wrap(position.Y + radius.Y)),
            SelectsShot: contactKind == HitboxContactKind.Shot);

    /// <summary>
    /// The shared frame-hitbox rectangle test. Shots and touch meet the rectangle's edges differently in the game:
    /// strictly and inclusively.
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
