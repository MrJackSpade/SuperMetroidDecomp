using SuperMetroid.Core.Assets;
using Pose = SuperMetroid.Core.Assets.EndingExplosionSpriteDefinitions.Pose;

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
    /// <summary>$8B:EB71: right-star exposure before ExplosionFinale; the selected actor timing remains separately required.</summary>
    internal const ushort RightStarInitialHoldTicks = 144;
    /// <summary>$8B:EB81, left stars loop; also the right program's fallthrough.</summary>
    private const ushort LeftStars = 0xeb81;
    /// <summary>$8B:EB89, afterglow loop.</summary>
    private const ushort Afterglow = 0xeb89;
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
            return (word / 2) switch { 0 => EndingExplosionSpriteDefinitions.Pointer(Pose.Glow), 2 => EndingExplosionSpriteDefinitions.Pointer(Pose.SupernovaSecond), _ => EndingExplosionSpriteDefinitions.Pointer(Pose.SupernovaFirst) };
        }
        if (pointer < Lava) return HoldWord(pointer, Stars, EndingExplosionSpriteDefinitions.Pointer(Pose.Stars));
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
                1 => EndingExplosionSpriteDefinitions.Pointer(Pose.Silhouette),
                2 => CinematicCodePointers.Ending_Instruction_StartZebesExplosion,
                _ => CinematicCodePointers.CinematicSpriteObject_Instruction_Delete,
            };
        if (pointer < LeftStars)
            return ((pointer - RightStars) / 2) switch
            {
                0 => RightStarInitialHoldTicks,
                1 or 4 => EndingExplosionSpriteDefinitions.Pointer(Pose.Stars),
                2 => CinematicCodePointers.Ending_Instruction_ExplosionFinale,
                3 => 332,
                5 => CinematicCodePointers.Ending_Instruction_EndZebesExplosion,
                6 => CinematicCodePointers.CinematicSpriteObject_Instruction_SetPreInstruction,
                _ => StarsPreInstruction,
            };
        return pointer < Afterglow ? HoldWord(pointer, LeftStars, EndingExplosionSpriteDefinitions.Pointer(Pose.Stars)) : HoldWord(pointer, Afterglow, EndingExplosionSpriteDefinitions.Pointer(Pose.Afterglow));
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

    private static ushort PlanetFrame(int frame) => EndingExplosionSpriteDefinitions.Pointer((Pose)frame);

    private static ushort HoldWord(ushort pointer, ushort start, ushort frame) => ((pointer - start) / 2) switch
    {
        0 => 16,
        1 => frame,
        2 => CinematicCodePointers.CinematicSpriteObject_Instruction_Goto,
        _ => start,
    };
}
