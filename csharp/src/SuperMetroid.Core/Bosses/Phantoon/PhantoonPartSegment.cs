namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed part animations (#641), in authored order.</summary>
internal enum PhantoonPartSegment
{
    /// <summary>His body's still picture; which of its parts are hittable is his fight's <see cref="PhantoonBodyHitArea"/>.</summary>
    Body,
    /// <summary>The eye opening, then the materialization sound and eye tracking.</summary>
    EyeOpen,
    /// <summary>His eye shut, held.</summary>
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