namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// What a signal cue of the Magdollite's animations does: place the head and body for the throw, throw lava, finish the
/// part's submerge, emerge or throw, or start the attack cooldown.
/// </summary>
internal enum MagdolliteCueKind
{
    /// <summary>Finishes the head's emergence (#2439): its next update idles it.</summary>
    FinishEmerge,
    /// <summary>Finishes the head's submerge (#2439): its next update releases its arm.</summary>
    FinishSubmerge,
    /// <summary>Finishes the hand's throw (#2439): its next update starts the arm's descent.</summary>
    FinishThrow,
    /// <summary>Returns the head and body to the origin height.</summary>
    RestoreOrigins,
    /// <summary>Puts the head and body at their sunk depth (<see cref="MagdolliteMotionDefinitions.SunkDepth"/>) and shows the body and overlay.</summary>
    SinkAndShowParts,
    /// <summary>Throws a lava projectile.</summary>
    SpawnLava,
    /// <summary>Starts the attack cooldown (<see cref="MagdolliteMotionDefinitions.AttackCooldown"/>).</summary>
    StartCooldown,
    /// <summary>Puts the head and body at their surfaced depth (<see cref="MagdolliteMotionDefinitions.SurfacedDepth"/>) and hides the body and overlay.</summary>
    SurfaceAndHideParts,
}
