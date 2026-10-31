namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Speed parameters bank $A5 passes in A to <c>SpawnEnemyProjectileY_ParameterA_XGraphics</c>,
/// which stores A into <c>EnemyProjectile_InitParam0</c> ($86:802B) before the initializer.
/// </summary>
public static class DraygonProjectileSpeeds
{
    /// <summary><c>LDA #regional($0003, $0004)</c> at $A5:87D4: NTSC wall-turret shot speed.</summary>
    public const int WallTurret = 3;

    /// <summary>
    /// <c>LDA #$0002</c> at $A5:9FA4 and $A5:9FD6. Despite the disassembly's note that the
    /// goop instructions do not set parameter zero, the spawn routine stores this A value.
    /// </summary>
    public const int Goop = 2;
}
