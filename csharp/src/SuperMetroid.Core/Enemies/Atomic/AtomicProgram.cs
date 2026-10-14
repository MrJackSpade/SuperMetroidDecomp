namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's four spinning animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="AtomicAnimations"/>.
/// </summary>
internal enum AtomicProgram
{
    /// <summary>The native list at $A8:E310.</summary>
    UpRight,
    /// <summary>The native list at $A8:E32C.</summary>
    UpLeft,
    /// <summary>The native list at $A8:E348.</summary>
    DownLeft,
    /// <summary>The native list at $A8:E364.</summary>
    DownRight,
}
