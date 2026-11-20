namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's goop and wall-turret projectile animations.</summary>
public enum DraygonProjectileSegment
{
    /// <summary>Sticks the goop to Samus, then runs on into the goop loop.</summary>
    GoopTouch,
    /// <summary>A goop blob wobbling in flight, looping.</summary>
    Goop,
    /// <summary>A goop blob bursting, a drop, then deletion.</summary>
    GoopShot,
    /// <summary>The wall turret shot's bloom and its firing, which runs on into its flight.</summary>
    WallTurretBloom,
    /// <summary>The fired wall turret shot's flight frames, looping (#2221).</summary>
    WallTurretFlight,
}
