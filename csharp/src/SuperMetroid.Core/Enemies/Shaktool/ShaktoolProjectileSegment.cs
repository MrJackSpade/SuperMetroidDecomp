namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Shaktool's attack-circle animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="ShaktoolProjectileAnimations"/>.
/// </summary>
internal enum ShaktoolProjectileSegment
{
    /// <summary>The front animation.</summary>
    Front,
    /// <summary>The loop.</summary>
    FrontLoop,
    /// <summary>The middle animation.</summary>
    Middle,
    /// <summary>The loop.</summary>
    MiddleLoop,
    /// <summary>The back animation.</summary>
    Back,
    /// <summary>The loop.</summary>
    BackLoop,
}
