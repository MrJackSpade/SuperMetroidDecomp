namespace SuperMetroid.Core.Game;

/// <summary>
/// The common enemy instructions every enemy bank carries at the same bank-local offsets (the
/// bank-$A0 command set). The generic enemy interpreter executes these for every owner before
/// handing any other negative word to the owner's private instruction handler.
/// </summary>
internal enum CommonEnemyInstruction : ushort
{
    /// <summary>Stop the script and delete the actor at bank-local offset $807C.</summary>
    StopScript = 0x807c,
    /// <summary>Jump to the following same-bank instruction pointer at offset $80ED.</summary>
    Goto = 0x80ed,
    /// <summary>Decrement the loop timer and branch at offset $8108.</summary>
    DecrementTimerAndGoto = 0x8108,
    /// <summary>Bull's duplicate decrement-timer instruction at offset $8110.</summary>
    DecrementTimerAndGotoDuplicate = 0x8110,
    /// <summary>Load the loop timer from the following word at offset $8123.</summary>
    SetTimer = 0x8123,
    /// <summary>Sleep forever on the current instruction at offset $812F.</summary>
    Sleep = 0x812f,
    /// <summary>Wait for the following frame count at offset $813A.</summary>
    WaitFrames = 0x813a,
    /// <summary>Copy the following packed descriptor to VRAM at offset $814B.</summary>
    CopyToVram = 0x814b,
    /// <summary>Enable off-screen processing at offset $8173.</summary>
    EnableOffScreenProcessing = 0x8173,
    /// <summary>Disable off-screen processing at offset $817D.</summary>
    DisableOffScreenProcessing = 0x817d,
}
