namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The bank-$84 pre-instructions a grey cap installs (#1296); each member's value is its routine. The unlock checks are
/// the table at $84:BE4B, selected by the door's condition.
/// </summary>
internal enum GreyDoorPreInstruction : ushort
{
    /// <summary>No pre-instruction.</summary>
    None = 0,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfShot</c>.</summary>
    WakeWhenShot = 0xbd0f,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfAreaBossDead</c>.</summary>
    WakeIfAreaBossDead = 0xbdd4,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfAreaMiniBossIsDead</c>.</summary>
    WakeIfAreaMiniBossDead = 0xbde3,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfAreaTorizoIsDead</c>.</summary>
    WakeIfAreaTorizoDead = 0xbdf2,
    /// <summary><c>PreInst_PLM_GotoLinkInst_SetZebesAwakeEventIfEnemiesDead</c>.</summary>
    WakeIfEnemiesDead = 0xbe01,
    /// <summary><c>PreInstruction_PLM_PlayDudSound</c>: the door never unlocks.</summary>
    PlayDudSound = 0xbe1c,
    /// <summary><c>PreInst_PLM_GotoLinkInstIfTourianStatueFinishedProcessing</c>.</summary>
    WakeIfTourianStatueFinished = 0xbe1f,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfCrittersEscaped</c>.</summary>
    WakeIfCrittersEscaped = 0xbe30,
}
