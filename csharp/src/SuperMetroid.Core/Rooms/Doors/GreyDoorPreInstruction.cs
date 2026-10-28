namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The pre-instructions a grey cap installs (#1296); the unlock checks are selected by the door's condition. The
/// values are arbitrary (#1414).
/// </summary>
internal enum GreyDoorPreInstruction
{
    /// <summary>No pre-instruction.</summary>
    None = 1,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfShot</c>.</summary>
    WakeWhenShot = 2,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfAreaBossDead</c>.</summary>
    WakeIfAreaBossDead = 3,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfAreaMiniBossIsDead</c>.</summary>
    WakeIfAreaMiniBossDead = 4,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfAreaTorizoIsDead</c>.</summary>
    WakeIfAreaTorizoDead = 5,
    /// <summary><c>PreInst_PLM_GotoLinkInst_SetZebesAwakeEventIfEnemiesDead</c>.</summary>
    WakeIfEnemiesDead = 6,
    /// <summary><c>PreInstruction_PLM_PlayDudSound</c>: the door never unlocks.</summary>
    PlayDudSound = 7,
    /// <summary><c>PreInst_PLM_GotoLinkInstIfTourianStatueFinishedProcessing</c>.</summary>
    WakeIfTourianStatueFinished = 8,
    /// <summary><c>PreInstruction_PLM_GotoLinkInstructionIfCrittersEscaped</c>.</summary>
    WakeIfCrittersEscaped = 9,
}
