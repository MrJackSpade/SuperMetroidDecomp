namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's goop and wall-turret projectile animations.</summary>
public enum DraygonProjectileSegment
{
    /// <summary>Sticks the goop to Samus, then runs on into the goop loop.</summary>
    GoopTouch,
    /// <summary>Six ten-update frames, looping.</summary>
    Goop,
    /// <summary>Two eight-update frames, a drop, then deletion.</summary>
    GoopShot,
    /// <summary>The sixteen-frame bloom, then the shot is fired.</summary>
    WallTurretBloom,
    /// <summary>Three eight-update frames, looping.</summary>
    WallTurretFlight,
}
