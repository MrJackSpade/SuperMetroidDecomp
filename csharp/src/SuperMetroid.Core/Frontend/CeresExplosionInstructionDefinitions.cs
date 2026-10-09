using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>Four native Ceres explosion programs expressed as frame, loop and deletion operations.</summary>
internal static class CeresExplosionInstructionDefinitions
{
    /// <summary>Reads one byte from a compiled initial, repeating, or station-blast instruction program.</summary>
    /// <param name="pointer">Bank-$8B address within one of the supported Ceres explosion lists.</param>
    /// <returns>The selected byte of the synthesized instruction word.</returns>
    /// <exception cref="InvalidDataException">The pointer is outside all compiled explosion lists.</exception>
    internal static byte ReadByte(ushort pointer)
    {
        int offset;
        ushort word;
        if (pointer is >= CeresDestructionSpriteInstructionDefinitions.ExplosionsStart and < CeresDestructionSpriteInstructionDefinitions.InitialExplosionEnd)
        {
            offset = pointer - CeresDestructionSpriteInstructionDefinitions.ExplosionsStart;
            word = SinglePassWord(offset / 2, 3, station: false);
        }
        else if (pointer is >= CeresDestructionSpriteInstructionDefinitions.InitialExplosionEnd and < CeresDestructionSpriteInstructionDefinitions.RepeatingExplosionEnd)
        {
            offset = pointer - CeresDestructionSpriteInstructionDefinitions.InitialExplosionEnd;
            word = RepeatingWord(offset / 2, large: false);
        }
        else if (pointer is >= CeresDestructionSpriteInstructionDefinitions.RepeatingExplosionEnd and < CeresDestructionSpriteInstructionDefinitions.ExplosionsEnd)
        {
            offset = pointer - CeresDestructionSpriteInstructionDefinitions.RepeatingExplosionEnd;
            word = RepeatingWord(offset / 2, large: true);
        }
        else if (pointer is >= CeresDestructionSpriteInstructionDefinitions.StationBlastStart and < CeresDestructionSpriteInstructionDefinitions.StationBlastEnd)
        {
            offset = pointer - CeresDestructionSpriteInstructionDefinitions.StationBlastStart;
            word = SinglePassWord(offset / 2, 5, station: true);
        }
        else throw new InvalidDataException($"Ceres explosion instruction $8B:{pointer:X4} leaves its compiled lists.");
        return (byte)(word >> (8 * (offset & 1)));
    }

    /// <summary>Builds a word from a single-pass blast's duration/frame pairs and terminal delete command.</summary>
    /// <param name="word">Zero-based word position within the single-pass program.</param>
    /// <param name="duration">Frame duration used before each sprite-map operand.</param>
    /// <param name="station"><see langword="true"/> to select station-blast frames; otherwise selects small-explosion frames.</param>
    /// <returns>The duration, frame pointer, or delete opcode at that position.</returns>
    private static ushort SinglePassWord(int word, ushort duration, bool station)
    {
        if (word == 12) return CinematicCodePointers.CinematicSpriteObject_Instruction_Delete;
        return (word & 1) == 0 ? duration : station ? CeresDestructionSpriteDefinitions.StationFrame(word / 2) : CeresDestructionSpriteDefinitions.SmallFrame(word / 2);
    }

    /// <summary>Builds one word of a looping large- or small-explosion program, including its timer and deletion tail.</summary>
    /// <param name="word">Zero-based word position within the repeating program.</param>
    /// <param name="large"><see langword="true"/> for the large-explosion sequence; otherwise uses the small sequence.</param>
    /// <returns>The timer operation, duration, frame pointer, blank-frame value, loop command, or delete opcode.</returns>
    /// <exception cref="InvalidDataException">The word position lies beyond the compiled repeat program.</exception>
    private static ushort RepeatingWord(int word, bool large)
    {
        if (word == 0) return CinematicCodePointers.CinematicSpriteObject_Instruction_SetTimer;
        if (word == 1) return (ushort)(large ? 7 : 6);
        int frameWords = (large ? 4 : 6) * 2;
        int body = word - 2;
        if (body < frameWords)
            return (body & 1) == 0 ? (ushort)(large ? 5 : 3) :
                large ? CeresDestructionSpriteDefinitions.LargeFrame(body / 2) : CeresDestructionSpriteDefinitions.SmallFrame(body / 2);
        return (body - frameWords) switch
        {
            0 => (ushort)(large ? 8 : 16),
            1 => 0, // Blank frame between cycles.
            2 => CinematicCodePointers.CinematicSpriteObject_Instruction_DecrementTimerAndGoto,
            3 => (ushort)((large ? CeresDestructionSpriteInstructionDefinitions.RepeatingExplosionEnd :
                CeresDestructionSpriteInstructionDefinitions.InitialExplosionEnd) + 4),
            4 => CinematicCodePointers.CinematicSpriteObject_Instruction_Delete,
            _ => throw new InvalidDataException("Ceres explosion cursor leaves its repeat program."),
        };
    }

}
