using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>Four native Ceres explosion programs expressed as frame, loop and deletion operations.</summary>
internal static class CeresExplosionInstructionDefinitions
{
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

    private static ushort SinglePassWord(int word, ushort duration, bool station)
    {
        if (word == 12) return (ushort)CinematicSpriteInstruction.Delete;
        return (word & 1) == 0 ? duration : station ? CeresDestructionSpriteDefinitions.StationFrame(word / 2) : CeresDestructionSpriteDefinitions.SmallFrame(word / 2);
    }

    private static ushort RepeatingWord(int word, bool large)
    {
        if (word == 0) return (ushort)CinematicSpriteInstruction.SetTimer;
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
            2 => (ushort)CinematicSpriteInstruction.DecrementTimerAndGoto,
            3 => (ushort)((large ? CeresDestructionSpriteInstructionDefinitions.RepeatingExplosionEnd :
                CeresDestructionSpriteInstructionDefinitions.InitialExplosionEnd) + 4),
            4 => (ushort)CinematicSpriteInstruction.Delete,
            _ => throw new InvalidDataException("Ceres explosion cursor leaves its repeat program."),
        };
    }

}
