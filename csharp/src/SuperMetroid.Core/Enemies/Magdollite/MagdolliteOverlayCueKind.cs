namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>What a signal cue of the Magdollite overlay's throw does: throw lava, finish the throw, or start the attack cooldown (#2554).</summary>
internal enum MagdolliteOverlayCueKind
{
    /// <summary>Finishes the hand's throw (#2439): its next update starts the arm's descent.</summary>
    FinishThrow,
    /// <summary>Throws a lava projectile.</summary>
    SpawnLava,
    /// <summary>Starts the attack cooldown (<see cref="MagdolliteMotionDefinitions.AttackCooldown"/>).</summary>
    StartCooldown,
}
