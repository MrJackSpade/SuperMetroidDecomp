namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Bank-$AA pre-instruction pointers used by the two retail Chozo-statue variants.
/// The numeric values are the actual cartridge addresses stored in <c>ai_preinstr</c>.
/// </summary>
public enum ChozoStatuePreInstruction : ushort
{
    /// <summary>$AA:E7A6, the initializer's one-byte RTL.</summary>
    Idle = 0xe7a6,

    /// <summary>$AA:807B, the bank-common RTL installed by instruction $8074.</summary>
    Cleared = 0x807b,

    /// <summary>$AA:E445, waits for Lower Norfair's hand-trigger PLM.</summary>
    WaitForLowerNorfairHandTrigger = 0xe445,

    /// <summary>$AA:E7AE, waits for Phantoon's boss bit and the Wrecked Ship hand trigger.</summary>
    WaitForWreckedShipHandTrigger = 0xe7ae,

    /// <summary>$AA:E7DA, the no-op installed while the Wrecked Ship statue walks.</summary>
    Walking = 0xe7da,
}
