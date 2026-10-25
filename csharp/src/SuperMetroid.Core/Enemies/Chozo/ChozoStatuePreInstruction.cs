namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The pre-instruction routines the two retail Chozo-statue variants run.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum ChozoStatuePreInstruction : ushort
{
    /// <summary>$AA:E7A6, the initializer's one-byte RTL.</summary>
    Idle = 1,

    /// <summary>$AA:807B, the bank-common RTL installed by instruction $8074.</summary>
    Cleared,

    /// <summary>$AA:E445, waits for Lower Norfair's hand-trigger PLM.</summary>
    WaitForLowerNorfairHandTrigger,

    /// <summary>$AA:E7AE, waits for Phantoon's boss bit and the Wrecked Ship hand trigger.</summary>
    WaitForWreckedShipHandTrigger,

    /// <summary>$AA:E7DA, the no-op installed while the Wrecked Ship statue walks.</summary>
    Walking,
}
