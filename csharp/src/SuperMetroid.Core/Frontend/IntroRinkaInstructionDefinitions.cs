using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Calculated native8B:CDEB..CE1A programs: three expanding frames, movement,
/// then a four-phase pulse; the invisible spawner waits, creates two pairs and deletes.
/// Original byte and overlapping-word views are independently verified. Art is separate.
/// </summary>
internal static class IntroRinkaInstructionDefinitions
{
    /// <summary>$8B:CDEB, first Rinka frame in the actor instruction program.</summary>
    internal const ushort StartPointer = CinematicCodePointers.Lists.IntroRinka;
    /// <summary>$8B:CE1B, exclusive end after the spawner's delete opcode.</summary>
    internal const ushort EndPointer = 0xce1b;

    /// <summary>Compiles one 16-bit entry in the Rinka actor and spawner instruction programs.</summary>
    /// <param name="word">Zero-based word offset from <see cref="StartPointer"/>.</param>
    /// <returns>The native instruction operand or opcode word at that offset.</returns>
    private static ushort ProgramWord(int word)
    {
        if (word < 6)
            return (word & 1) == 0 ? (ushort)10 : IntroRinkaSpriteDefinitions.FramePointer(word / 2);
        if (word == 6) return CinematicCodePointers.Instruction_StartMoving_IntroRinka;
        if (word < 15)
        {
            int display = word - 7;
            return (display & 1) == 0 ? (ushort)10
                : IntroRinkaSpriteDefinitions.FramePointer(Math.Abs(display / 2 - 1));
        }
        if (word == 15) return CinematicCodePointers.CinematicSpriteObject_Instruction_Goto;
        if (word == 16) return StartPointer + 14;
        // Two invisible waits and their distinct spawn operations, then terminate.
        return (word - 17) switch
        {
            0 => 74,
            1 or 4 => 0,
            2 => CinematicCodePointers.Instruction_Spawn_IntroRinkas_0_1,
            3 => 128,
            5 => CinematicCodePointers.Instruction_Spawn_IntroRinkas_2_3,
            _ => CinematicCodePointers.CinematicSpriteObject_Instruction_Delete,
        };
    }

    /// <summary>Reads one byte from the compiled instruction stream using its native address.</summary>
    /// <param name="pointer">Address within the half-open range from <see cref="StartPointer"/> to <see cref="EndPointer"/>.</param>
    /// <returns>The low or high byte of the compiled word containing the address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The address is outside the compiled stream.</exception>
    internal static byte ReadByte(ushort pointer)
    {
        if (pointer < StartPointer || pointer >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        int offset = pointer - StartPointer;
        return unchecked((byte)(ProgramWord(offset / 2) >> (8 * (offset & 1))));
    }

    /// <summary>Reads one little-endian instruction word without crossing the compiled stream boundary.</summary>
    /// <param name="pointer">Address of the first byte; both bytes must lie before <see cref="EndPointer"/>.</param>
    /// <returns>The two consecutive compiled bytes combined as a native instruction word.</returns>
    /// <exception cref="InvalidDataException">The address does not leave room for a complete word in the stream.</exception>
    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer < StartPointer || pointer >= EndPointer - 1)
            throw new InvalidDataException(
                $"Intro Rinka instruction read $8B:{pointer:X4} leaves its compiled programs.");
        return (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
    }
}
