namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The generic enemy-death explosions, the drops they become and the respawn tail (#641), one per bank-$86 list a spawn, branch or family routine enters, in authored order.
/// Generated from the verified native lists; see <see cref="EnemyDeathProjectileAnimations"/>.
/// </summary>
internal enum EnemyDeathProjectileProgram
{
    /// <summary><c>InstList_EnemyProjectile_Pickup_HandleRespawningEnemy</c> at $86:ECA3.</summary>
    RespawnTail,
    /// <summary><c>InstList_EnemyProjectile_EnemyDeathExplosion_BigExplosion_0</c> at $86:ECAB.</summary>
    BigExplosion,
    /// <summary>The loop at $86:ECAF.</summary>
    BigExplosionLoop,
    /// <summary><c>InstList_EnemyProj_EnemyDeathExplosion_MiniKraidExplosion_0</c> at $86:ECC5.</summary>
    MiniKraidExplosion,
    /// <summary>The loop at $86:ECC9.</summary>
    MiniKraidExplosionLoop,
    /// <summary><c>InstList_EnemyProjectile_EnemyDeathExplosion_NormalExplosion</c> at $86:ED4B.</summary>
    NormalExplosion,
    /// <summary><c>InstList_EnemyProjectile_EnemyDeathExplosion_SmallExplosion</c> at $86:ED69.</summary>
    SmallExplosion,
    /// <summary><c>InstList_EnemyProjectile_Pickup_SmallEnergy</c> at $86:ED8D.</summary>
    SmallEnergy,
    /// <summary><c>InstList_EnemyProjectile_Pickup_BigEnergy</c> at $86:EDA3.</summary>
    BigEnergy,
    /// <summary><c>InstList_EnemyProjectile_Pickup_Missiles</c> at $86:EDB9.</summary>
    Missiles,
    /// <summary><c>InstList_EnemyProjectile_Pickup_SuperMissiles</c> at $86:EDDD.</summary>
    SuperMissiles,
    /// <summary><c>InstList_EnemyProjectile_Pickup_PowerBombs</c> at $86:EDEB.</summary>
    PowerBombs,
    /// <summary><c>InstList_EnemyProj_EnemyDeathExplosion_KilledBySamusContact</c> at $86:EDFF.</summary>
    KilledBySamusContact,
}
