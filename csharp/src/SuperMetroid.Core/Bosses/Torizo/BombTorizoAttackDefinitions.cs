namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Fixed physical placement definitions for Bomb Torizo's low-health explosions, each a body-relative room-pixel offset.
/// Each swipe cue carries its own hand placement.
/// </summary>
internal static class BombTorizoAttackDefinitions
{
    /// <summary>
    /// The gut's and then the face's explosion placements, facing right then left. Gut placements are
    /// 12 pixels horizontally and 8 up; face placements are 16 horizontally and 20 up.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] ExplosionPlacements =
    [
        new(12, -8), new(-12, -8),
        new(16, -20), new(-16, -20),
    ];
}
