namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// What a cue of Ridley's body animations does: roar, face forward, aim or spawn a fireball, lift him off, or give the
/// encounter that owns his body the chance to cut his fireballing short.
/// </summary>
internal enum RidleyBodyCueKind
{
    /// <summary>
    /// The encounter that owns the body may cut his fireballing short (#2175, #2230): Ceres Ridley does when Samus has
    /// under 30 energy, shortening his hover to eight updates and continuing in the fireballing's end for the way he
    /// faces; Lower Norfair Ridley plays on.
    /// </summary>
    MayCutFireballingShort,
    /// <summary>Starts his roar and queues its sound; while he roars he sweeps the wall with no fireballs.</summary>
    Roar,
    /// <summary>Ends his roar.</summary>
    StopRoaring,
    /// <summary>Faces forward while turning.</summary>
    FaceForward,
    /// <summary>Aims the next fireball at Samus.</summary>
    AimFireball,
    /// <summary>Spawns a fireball with its afterburn.</summary>
    SpawnFireballWithAfterburn,
    /// <summary>Spawns a fireball without an afterburn.</summary>
    SpawnFireballWithoutAfterburn,
    /// <summary>
    /// Lifts Ridley off at the end of his take-off (#2230): he starts rising at a pixel and three eighths an update, and the
    /// encounter that owns his body starts its flight.
    /// </summary>
    LiftOff,
}
