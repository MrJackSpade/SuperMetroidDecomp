namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Cartridge values of <see cref="CinematicCodePointers"/> that only verification reads.</summary>
internal static class CinematicCodePointersConstants
{
    /// <summary>Bank-$8B credits-object command at $9A0D that decrements a timer and branches.</summary>
    public const ushort CreditsObject_Instruction_DecrementTimerAndGoto = 0x9a0d;
    /// <summary>Bank-$8B credits-object command at $99FE that removes the object.</summary>
    public const ushort CreditsObject_Instruction_Delete = 0x99fe;
    /// <summary>Bank-$8B credits-object command at $F6FE that terminates the credits sequence.</summary>
    public const ushort CreditsObject_Instruction_EndCredits = 0xf6fe;
    /// <summary>Bank-$8B credits-object command at $9A17 that arms its countdown.</summary>
    public const ushort CreditsObject_Instruction_SetTimer = 0x9a17;
    /// <summary>Bank-$8B ending command at $E780 that clears the item-percentage subtitle.</summary>
    public const ushort Ending_Instruction_ClearItemPercentageSubtitle = 0xe780;
    /// <summary>Bank-$8B ending command at $E627 that renders the collected-item percentage.</summary>
    public const ushort Ending_Instruction_DrawItemPercentage = 0xe627;
    /// <summary>Bank-$8B ending command at $E769 that draws the percentage subtitle.</summary>
    public const ushort Ending_Instruction_DrawItemPercentageSubtitle = 0xe769;
    /// <summary>
    /// <c>CinematicBGObject_Instruction_GotoY</c> at $8B:971E, the only command in the bank-$8C eye lists; the eye
    /// program oracle follows it to the list's loop target.
    /// </summary>
    public const ushort CinematicBackgroundObject_Instruction_Goto = 0x971e;
    /// <summary><c>IndirectInstructionFunction_DoNothing</c> at $8B:8849.</summary>
    public const ushort IndirectInstruction_DoNothing = 0x8849;
    /// <summary><c>IndirectInstructionFunction_DrawTextCharacter</c> at $8B:884D.</summary>
    public const ushort IndirectInstruction_DrawTextCharacter = 0x884d;
    /// <summary><c>IndirectInstructionFunction_DrawToCinematicBGTilemap</c> at $8B:88B7.</summary>
    public const ushort IndirectInstruction_DrawToBackgroundTilemap = 0x88b7;

    /// <summary>Provides native cinematic instruction addresses used to verify credits and ending script decoding.</summary>
    extension(CinematicCodePointers)
    {
        /// <inheritdoc cref="CinematicCodePointersConstants.CreditsObject_Instruction_DecrementTimerAndGoto"/>
        internal static ushort CreditsObject_Instruction_DecrementTimerAndGoto => CinematicCodePointersConstants.CreditsObject_Instruction_DecrementTimerAndGoto;
        /// <inheritdoc cref="CinematicCodePointersConstants.CreditsObject_Instruction_Delete"/>
        internal static ushort CreditsObject_Instruction_Delete => CinematicCodePointersConstants.CreditsObject_Instruction_Delete;
        /// <inheritdoc cref="CinematicCodePointersConstants.CreditsObject_Instruction_EndCredits"/>
        internal static ushort CreditsObject_Instruction_EndCredits => CinematicCodePointersConstants.CreditsObject_Instruction_EndCredits;
        /// <inheritdoc cref="CinematicCodePointersConstants.CreditsObject_Instruction_SetTimer"/>
        internal static ushort CreditsObject_Instruction_SetTimer => CinematicCodePointersConstants.CreditsObject_Instruction_SetTimer;
        /// <inheritdoc cref="CinematicCodePointersConstants.Ending_Instruction_ClearItemPercentageSubtitle"/>
        internal static ushort Ending_Instruction_ClearItemPercentageSubtitle => CinematicCodePointersConstants.Ending_Instruction_ClearItemPercentageSubtitle;
        /// <inheritdoc cref="CinematicCodePointersConstants.Ending_Instruction_DrawItemPercentage"/>
        internal static ushort Ending_Instruction_DrawItemPercentage => CinematicCodePointersConstants.Ending_Instruction_DrawItemPercentage;
        /// <inheritdoc cref="CinematicCodePointersConstants.Ending_Instruction_DrawItemPercentageSubtitle"/>
        internal static ushort Ending_Instruction_DrawItemPercentageSubtitle => CinematicCodePointersConstants.Ending_Instruction_DrawItemPercentageSubtitle;
        /// <inheritdoc cref="CinematicCodePointersConstants.IndirectInstruction_DoNothing"/>
        internal static ushort IndirectInstruction_DoNothing => CinematicCodePointersConstants.IndirectInstruction_DoNothing;
        /// <inheritdoc cref="CinematicCodePointersConstants.IndirectInstruction_DrawTextCharacter"/>
        internal static ushort IndirectInstruction_DrawTextCharacter => CinematicCodePointersConstants.IndirectInstruction_DrawTextCharacter;
        /// <inheritdoc cref="CinematicCodePointersConstants.IndirectInstruction_DrawToBackgroundTilemap"/>
        internal static ushort IndirectInstruction_DrawToBackgroundTilemap => CinematicCodePointersConstants.IndirectInstruction_DrawToBackgroundTilemap;
    }
}
