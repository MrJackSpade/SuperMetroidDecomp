namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The routines the Dead Torizo corpse's main AI dispatches.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
internal enum DeadTorizoFunction : ushort
{
    /// <summary>$A9:D3AD holds the intact corpse before its contact or shot trigger.</summary>
    Wait = 1,

    /// <summary>$A9:D3C8 counts the contact delay before entering the rotting phase.</summary>
    PreRot,

    /// <summary>$A9:D3E6 advances row rot, sand-line copies, and VRAM transfer phases.</summary>
    Rotting,

    /// <summary>$A9:D3C7 leaves the finished corpse slot inert.</summary>
    NoOperation,
}
