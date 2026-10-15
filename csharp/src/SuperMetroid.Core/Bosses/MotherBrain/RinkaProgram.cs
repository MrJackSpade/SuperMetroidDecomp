namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The ordinary and Mother Brain Rinkas' hide, fire and spin animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="RinkaAnimations"/>.
/// </summary>
internal enum RinkaProgram
{
    /// <summary>The native list at $A2:B9E0.</summary>
    Ordinary,
    /// <summary>The loop at $A2:B9E8.</summary>
    OrdinaryLoop,
    /// <summary>The native list at $A2:BA0C.</summary>
    Special,
    /// <summary>The loop at $A2:BA14.</summary>
    SpecialLoop,
}
