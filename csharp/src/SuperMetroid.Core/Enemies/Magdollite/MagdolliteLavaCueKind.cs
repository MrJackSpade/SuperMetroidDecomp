namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>What a cue of the Magdollite's lava animations does: one per command its lists use.</summary>
internal enum MagdolliteLavaCueKind
{
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Requests the shot lava's drop.</summary>
    SpawnDrops,
}
