namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter's acid-spit programs (#641), one per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="KiHunterAcidSpitAnimations"/>.
/// </summary>
internal enum KiHunterAcidSpitSegment
{
    /// <summary>The left animation.</summary>
    Left,
    /// <summary>The hit floor animation.</summary>
    HitFloor,
    /// <summary>The right animation.</summary>
    Right,
}
