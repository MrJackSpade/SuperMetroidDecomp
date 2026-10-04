namespace SuperMetroid.Core.Frontend;

/// <summary>Four native Ceres explosion programs expressed as frame, loop and deletion operations.</summary>
internal static class CeresExplosionInstructionDefinitions
{
    /// <summary>$8C:97F7, SpaceSpritemaps_IntroMotherBrainExplosionSmallFrame1.</summary>
    private const ushort SmallFrame1 = 0x97f7;
    /// <summary>$8C:97FE, SpaceSpritemaps_IntroMotherBrainExplosionSmallFrame2.</summary>
    private const ushort SmallFrame2 = 0x97fe;
    /// <summary>$8C:9805, SpaceSpritemaps_IntroMotherBrainExplosionSmallFrame3.</summary>
    private const ushort SmallFrame3 = 0x9805;
    /// <summary>$8C:981B, SpaceSpritemaps_IntroMotherBrainExplosionSmallFrame4.</summary>
    private const ushort SmallFrame4 = 0x981b;
    /// <summary>$8C:9831, SpaceSpritemaps_IntroMotherBrainExplosionSmallFrame5.</summary>
    private const ushort SmallFrame5 = 0x9831;
    /// <summary>$8C:9847, SpaceSpritemaps_IntroMotherBrainExplosionSmallFrame6.</summary>
    private const ushort SmallFrame6 = 0x9847;
    /// <summary>$8C:98D2, SpaceSpritemaps_CeresExplosionFrame1; four consecutive seven-byte single-sprite maps.</summary>
    private const ushort LargeFrameStart = 0x98d2;
    /// <summary>$8C:98EE, SpaceSpritemaps_CeresFinalExplosionFrame1.</summary>
    private const ushort StationFrame1 = 0x98ee;
    /// <summary>$8C:9904, SpaceSpritemaps_CeresFinalExplosionFrame2.</summary>
    private const ushort StationFrame2 = 0x9904;
    /// <summary>$8C:991A, SpaceSpritemaps_CeresFinalExplosionFrame3.</summary>
    private const ushort StationFrame3 = 0x991a;
    /// <summary>$8C:9930, SpaceSpritemaps_CeresFinalExplosionFrame4.</summary>
    private const ushort StationFrame4 = 0x9930;
    /// <summary>$8C:996E, SpaceSpritemaps_CeresFinalExplosionFrame5.</summary>
    private const ushort StationFrame5 = 0x996e;
    /// <summary>$8C:9998, SpaceSpritemaps_CeresFinalExplosionFrame6.</summary>
    private const ushort StationFrame6 = 0x9998;

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
        if (word == 12) return CinematicCodePointers.CinematicSpriteObject_Instruction_Delete;
        return (word & 1) == 0 ? duration : station ? StationFrame(word / 2) : SmallFrame(word / 2);
    }

    private static ushort RepeatingWord(int word, bool large)
    {
        if (word == 0) return CinematicCodePointers.CinematicSpriteObject_Instruction_SetTimer;
        if (word == 1) return (ushort)(large ? 7 : 6);
        int frameWords = (large ? 4 : 6) * 2;
        int body = word - 2;
        if (body < frameWords)
            return (body & 1) == 0 ? (ushort)(large ? 5 : 3) :
                large ? (ushort)(LargeFrameStart + 7 * (body / 2)) : SmallFrame(body / 2);
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

    private static ushort SmallFrame(int frame) => frame switch
    {
        0 => SmallFrame1, 1 => SmallFrame2, 2 => SmallFrame3,
        3 => SmallFrame4, 4 => SmallFrame5, 5 => SmallFrame6,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    private static ushort StationFrame(int frame) => frame switch
    {
        0 => StationFrame1, 1 => StationFrame2, 2 => StationFrame3,
        3 => StationFrame4, 4 => StationFrame5, 5 => StationFrame6,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
}
