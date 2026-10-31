namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The pre-instructions a coloured cap installs (#1296). The values are arbitrary (#1414).</summary>
internal enum ColoredDoorFrameBehavior
{
    /// <summary>No pre-instruction.</summary>
    None = 1,
    /// <summary><c>FrameBehavior_PLM_GotoLinkInstructionIfHitWithPowerBomb</c> (yellow).</summary>
    WakeOnPowerBomb = 2,
    /// <summary><c>FrameBehavior_PLM_GotoLinkInstructionIfShotWithAMissile</c> (red; a Super Missile seeds the counter).</summary>
    WakeOnMissile = 3,
    /// <summary><c>FrameBehavior_PLM_GotoLinkInstIfShotWithASuperMissile</c> (green).</summary>
    WakeOnSuperMissile = 4,
}
