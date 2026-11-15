namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Fixed physical placement definitions for Bomb Torizo's low-health explosions, each a body-relative room-pixel offset
/// for a right-facing Torizo; a left-facing Torizo places them mirrored (#2029). Each swipe cue carries its own hand
/// placement.
/// </summary>
internal static class BombTorizoAttackDefinitions
{
    /// <summary>The gut's explosions: twelve pixels ahead and eight up.</summary>
    internal static readonly Offset16<RoomPixels> GutExplosion = new(12, -8);

    /// <summary>The face's explosions: sixteen pixels ahead and twenty up.</summary>
    internal static readonly Offset16<RoomPixels> FaceExplosion = new(16, -20);
}
