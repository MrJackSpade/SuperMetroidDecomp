namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The colliding projectile's or Samus's edges as the frame-hitbox tests compare them, whether the test selects the shot
/// (strict) or touch (inclusive) reaction (#633).
/// </summary>
/// <remarks>
/// The edges are room pixels on the room's axis, and the test compares them the short way across it
/// (<see cref="CoordinateAxis.Precedes"/>), because both sides can lie past the room's left or top edge (#2537). The
/// target is one of Samus's shots or bombs, or Samus herself: a shot lives on up to 64 pixels past the view's left and
/// top edges and a Wave shot passes through the room's walls, so with the view at the room's left or top edge a shot can
/// lie beyond it, and Samus lies above the room's top on a departing up-elevator (#2430); a target near either edge has
/// an edge past it even while its centre is inside. The rectangle tested is the hitbox around an enemy, and an enemy can
/// lie above or left of the room's origin (a fly that has climbed through the ceiling, a Bull braking past Samus), with a
/// hitbox reaching past the edge from inside it besides. Each edge past the room's left or top edge stays on the room's
/// axis, as <see cref="CoordinateAxis.Wrap"/> keeps it, and the comparison meets it with the other side the
/// short way, as <see cref="EnemyCollision.RadiusBoxesOverlap"/> meets the same targets by their radii.
/// </remarks>
/// <param name="TopLeft">The target's left and top edges.</param>
/// <param name="BottomRight">The target's right and bottom edges.</param>
/// <param name="SelectsShot">Whether the test selects the shot reaction with strict edges, rather than the touch reaction with inclusive ones.</param>
internal readonly record struct EnemyHitboxProbe(
    Position16<RoomPixels> TopLeft, Position16<RoomPixels> BottomRight, bool SelectsShot)
{
    /// <summary>
    /// The edges of a target at <paramref name="position"/> reaching <paramref name="radius"/> each way, for the
    /// <paramref name="contactKind"/> the test selects: an edge past the room's left or top edge kept on the room's axis,
    /// for the targets the probe's remarks name.
    /// </summary>
    internal static EnemyHitboxProbe Around(Position16<RoomPixels> position, Extent16<RoomPixels> radius, HitboxContactKind contactKind) =>
        new(
            TopLeft: new(CoordinateAxis.Wrap(position.X - radius.X), CoordinateAxis.Wrap(position.Y - radius.Y)),
            BottomRight: new(CoordinateAxis.Wrap(position.X + radius.X), CoordinateAxis.Wrap(position.Y + radius.Y)),
            SelectsShot: contactKind == HitboxContactKind.Shot);

    /// <summary>
    /// The shared frame-hitbox rectangle test. Shots and touch meet the rectangle's edges differently in the game:
    /// strictly and inclusively. The rectangle's edges, which the species compute from an enemy's place and its hitbox,
    /// and the target's are compared the short way across the room's axis, for the targets and enemies the probe's
    /// remarks name.
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
