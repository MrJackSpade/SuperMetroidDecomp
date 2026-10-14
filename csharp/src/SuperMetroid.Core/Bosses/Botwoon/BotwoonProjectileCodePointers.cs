using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Native bank-$86 shared-program pointers used by Botwoon's projectiles.</summary>
internal static class BotwoonProjectileCodePointers
{
    /// <summary><c>InstList_EnemyProj_MiscDust_1D_BigExplosion</c> at $86:E208.</summary>
    internal const ushort BodyLandedInstruction =
        EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBigDeathExplosionInitial;
}
