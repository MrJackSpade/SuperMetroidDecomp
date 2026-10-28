namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Fixed physical placement definitions for Bomb Torizo's body projectiles: hand-relative
/// swipe placements and body-relative low-health explosion placements, each a room-pixel offset.
/// </summary>
internal static class BombTorizoAttackDefinitions
{
    /// <summary>
    /// The eleven paired X/Y placements at <c>$86:A738-$86:A763</c>. Native swipe
    /// parameters are even byte offsets <c>$00..$14</c>.
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
    /// $86:A859/$A865: gut and face explosion offsets, each with an unused centered row followed by right/left facing
    /// rows. Gut offsets are 12 pixels horizontally and 8 up; face offsets are 16 horizontally and 20 up.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] ExplosionRows =
    [
        new(0, -8), new(12, -8), new(-12, -8),
        new(0, -20), new(16, -20), new(-16, -20),
    ];
}
