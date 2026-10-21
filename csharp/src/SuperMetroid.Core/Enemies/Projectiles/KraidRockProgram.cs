namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Kraid's rock projectile programs, which the Kago bug's initial pose shares.</summary>
public enum KraidRockProgram
{
    /// <summary><c>InstList_EnemyProjectile_KraidRocks_KagoBug</c> ($86:9C7D): one still pose, shared by the spat, ceiling and left rising rocks and the Kago bug.</summary>
    SharedPose,
    /// <summary><c>InstList_EnemyProjectile_KraidFloorRocks_Right</c> ($86:9C83): the right rising rock's still pose.</summary>
    RisingRightPose,
    /// <summary><c>InstList_EnemyProjectile_Shot_KraidRockSpit</c> ($86:9C89): the spat rock's five-frame burst after a Samus shot.</summary>
    SpitRockShot,
}
