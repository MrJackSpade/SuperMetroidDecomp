namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>What a cue of Ridley's body animations does: one per kind of step its lists use.</summary>
internal enum RidleyBodyCueKind
{
    /// <summary>Raises the roaring flag and queues the roar.</summary>
    Roar,
    /// <summary>Clears the roaring flag.</summary>
    ClearRoaringFlag,
    /// <summary>Faces forward while turning.</summary>
    FaceForward,
    /// <summary>Faces left and mirrors the tail.</summary>
    FlipLeft,
    /// <summary>Faces right and mirrors the tail.</summary>
    FlipRight,
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
