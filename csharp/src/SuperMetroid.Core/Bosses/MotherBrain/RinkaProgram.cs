namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The ordinary and Mother Brain Rinkas' hide, fire and spin animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="RinkaAnimations"/>.
/// </summary>
internal enum RinkaProgram
{
    /// <summary>The native list.</summary>
    Ordinary,
    /// <summary>The loop.</summary>
    OrdinaryLoop,
    /// <summary>The native list.</summary>
    Special,
    /// <summary>The loop.</summary>
    SpecialLoop,
}
