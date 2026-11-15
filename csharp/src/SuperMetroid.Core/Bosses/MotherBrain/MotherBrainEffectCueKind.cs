namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a cue of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle animations does: stop or delete the projectile, release or lower the drool, end the purple breath, or draw from the room's shared sheet.</summary>
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
    /// <summary>Marks Mother Brain's small purple breath inactive.</summary>
    EndSmallPurpleBreath,
    /// <summary>Draws the projectile from the room's shared sheet in its standard colours.</summary>
    DrawFromSharedSheet,
}
