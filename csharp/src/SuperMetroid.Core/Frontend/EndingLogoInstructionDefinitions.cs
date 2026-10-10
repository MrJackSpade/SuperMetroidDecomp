using SuperMetroid.Core.Assets;

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
                1 => EndingLogoSpriteDefinitions.FramePointer(actor),
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
            2 => EndingLogoSpriteDefinitions.FramePointer(4),
            3 => CinematicCodePointers.CinematicSpriteObject_Instruction_Goto,
            _ => (ushort)(start + 18),
        };
    }

    /// <summary>Selects the native logo spritemap for one wrap/reveal stage on the requested side.</summary>
    /// <param name="right">Whether to select the upper-right actor's frame sequence.</param>
    /// <param name="stage">Zero-based reveal stage within the side's three-frame sequence.</param>
    private static ushort WrapFrame(bool right, int stage) =>
        EndingLogoSpriteDefinitions.FramePointer((right ? 2 : 5) + stage);
}
