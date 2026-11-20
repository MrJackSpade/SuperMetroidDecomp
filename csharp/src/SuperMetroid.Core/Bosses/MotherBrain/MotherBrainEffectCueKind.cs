namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a cue of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle animations does: stop or delete the projectile, or release or lower the drool.</summary>
internal enum MotherBrainEffectCueKind
{
    /// <summary>Stops the projectile's movement.</summary>
    StopMoving,
    /// <summary>Releases the hanging drool: it falls from now on.</summary>
    ReleaseDrool,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Lowers the released drool twelve pixels.</summary>
    DropDrool,
}
