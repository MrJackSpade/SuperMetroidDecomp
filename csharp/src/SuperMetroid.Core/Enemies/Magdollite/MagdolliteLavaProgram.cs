namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite's thrown-lava programs (#641), one per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="MagdolliteLavaAnimations"/>.
/// </summary>
internal enum MagdolliteLavaProgram
{
    /// <summary>The shared delete animation.</summary>
    SharedDelete,
    /// <summary>The left animation.</summary>
    Left,
    /// <summary>The right animation.</summary>
    Right,
    /// <summary>The shot animation.</summary>
    Shot,
}
