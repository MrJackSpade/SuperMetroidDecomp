namespace SuperMetroid.Core.Game;

/// <summary>Bank-local instruction offsets shared by enemy bytecode interpreters.</summary>
internal static class CommonEnemyInstructionCodes
{
    /// <summary>Stop the script and delete the actor at bank-local offset $807C.</summary>
    public const ushort StopScript = 0x807c;
    /// <summary>Jump to the following same-bank instruction pointer at offset $80ED.</summary>
    public const ushort Goto = 0x80ed;
    /// <summary>Decrement the loop timer and branch at offset $8108.</summary>
    public const ushort DecrementTimerAndGoto = 0x8108;
    /// <summary>Bull's duplicate decrement-timer instruction at offset $8110.</summary>
    public const ushort DecrementTimerAndGotoDuplicate = 0x8110;
    /// <summary>Load the loop timer from the following word at offset $8123.</summary>
    public const ushort SetTimer = 0x8123;
    /// <summary>Sleep forever on the current instruction at offset $812F.</summary>
    public const ushort Sleep = 0x812f;
    /// <summary>Wait for the following frame count at offset $813A.</summary>
    public const ushort WaitFrames = 0x813a;
    /// <summary>Copy the following packed descriptor to VRAM at offset $814B.</summary>
    public const ushort CopyToVram = 0x814b;
    /// <summary>Enable off-screen processing at offset $8173.</summary>
    public const ushort EnableOffScreenProcessing = 0x8173;
    /// <summary>Disable off-screen processing at offset $817D.</summary>
    public const ushort DisableOffScreenProcessing = 0x817d;
}
