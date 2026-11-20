namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// The Kago bugs' landing, falling, jumping and shot animations (#641).
/// Their steps are <see cref="KagoBugProjectileAnimations"/>.
/// </summary>
internal enum KagoBugProjectileSegment
{
    /// <summary>The still pose a new bug holds while it waits to make its first jump.</summary>
    Waiting,
    /// <summary>A bug crouching as it lands, then standing idle, repeating.</summary>
    Landed,
    /// <summary>A bug standing, held while it falls.</summary>
    Falling,
    /// <summary>A bug crouching, standing and then leaping, held while it jumps.</summary>
    JumpStart,
    /// <summary>The shot animation: the bug bursts, drops its pickup and is deleted.</summary>
    Shot,
}
