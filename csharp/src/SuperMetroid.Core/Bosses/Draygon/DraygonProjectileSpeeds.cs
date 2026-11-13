namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Speed parameters Draygon passes to the enemy-projectile spawn,
/// which stores them in the projectile's first initialization parameter before the initializer.
/// </summary>
public static class DraygonProjectileSpeeds
{
    /// <summary>The NTSC wall-turret shot speed.</summary>
    public const int WallTurret = 3;

    /// <summary>
    /// Two, passed by the goop spawn. The goop's own animation never sets its speed;
    /// the spawn stores this value.
    /// </summary>
    public const int Goop = 2;
}
