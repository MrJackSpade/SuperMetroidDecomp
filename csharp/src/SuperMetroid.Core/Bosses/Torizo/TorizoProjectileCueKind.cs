namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue every Torizo projectile's behaviour runs alike does: stop it, delete it, raise a dust cloud, or change whether it blocks or damages and where it draws (#2554).</summary>
internal enum TorizoProjectileCueKind
{
    /// <summary>Stops the projectile's movement.</summary>
    StopMoving,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Raises a landing dust cloud by <see cref="TorizoProjectilePlacementDefinitions.DustRise"/>.</summary>
    RaiseDust,
    /// <summary>Stops blocking Samus's projectiles.</summary>
    StopBlockingShots,
    /// <summary>Stops damaging Samus.</summary>
    StopHarmingSamus,
    /// <summary>Moves the projectile in front of Samus.</summary>
    DrawInFront,
    /// <summary>Moves the projectile behind Samus.</summary>
    DrawBehindSamus,
    /// <summary>Stops the projectile damaging Samus and moves it in front of her.</summary>
    StopHarmingSamusAndDrawInFront,
    /// <summary>Lets the projectile survive touching Samus and moves it in front of her.</summary>
    PersistOnContactAndDrawInFront,
    /// <summary>Lets the projectile damage Samus.</summary>
    TurnHarmful,
    /// <summary>Lets the projectile damage Samus and stops it blocking her projectiles.</summary>
    TurnHarmfulAndStopBlockingShots,
    /// <summary>Makes the projectile block Samus's projectiles.</summary>
    BlockShots,
}
