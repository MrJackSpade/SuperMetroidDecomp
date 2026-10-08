using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Calculated bank-$8B small/large explosion loops: six sequential frames held
/// three/six ticks, a16-tick blank, then self-loop. Native8B:CDAB..CDEA and the
/// shared delete opcode are independently verified, including overlapping reads.
/// Spritemap identities follow the native record-size calculation in the asset catalog.
/// </summary>
internal static class IntroMotherBrainExplosionInstructionDefinitions
{
    /// <summary>$8B:CDAB, first large-explosion animation record.</summary>
    internal const ushort StartPointer = CinematicCodePointers.Lists.IntroMotherBrainExplosionBig;
    /// <summary>$8B:CDCB, first small-explosion animation record.</summary>
    internal const ushort SmallPointer = CinematicCodePointers.Lists.IntroMotherBrainExplosionSmall;
    /// <summary>$8B:CDEB, exclusive end before the next actor's stream.</summary>
    internal const ushort EndPointer = 0xcdeb;
    /// <summary>$8B:CE53, shared sprite-object delete list.</summary>
    internal const ushort DeletePointer = CinematicCodePointers.Lists.Delete;

    private static ushort ProgramWord(int index)
    {
        bool big = index < 16;
        int word = index % 16;
        if (word < 12)
            return (word & 1) == 0 ? (ushort)(big ? 6 : 3)
                : IntroMotherBrainExplosionSpriteDefinitions.FramePointer(big, word / 2);
        return word switch
        {
            12 => 16, // blank hold duration
            13 => 0, // no spritemap
            14 => CinematicCodePointers.CinematicSpriteObject_Instruction_Goto,
            _ => big ? StartPointer : SmallPointer,
        };
    }

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer == DeletePointer)
            return (byte)(CinematicCodePointers.CinematicSpriteObject_Instruction_Delete & 0xff);
        if (pointer == DeletePointer + 1)
            return (byte)(CinematicCodePointers.CinematicSpriteObject_Instruction_Delete >> 8);
        if (pointer < StartPointer || pointer >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        int offset = pointer - StartPointer;
        return unchecked((byte)(ProgramWord(offset / 2) >> (8 * (offset & 1))));
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer == DeletePointer)
            return CinematicCodePointers.CinematicSpriteObject_Instruction_Delete;
        if (pointer < StartPointer || pointer >= EndPointer - 1)
            throw new InvalidDataException(
                $"Intro Mother Brain explosion instruction read $8B:{pointer:X4} leaves its compiled program.");
        return (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
    }
}
