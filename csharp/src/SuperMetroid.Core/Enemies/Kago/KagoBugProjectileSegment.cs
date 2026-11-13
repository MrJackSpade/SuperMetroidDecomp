namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// The Kago bugs' landing, falling, jumping and shot programs (#641), one per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="KagoBugProjectileAnimations"/>.
/// </summary>
internal enum KagoBugProjectileSegment
{
    /// <summary>The shared delete animation.</summary>
    SharedDelete,
    /// <summary>The landed animation.</summary>
    Landed,
    /// <summary>The falling animation.</summary>
    Falling,
    /// <summary>The jump start animation.</summary>
    JumpStart,
    /// <summary>The jump loop animation.</summary>
    JumpLoop,
    /// <summary>The shot animation.</summary>
    Shot,
}
