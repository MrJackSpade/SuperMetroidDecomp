namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Fixed physical placement definitions for Bomb Torizo's body projectiles: hand-relative
/// swipe placements and body-relative low-health explosion placements, each a room-pixel offset.
/// </summary>
internal static class BombTorizoAttackDefinitions
{
    /// <summary>
    /// The eleven paired X/Y placements at <c>$86:A738-$86:A763</c>, for a left-facing Torizo; a swipe cue selects
    /// one by its index.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] SwipePlacements =
    [
        new(-30, -52),
        new(-40, -28),
        new(-47, -11),
        new(-31, 9),
        new(-21, 21),
        new(-1, 20),
        new(-28, -52),
        new(-43, -27),
        new(-48, -10),
        new(-31, 9),
        new(-21, 20),
    ];

    /// <summary>
    /// $86:A859/$A865: the gut's and then the face's explosion placements, facing right then left. Gut placements are
    /// 12 pixels horizontally and 8 up; face placements are 16 horizontally and 20 up.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] ExplosionPlacements =
    [
        new(12, -8), new(-12, -8),
        new(16, -20), new(-16, -20),
    ];
}
