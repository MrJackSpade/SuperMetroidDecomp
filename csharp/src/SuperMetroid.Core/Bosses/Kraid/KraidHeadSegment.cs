namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's four head animations, in the order the game lays them out. The order matters: a head animation's first
/// mouth reads look back into the animation authored before it (see <see cref="KraidHeadAnimations.ReadsAt"/>).
/// </summary>
internal enum KraidHeadSegment
{
    /// <summary>The attack roar: the mouth opens through three stages, roars, holds wide open, then closes.</summary>
    Roar,
    /// <summary>The slower roar authored after the attack roar, opening for longer.</summary>
    DyingRoar,
    /// <summary>The eye glow after a body hit: the mouth opens one stage and closes.</summary>
    EyeGlow,
    /// <summary>The death cry: the mouth opens through every stage and stays open.</summary>
    Death,
}
