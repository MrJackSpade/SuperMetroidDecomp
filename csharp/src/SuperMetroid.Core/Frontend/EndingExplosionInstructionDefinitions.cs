namespace SuperMetroid.Core.Frontend;

/// <summary>Timed display and scene operations for the eight Zebes-explosion actors.</summary>
internal static class EndingExplosionInstructionDefinitions
{
    /// <summary>$8B:EB0F, exploding-planet program.</summary>
    internal const ushort Start = 0xeb0f;
    /// <summary>$8B:EB91, exclusive end after the afterglow loop.</summary>
    internal const ushort End = 0xeb91;
    /// <summary>$8B:EB3D, purple glow program.</summary>
    private const ushort Glow = 0xeb3d;
    /// <summary>$8B:EB51, explosion stars loop.</summary>
    private const ushort Stars = 0xeb51;
    /// <summary>$8B:EB59, delayed lava program.</summary>
    private const ushort Lava = 0xeb59;
    /// <summary>$8B:EB69, silhouette program.</summary>
    private const ushort Silhouette = 0xeb69;
    /// <summary>$8B:EB71, right stars and scene handoff.</summary>
    private const ushort RightStars = 0xeb71;
    /// <summary>$8B:EB81, left stars loop; also the right program's fallthrough.</summary>
    private const ushort LeftStars = 0xeb81;
    /// <summary>$8B:EB89, afterglow loop.</summary>
    private const ushort Afterglow = 0xeb89;
    /// <summary>$8C:A396, ExplodingPlanetZebesFrame1; ten consecutive four-part
    /// OAM records (22 bytes each) contain four damage, four flash and two core frames.</summary>
    private const ushort PlanetFrames = 0xa396;
    /// <summary>$8C:A472, ExplodingPlanetZebesGlow.</summary>
    private const ushort GlowFrame = 0xa472;
    /// <summary>$8C:A4B0, ZebesSupernovaPart1.</summary>
    private const ushort SupernovaFirst = 0xa4b0;
    /// <summary>$8C:A516, ZebesSupernovaPart2.</summary>
    private const ushort SupernovaSecond = 0xa516;
    /// <summary>$8C:A28B, ZebesBoomStarryBackground.</summary>
    private const ushort Starfield = 0xa28b;
    /// <summary>$8C:A57C, ZebesSupernovaPart3 (silhouette).</summary>
    private const ushort SilhouetteFrame = 0xa57c;
    /// <summary>$8C:A5E2, ZebesSupernovaPart4 (afterglow).</summary>
    private const ushort AfterglowFrame = 0xa5e2;
    /// <summary>$8B:F35A, Instruction_CinematicSpriteObject_ZebesExplosion_Stars_Left,
    /// pre-instruction installed after the right starfield ends the explosion.</summary>
    private const ushort StarsPreInstruction = 0xf35a;

    internal static ushort ReadWord(ushort pointer)
    {
        int offset = pointer - Start;
        if ((uint)offset >= End - Start || (offset & 1) != 0)
            throw new InvalidDataException($"Ending explosion instruction $8B:{pointer:X4} leaves its compiled lists.");
        if (pointer < Glow) return PlanetWord(offset / 2);
        if (pointer < Stars)
        {
            int word = (pointer - Glow) / 2;
            if (word >= 8) return word == 8 ? CinematicCodePointers.CinematicSpriteObject_Instruction_Goto : Glow;
            if ((word & 1) == 0) return 16;
            // Expand and contract through the same middle pose.
            return (word / 2) switch { 0 => GlowFrame, 2 => SupernovaSecond, _ => SupernovaFirst };
        }
        if (pointer < Lava) return HoldWord(pointer, Stars, Starfield);
        if (pointer < Silhouette)
        {
            int word = (pointer - Lava) / 2;
            if (word == 0) return 156;
            if (word == 1) return 0;
            if (word < 6) return (word & 1) == 0 ? (ushort)10 : PlanetFrame(8 + (word - 2) / 2);
            return word == 6 ? CinematicCodePointers.CinematicSpriteObject_Instruction_Goto : (ushort)(Lava + 4);
        }
        if (pointer < RightStars)
            return ((pointer - Silhouette) / 2) switch
            {
                0 => 8,
                1 => SilhouetteFrame,
                2 => CinematicCodePointers.Ending_Instruction_StartZebesExplosion,
                _ => CinematicCodePointers.CinematicSpriteObject_Instruction_Delete,
            };
        if (pointer < LeftStars)
            return ((pointer - RightStars) / 2) switch
            {
                0 => 144,
                1 or 4 => Starfield,
                2 => CinematicCodePointers.Ending_Instruction_ExplosionFinale,
                3 => 332,
                5 => CinematicCodePointers.Ending_Instruction_EndZebesExplosion,
                6 => CinematicCodePointers.CinematicSpriteObject_Instruction_SetPreInstruction,
                _ => StarsPreInstruction,
            };
        return pointer < Afterglow ? HoldWord(pointer, LeftStars, Starfield) : HoldWord(pointer, Afterglow, AfterglowFrame);
    }

    private static ushort PlanetWord(int word)
    {
        if (word == 0) return CinematicCodePointers.CinematicSpriteObject_Instruction_SetTimer;
        if (word == 1) return 5;
        // Repeat four damage poses five times, then fade and play four flash poses.
        if (word < 10) return (word & 1) == 0 ? (ushort)13 : PlanetFrame((word - 2) / 2);
        if (word == 10) return CinematicCodePointers.CinematicSpriteObject_Instruction_DecrementTimerAndGoto;
        if (word == 11) return Start + 4;
        if (word == 12) return CinematicCodePointers.Ending_Instruction_FadeExplosionPalette;
        if (word < 21) return (word & 1) != 0 ? (ushort)32 : PlanetFrame(4 + (word - 13) / 2);
        return word == 21 ? CinematicCodePointers.Ending_Instruction_SpawnExplosionSilhouette : CinematicCodePointers.CinematicSpriteObject_Instruction_Delete;
    }

    private static ushort PlanetFrame(int frame) => (ushort)(PlanetFrames + (2 + 4 * 5) * frame);

    private static ushort HoldWord(ushort pointer, ushort start, ushort frame) => ((pointer - start) / 2) switch
    {
        0 => 16,
        1 => frame,
        2 => CinematicCodePointers.CinematicSpriteObject_Instruction_Goto,
        _ => start,
    };
}
