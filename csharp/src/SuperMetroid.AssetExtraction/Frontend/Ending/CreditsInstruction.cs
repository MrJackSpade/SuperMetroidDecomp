namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The bank-$8B commands the retail credits row stream uses, decoded once at import (#1416: moved out of Core).</summary>
internal enum CreditsInstruction
{
    /// <summary><c>Instruction_CreditsObject_TimerInY</c> at $8B:9A17.</summary>
    SetTimer = 0x9a17,
    /// <summary><c>Instruction_CreditsObject_DecrementTimer_GotoYIfNonZero</c> at $8B:9A0D.</summary>
    DecrementTimerAndGoto = 0x9a0d,
    /// <summary><c>CreditsObject_Func2</c> at $8B:F6FE, which ends the scrolling credits.</summary>
    EndCredits = 0xf6fe,
    /// <summary><c>Instruction_CreditsObject_Delete</c> at $8B:99FE.</summary>
    Delete = 0x99fe,
}
