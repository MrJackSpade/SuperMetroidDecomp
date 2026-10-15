namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>
/// The falling spark's programs (#641), one per bank-$86 list a spawn, branch or family routine enters, in authored order.
/// Generated from the verified native lists; see <see cref="FallingSparkAnimations"/>.
/// </summary>
internal enum FallingSparkProgram
{
    /// <summary><c>InstList_EnemyProjectile_FallingSpark_Falling</c> at $86:F353.</summary>
    Falling,
    /// <summary><c>InstList_EnemyProjectile_FallingSpark_HitFloor</c> at $86:F363.</summary>
    HitFloor,
}
