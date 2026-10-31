namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's goop and wall-turret projectile programs.</summary>
public enum DraygonProjectileProgram
{
    /// <summary><c>InstList_EnemyProjectile_DraygonGoop_Touch</c>: sticks the goop to Samus, then runs on into the goop loop.</summary>
    GoopTouch,
    /// <summary><c>InstList_EnemyProjectile_DraygonGoop</c>: six ten-update frames, looping.</summary>
    Goop,
    /// <summary><c>InstList_EnemyProjectile_DraygonGoop_Shot</c>: two eight-update frames, a drop, then deletion.</summary>
    GoopShot,
    /// <summary><c>InstList_EnemyProjectile_DraygonsWallTurretProjectile_0</c>: the sixteen-frame bloom, then the shot is fired.</summary>
    WallTurretBloom,
    /// <summary><c>InstList_EnemyProjectile_DraygonsWallTurretProjectile_1</c>: three eight-update frames, looping.</summary>
    WallTurretFlight,
}
