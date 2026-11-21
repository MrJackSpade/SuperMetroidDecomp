namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The colliding projectile's or Samus's edges as the frame-hitbox tests compare them, whether the test selects the shot
/// (strict) or touch (inclusive) reaction (#633).
/// </summary>
/// <remarks>
/// The edges are signed room pixels, and the test compares them the short way across the room's coordinates
/// (<see cref="DistanceAcrossRoomEdge.LiesBefore"/>), because both sides can lie past the room's left or top edge (#2537). The
/// target is one of Samus's shots or bombs, or Samus herself: a shot lives on up to 64 pixels past the view's left and
/// top edges and a Wave shot passes through the room's walls, so with the view at the room's left or top edge a shot can
/// lie beyond it, and Samus lies above the room's top on a departing up-elevator (#2430); a target near either edge has
/// an edge past it even while its centre is inside. The rectangle tested is the hitbox around an enemy, and an enemy can
/// lie above or left of the room's origin (a fly that has climbed through the ceiling, a Bull braking past Samus), with a
/// hitbox reaching past the edge from inside it besides. The short-way comparison reads an edge the same whether it is
/// computed below zero or counted back from the far end of the coordinates, so the edges need no wrap, and it meets the
/// other side as <see cref="EnemyCollision.RadiusBoxesOverlap"/> meets the same targets by their radii.
/// </remarks>
/// <param name="Left">The target's left edge, which may lie left of the room.</param>
/// <param name="Top">The target's top edge, which may lie above the room.</param>
/// <param name="Right">The target's right edge.</param>
/// <param name="Bottom">The target's bottom edge.</param>
/// <param name="Contact">The contact the test selects (#2560): the shot reaction with strict edges, or the touch reaction with inclusive ones.</param>
internal readonly record struct EnemyHitboxProbe(int Left, int Top, int Right, int Bottom, HitboxContactKind Contact)
{
    /// <summary>
    /// The edges of a target at <paramref name="position"/> reaching <paramref name="radius"/> each way, for the
    /// <paramref name="contactKind"/> the test selects.
    /// </summary>
    internal static EnemyHitboxProbe Around(Position16<RoomPixels> position, Extent16<RoomPixels> radius, HitboxContactKind contactKind) =>
        new(
            Left: position.X - radius.X,
            Top: position.Y - radius.Y,
            Right: position.X + radius.X,
            Bottom: position.Y + radius.Y,
            Contact: contactKind);

    /// <summary>
    /// The shared frame-hitbox rectangle test. Shots and touch meet the rectangle's edges differently in the game:
    /// strictly and inclusively. The rectangle's edges, which the species compute from an enemy's place and its hitbox,
    /// and the target's are compared the short way across the room's coordinates, for the targets and enemies the probe's
    /// remarks name.
    /// </summary>
    internal bool Overlaps(int left, int top, int right, int bottom) =>
        Contact == HitboxContactKind.Shot
            ? !DistanceAcrossRoomEdge.LiesBefore(Right, left) &&
              DistanceAcrossRoomEdge.LiesBefore(Left, right) &&
              !DistanceAcrossRoomEdge.LiesBefore(Bottom, top) &&
              DistanceAcrossRoomEdge.LiesBefore(Top, bottom)
            : DistanceAcrossRoomEdge.LiesBefore(left, Right) &&
              !DistanceAcrossRoomEdge.LiesBefore(right, Left) &&
              DistanceAcrossRoomEdge.LiesBefore(top, Bottom) &&
              !DistanceAcrossRoomEdge.LiesBefore(bottom, Top);
}
