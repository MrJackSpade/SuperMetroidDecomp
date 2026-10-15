namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The bank-$84 pre-instructions a coloured cap installs (#1296); each member's value is its routine.</summary>
internal enum ColoredDoorPreInstruction : ushort
{
    /// <summary>No pre-instruction.</summary>
    None = 0,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfHitWithPowerBomb</c> (yellow).</summary>
    WakeOnPowerBomb = 0xbd26,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfShotWithAMissile</c> (red; a Super Missile seeds the counter).</summary>
    WakeOnMissile = 0xbd50,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstIfShotWithASuperMissile</c> (green).</summary>
    WakeOnSuperMissile = 0xbd88,
}
