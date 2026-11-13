namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The ordinary and Mother Brain Rinkas' hide, fire and spin animations as typed programs (#641), one per list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified lists; see <see cref="RinkaAnimations"/>.
/// </summary>
internal enum RinkaSegment
{
    /// <summary>Where the animation starts.</summary>
    Ordinary,
    /// <summary>The animation's repeating part.</summary>
    OrdinaryLoop,
    /// <summary>Where the animation starts.</summary>
    Special,
    /// <summary>The animation's repeating part.</summary>
    SpecialLoop,
}
