namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The downward shot gate's moving and closed programs (#641), one per bank-$86 list a spawn, branch or family routine enters, in authored order.
/// Generated from the verified native lists; see <see cref="DownwardGateProjectileAnimations"/>.
/// </summary>
internal enum DownwardGateProjectileProgram
{
    /// <summary><c>InstList_EnemyProjectile_ShotGate_SpawnedDownwards</c> at $86:E53C.</summary>
    Moving,
    /// <summary>The loop at $86:E54A.</summary>
    MovingStep2,
    /// <summary>The loop at $86:E550.</summary>
    MovingStep3,
    /// <summary>The loop at $86:E556.</summary>
    MovingStep4,
    /// <summary>The loop at $86:E55C.</summary>
    MovingStep5,
    /// <summary><c>InstList_EnemyProjectile_ShotGate_InitialClosedDownwards</c> at $86:E55E.</summary>
    Closed,
    /// <summary>The loop at $86:E568.</summary>
    ClosedStep2,
    /// <summary>The loop at $86:E572.</summary>
    ClosedStep3,
    /// <summary>The loop at $86:E578.</summary>
    ClosedStep4,
    /// <summary>The loop at $86:E57E.</summary>
    ClosedStep5,
    /// <summary>The loop at $86:E584.</summary>
    ClosedStep6,
}
