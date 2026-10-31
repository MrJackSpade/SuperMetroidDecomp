namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The bank-$8B commands the retail credits row stream uses, decoded once at import (#1416: moved out of Core);
/// <see cref="CreditsPresentationExtractor"/> owns the instruction word that names each.
/// </summary>
internal enum CreditsInstruction
{
    /// <summary><c>Instruction_CreditsObject_TimerInY</c>: loads the row timer.</summary>
    SetTimer,
    /// <summary><c>Instruction_CreditsObject_DecrementTimer_GotoYIfNonZero</c>: counts the timer down and loops while it runs.</summary>
    DecrementTimerAndGoto,
    /// <summary><c>CreditsObject_Func2</c>, which ends the scrolling credits.</summary>
    EndCredits,
    /// <summary><c>Instruction_CreditsObject_Delete</c>.</summary>
    Delete,
}
