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

    private static ushort ProgramWord(int word)
    {
        if (word < 6)
            return (word & 1) == 0 ? (ushort)10 : IntroRinkaSpriteDefinitions.FramePointer(word / 2);
        if (word == 6) return (ushort)IntroRinkaInstruction.StartMoving;
        if (word < 15)
        {
            int display = word - 7;
            return (display & 1) == 0 ? (ushort)10
                : IntroRinkaSpriteDefinitions.FramePointer(Math.Abs(display / 2 - 1));
        }
        if (word == 15) return (ushort)CinematicSpriteInstruction.Goto;
        if (word == 16) return StartPointer + 14;
        // Two invisible waits and their distinct spawn operations, then terminate.
        return (word - 17) switch
        {
            0 => 74,
            1 or 4 => 0,
            2 => (ushort)IntroRinkaSpawnerInstruction.SpawnRinkas0And1,
            3 => 128,
            5 => (ushort)IntroRinkaSpawnerInstruction.SpawnRinkas2And3,
            _ => (ushort)CinematicSpriteInstruction.Delete,
        };
    }

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer is < StartPointer or >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        int offset = pointer - StartPointer;
        return unchecked((byte)(ProgramWord(offset / 2) >> (8 * (offset & 1))));
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer is < StartPointer or >= (EndPointer - 1))
            throw new InvalidDataException(
                $"Intro Rinka instruction read $8B:{pointer:X4} leaves its compiled programs.");
        return (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
    }
}
