using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The downward shot gate's moving and closed programs (#641), one per list a spawn, branch or family step enters, in authored order.
/// Generated from the verified lists; see <see cref="DownwardGateProjectileAnimations"/>.
/// </summary>
internal enum DownwardGateProjectileProgram
{
    /// <summary>Enemy projectile animation script: shot gate, spawned downwards.</summary>
    Moving,
    /// <summary>The animation's repeating part.</summary>
    MovingStep2,
    /// <summary>The animation's repeating part.</summary>
    MovingStep3,
    /// <summary>The animation's repeating part.</summary>
    MovingStep4,
    /// <summary>The animation's repeating part.</summary>
    MovingStep5,
    /// <summary>Enemy projectile animation script: shot gate, initial closed downwards.</summary>
    Closed,
    /// <summary>The animation's repeating part.</summary>
    ClosedStep2,
    /// <summary>The animation's repeating part.</summary>
    ClosedStep3,
    /// <summary>The animation's repeating part.</summary>
    ClosedStep4,
    /// <summary>The animation's repeating part.</summary>
    ClosedStep5,
    /// <summary>The animation's repeating part.</summary>
    ClosedStep6,
}
