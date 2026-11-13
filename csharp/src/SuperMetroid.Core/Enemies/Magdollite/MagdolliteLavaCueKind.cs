namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>What a cue of the Magdollite's lava animations does: delete the projectile or request the shot lava's drop.</summary>
internal enum MagdolliteLavaCueKind
{
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Requests the shot lava's drop.</summary>
    SpawnDrops,
}
