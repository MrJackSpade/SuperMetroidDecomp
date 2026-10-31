namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a cue of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle programs does: one per bank-$86 command its lists use.</summary>
internal enum MotherBrainEffectCueKind
{
    /// <summary>Stops the projectile's movement.</summary>
    ClearFrameBehavior,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Lowers the released drool twelve pixels.</summary>
    DropDrool,
    /// <summary>Marks Mother Brain's small purple breath inactive.</summary>
    EndSmallPurpleBreath,
    /// <summary>Draws the projectile with palette zero.</summary>
    UsePaletteZero,
}
