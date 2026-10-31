namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Speed parameters Draygon passes to the enemy-projectile spawn,
/// which stores them in the projectile's first initialization parameter before the initializer.
/// </summary>
public static class DraygonProjectileSpeeds
{
    /// <summary><c>LDA #regional</c>: NTSC wall-turret shot speed.</summary>
    public const int WallTurret = 3;

    /// <summary>
    /// Two, passed by the goop spawn. Despite the disassembly's note that the
    /// goop instructions do not set parameter zero, the spawn routine stores this A value.
    /// </summary>
    public const int Goop = 2;
}
