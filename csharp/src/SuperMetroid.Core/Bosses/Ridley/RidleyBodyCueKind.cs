namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// What a cue of Ridley's body animations does: roar, face forward, aim or spawn a fireball, start his lift-off, or give
/// Ceres Ridley the chance to cut his fireballing short.
/// </summary>
internal enum RidleyBodyCueKind
{
    /// <summary>
    /// Ceres Ridley may cut his fireballing short (#2175): when Samus has under 30 energy his owner shortens the hover to
    /// eight updates and continues in the fireballing's end for the way he faces.
    /// </summary>
    MayCutFireballingShort,
    /// <summary>Raises the roaring flag and queues the roar.</summary>
    Roar,
    /// <summary>Clears the roaring flag.</summary>
    ClearRoaringFlag,
    /// <summary>Faces forward while turning.</summary>
    FaceForward,
    /// <summary>Aims the next fireball at Samus.</summary>
    AimFireball,
    /// <summary>Spawns a fireball with its afterburn.</summary>
    SpawnFireballWithAfterburn,
    /// <summary>Spawns a fireball without an afterburn.</summary>
    SpawnFireballWithoutAfterburn,
    /// <summary>Starts Ceres Ridley's lift-off.</summary>
    CeresStartLiftoff,
    /// <summary>Starts Lower Norfair Ridley's lift-off into the fight.</summary>
    StartLiftoff,
}
