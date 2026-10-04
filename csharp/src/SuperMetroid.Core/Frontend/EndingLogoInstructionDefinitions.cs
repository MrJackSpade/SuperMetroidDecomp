namespace SuperMetroid.Core.Frontend;

/// <summary>Display loops and wrap/reveal operations for the four assembling logo actors.</summary>
internal static class EndingLogoInstructionDefinitions
{
    /// <summary>$8B:EE5D, first upper-S display loop.</summary>
    internal const ushort Start = 0xee5d;
    /// <summary>$8B:EE9B, exclusive end after the lower-circle loop.</summary>
    internal const ushort End = 0xee9b;
    /// <summary>$8B:EE6D, upper-circle/right-wrap sequence.</summary>
    private const ushort RightWrapStart = 0xee6d;
    /// <summary>$8B:EE87, lower-circle/left-wrap sequence.</summary>
    private const ushort LeftWrapStart = 0xee87;
    /// <summary>$8C:B97F, EndingSequenceSpritemaps_ScrewAttackSymbolUpperPart.</summary>
    private const ushort UpperS = 0xb97f;
    /// <summary>$8C:B9C7, EndingSequenceSpritemaps_ScrewAttackSymbolLowerPart.</summary>
    private const ushort LowerS = 0xb9c7;
    /// <summary>$8C:BA0F, ScrewAttackSymbolRightWrapFrame1.</summary>
    private const ushort RightWrapFirst = 0xba0f;
    /// <summary>$8C:BA4D, ScrewAttackSymbolRightWrapFrame2.</summary>
    private const ushort RightWrapSecond = 0xba4d;
    /// <summary>$8C:BAA9, ScrewAttackSymbolRightWrapFrame3.</summary>
    private const ushort RightWrapComplete = 0xbaa9;
    /// <summary>$8C:BB28, ScrewAttackSymbolLeftWrapFrame1.</summary>
    private const ushort LeftWrapFirst = 0xbb28;
    /// <summary>$8C:BB66, ScrewAttackSymbolLeftWrapFrame2.</summary>
    private const ushort LeftWrapSecond = 0xbb66;
    /// <summary>$8C:BBC2, ScrewAttackSymbolLeftWrapFrame3.</summary>
    private const ushort LeftWrapComplete = 0xbbc2;

    /// <summary>Generate the exact31 aligned native words. S halves display for10 and
    /// loop. Circle halves wait96 blank frames, reveal three stages at5-frame intervals,
    /// and hold their completed frame. The upper half holds64 before requesting crossfade.
    /// Addresses select control positions; they are not samples of a numeric curve.</summary>
    internal static ushort ReadWord(ushort pointer)
    {
        int offset = pointer - Start;
        if ((uint)offset >= End - Start || (offset & 1) != 0)
            throw new InvalidDataException($"Ending logo instruction $8B:{pointer:X4} leaves its compiled lists.");
        if (pointer < RightWrapStart)
        {
            int actor = offset / 8;
            return (offset / 2 % 4) switch
            {
                0 => 10,
                1 => actor == 0 ? UpperS : LowerS,
                2 => CinematicCodePointers.CinematicSpriteObject_Instruction_Goto,
                _ => (ushort)(Start + actor * 8),
            };
        }
        bool right = pointer < LeftWrapStart;
        ushort start = right ? RightWrapStart : LeftWrapStart;
        int word = (pointer - start) / 2;
        if (word == 0) return 96;
        if (word == 1) return 0; // Blank during the approach.
        if (word < 8)
        {
            int stage = word / 2 - 1;
            return (word & 1) == 0 ? (ushort)(right && stage == 2 ? 64 : 5) : WrapFrame(right, stage);
        }
        if (!right)
            return word == 8 ? CinematicCodePointers.CinematicSpriteObject_Instruction_Goto : (ushort)(start + 12);
        return (word - 8) switch
        {
            0 => EndingLogoDefinitions.GreyOutInstruction,
            1 => 5,
            2 => RightWrapComplete,
            3 => CinematicCodePointers.CinematicSpriteObject_Instruction_Goto,
            _ => (ushort)(start + 18),
        };
    }

    private static ushort WrapFrame(bool right, int stage) => stage switch
    {
        0 => right ? RightWrapFirst : LeftWrapFirst,
        1 => right ? RightWrapSecond : LeftWrapSecond,
        _ => right ? RightWrapComplete : LeftWrapComplete,
    };
}