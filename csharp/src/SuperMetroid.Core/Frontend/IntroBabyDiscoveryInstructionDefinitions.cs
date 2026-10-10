using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Fixed bank-$8B sprite instruction lists for the SR388 egg and the confused
/// baby. Scientist-scene baby lists and visual spritemaps are separate owners.
/// </summary>
internal static class IntroBabyDiscoveryInstructionDefinitions
{
    /// <summary>$8B:CB33, egg's pre-hatching frame loop.</summary>
    internal const ushort EggStart = CinematicCodePointers.Lists.MetroidEgg;
    /// <summary>$8B:CB9F, exclusive end of the egg's hatching/page-three program.</summary>
    internal const ushort EggEnd = CinematicCodePointers.Lists.BabyMetroidBeingDelivered;
    /// <summary>$8B:CC2B, confused baby's first four-frame animation loop.</summary>
    internal const ushort BabyStart = CinematicCodePointers.Lists.ConfusedBabyMetroid;
    /// <summary>$8B:CC47, exclusive end of the confused baby's two loops.</summary>
    internal const ushort BabyEnd = CinematicCodePointers.Lists.CeresUnderAttack;
    /// <summary>$8B:CE53, shared actor-delete instruction after the reverse crossfade.</summary>
    internal const ushort DeletePointer = CinematicCodePointers.Lists.Delete;

    private static ushort EggWord(int word)
    {
        if (word is >= 8 and < 16)
        {
            int phase = (word - 8) / 2;
            return (word & 1) == 0 ? (ushort)5
                : IntroDiscoveryActorSpriteDefinitions.EggFramePointer((phase & 1) == 0 ? 0 : (phase + 1) / 2);
        }
        if (word is >= 18 and < 32)
        {
            int phase = (word - 18) / 2;
            return (word & 1) == 0 ? (ushort)(phase == 6 ? 80 : 10)
                : IntroDiscoveryActorSpriteDefinitions.EggFramePointer(phase == 0 ? 0 : phase + 2);
        }
        if (word is >= 33 and < 47)
        {
            int offset = word - 33;
            int phase = offset / 2;
            return (offset & 1) == 0 ? (ushort)(phase == 6 ? 320 : 10)
                : IntroDiscoveryActorSpriteDefinitions.EggFramePointer(9 + phase);
        }
        return word switch
        {
            0 => 5,
            1 or 5 => IntroDiscoveryActorSpriteDefinitions.EggFramePointer(0),
            2 or 52 => (ushort)CinematicSpriteInstruction.Goto,
            3 => EggStart,
            4 => 32,
            6 => (ushort)CinematicSpriteInstruction.SetTimer,
            7 => 4,
            16 => (ushort)CinematicSpriteInstruction.DecrementTimerAndGoto,
            17 => EggStart + 16,
            32 => (ushort)IntroEggInstruction.SpawnParticles,
            47 => (ushort)IntroEggInstruction.StartIntroPage3,
            48 => (ushort)CinematicSpriteInstruction.SetPreInstruction,
            49 => CinematicCodePointers.PreInstruction_MetroidEgg_DeleteAfterCrossFade,
            50 => 80,
            51 => IntroDiscoveryActorSpriteDefinitions.EggFramePointer(15),
            53 => EggStart + 100,
            _ => throw new ArgumentOutOfRangeException(nameof(word)),
        };
    }

    private static ushort BabyWord(int word)
    {
        if (word < 8)
        {
            int phase = word / 2;
            return (word & 1) == 0 ? (ushort)10
                : IntroDiscoveryActorSpriteDefinitions.BabyFramePointer(phase <= 2 ? phase : 4 - phase);
        }
        return word switch
        {
            8 or 12 => (ushort)CinematicSpriteInstruction.Goto,
            9 => BabyStart,
            10 => 10,
            11 => IntroDiscoveryActorSpriteDefinitions.BabyLarge,
            13 => BabyStart + 20,
            _ => throw new ArgumentOutOfRangeException(nameof(word)),
        };
    }

    internal static byte ReadByte(ushort pointer)
    {
        ushort word;
        int offset;
        if (pointer is >= EggStart and < EggEnd)
        {
            offset = pointer - EggStart;
            word = EggWord(offset / 2);
        }
        else if (pointer is >= BabyStart and < BabyEnd)
        {
            offset = pointer - BabyStart;
            word = BabyWord(offset / 2);
        }
        else if (pointer is DeletePointer or (DeletePointer + 1))
        {
            offset = pointer - DeletePointer;
            word = (ushort)CinematicSpriteInstruction.Delete;
        }
        else throw new ArgumentOutOfRangeException(nameof(pointer));
        return (byte)(word >> (8 * (offset & 1)));
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer is DeletePointer or >= EggStart and < (EggEnd - 1)
            or >= BabyStart and < (BabyEnd - 1))
            return (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
        throw new InvalidDataException(
            $"Intro baby-discovery instruction read $8B:{pointer:X4} leaves its compiled lists.");
    }
}
