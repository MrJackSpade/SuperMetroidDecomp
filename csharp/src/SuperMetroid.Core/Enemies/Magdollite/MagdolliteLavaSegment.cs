namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite's thrown-lava animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="MagdolliteLavaAnimations"/>.
/// </summary>
internal enum MagdolliteLavaSegment
{
    /// <summary>A gob of lava thrown left, held.</summary>
    Left,
    /// <summary>A gob of lava thrown right, held.</summary>
    Right,
    /// <summary>The shot animation: the lava spawns its drops and is deleted.</summary>
    Shot,
}
