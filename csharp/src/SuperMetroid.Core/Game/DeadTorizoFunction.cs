namespace SuperMetroid.Core.Game;

/// <summary>Dead Torizo corpse functions stored in the native VariableA slot.</summary>
internal enum DeadTorizoFunction : ushort
{
    /// <summary>$A9:D3AD holds the intact corpse before its contact or shot trigger.</summary>
    Wait = 0xd3ad,

    /// <summary>$A9:D3C8 counts the contact delay before entering the rotting phase.</summary>
    PreRot = 0xd3c8,

    /// <summary>$A9:D3E6 advances row rot, sand-line copies, and VRAM transfer phases.</summary>
    Rotting = 0xd3e6,

    /// <summary>$A9:D3C7 leaves the finished corpse slot inert.</summary>
    NoOperation = 0xd3c7,
}
