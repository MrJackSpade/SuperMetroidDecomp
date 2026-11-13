namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed part animations (#641), in authored order.</summary>
internal enum PhantoonPartSegment
{
    /// <summary>The body with no hitbox.</summary>
    InvulnerableBody,
    /// <summary>The body with its full hitbox.</summary>
    FullHitboxBody,
    /// <summary>The body with only its eye hitbox.</summary>
    EyeHitboxBody,
    /// <summary>The eye opening, then the materialization sound and eye tracking.</summary>
    EyeOpen,
    /// <summary>The closed eye.</summary>
    EyeClosed,
    /// <summary>The eye closing, then picking the next pattern.</summary>
    EyeCloseAndPickNewPattern,
    /// <summary>The eye closing.</summary>
    EyeClose,
    /// <summary>The centered eyeball, then the materialization sound.</summary>
    EyeballCentered,
    /// <summary>The eyeball looking up.</summary>
    EyeLookingUp,
    /// <summary>The eyeball looking up and right.</summary>
    EyeLookingUpRight,
    /// <summary>The eyeball looking right.</summary>
    EyeLookingRight,
    /// <summary>The eyeball looking down and right.</summary>
    EyeLookingDownRight,
    /// <summary>The eyeball looking down.</summary>
    EyeLookingDown,
    /// <summary>The eyeball looking down and left.</summary>
    EyeLookingDownLeft,
    /// <summary>The eyeball looking left.</summary>
    EyeLookingLeft,
    /// <summary>The eyeball looking up and left.</summary>
    EyeLookingUpLeft,
    /// <summary>The tentacles' looping sway.</summary>
    Tentacles,
    /// <summary>The mouth opening to spawn a casual flame, running on into the initial mouth.</summary>
    MouthSpawnFlame,
    /// <summary>The closed mouth.</summary>
    MouthInitial,
}