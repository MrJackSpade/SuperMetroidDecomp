
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The shared Pirate/Mother Brain laser and Ninja Pirate claw lists' entries; <see cref="SpacePirateProjectileAnimation"/>
/// plays them typed.
/// </summary>
internal abstract class SpacePirateProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Left_0</c> at $86:9F41.</summary>
    internal const ushort LaserLeft = 0x9f41;

    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Left_1</c> at $86:9F71.</summary>
    internal const ushort LaserLeftLoop = 0x9f71;

    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Right_0</c> at $86:9F7D.</summary>
    internal const ushort LaserRight = 0x9f7d;

    /// <summary><c>InstList_EnemyProjectile_Pirate_MotherBrain_Laser_Right_1</c> at $86:9FAD.</summary>
    internal const ushort LaserRightLoop = 0x9fad;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Left_0</c> at $86:9FB9.</summary>
    internal const ushort ClawLeft = 0x9fb9;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Left_1</c> at $86:9FBD.</summary>
    internal const ushort ClawLeftLoop = 0x9fbd;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Right_0</c> at $86:9FE1.</summary>
    internal const ushort ClawRight = 0x9fe1;

    /// <summary><c>InstList_EnemyProjectile_PirateClaw_Right_1</c> at $86:9FE5.</summary>
    internal const ushort ClawRightLoop = 0x9fe5;

}
