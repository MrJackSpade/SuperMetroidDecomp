namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// The Kago bugs' landing, falling, jumping and shot animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="KagoBugProjectileAnimations"/>.
/// </summary>
internal enum KagoBugProjectileSegment
{
    /// <summary>The still pose a new bug holds while it waits to make its first jump.</summary>
    Waiting,
    /// <summary>The landed animation.</summary>
    Landed,
    /// <summary>The falling animation.</summary>
    Falling,
    /// <summary>The jump start animation.</summary>
    JumpStart,
    /// <summary>The shot animation: the bug bursts, drops its pickup and is deleted.</summary>
    Shot,
}
