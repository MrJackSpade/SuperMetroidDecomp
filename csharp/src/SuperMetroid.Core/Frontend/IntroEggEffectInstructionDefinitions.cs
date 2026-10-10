using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Calculated native8B:CD39..CD82 programs: seven one-tick self-loops and
/// four ten-tick impact frames followed by deletion. Byte/overlapping-word views
/// are independently verified for the six egg-shell fragments and
/// the slime drop's moving/impact phases. Visual spritemaps live in bank $8C.
/// </summary>
internal static class IntroEggEffectInstructionDefinitions
{
    /// <summary>$8B:CD39, first shell-fragment single-frame loop.</summary>
    internal const ushort StartPointer = CinematicCodePointers.Lists.MetroidEggParticle1;
    /// <summary>$8B:CD83, exclusive end after the slime impact delete opcode.</summary>
    internal const ushort EndPointer = 0xcd83;
    /// <summary>$8B:CE53, shared cinematic sprite delete list used by shell fragments.</summary>
    internal const ushort DeletePointer = CinematicCodePointers.Lists.Delete;

    private static ushort ProgramWord(int word)
    {
        if (word < 28)
        {
            int frame = word / 4;
            return (word % 4) switch
            {
                0 => 1,
                1 => IntroEggEffectSpriteDefinitions.FramePointer(frame),
                2 => (ushort)CinematicSpriteInstruction.Goto,
                _ => (ushort)(StartPointer + frame * 8),
            };
        }
        int impactWord = word - 28;
        if (impactWord == 8) return (ushort)CinematicSpriteInstruction.Delete;
        return (impactWord & 1) == 0 ? (ushort)10 : IntroEggEffectSpriteDefinitions.FramePointer(7 + impactWord / 2);
    }
    internal static byte ReadByte(ushort pointer)
    {
        if (pointer == DeletePointer)
            return (byte)((ushort)CinematicSpriteInstruction.Delete & 0xff);
        if (pointer == DeletePointer + 1)
            return (byte)((ushort)CinematicSpriteInstruction.Delete >> 8);
        if (pointer is < StartPointer or >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        int offset = pointer - StartPointer;
        return unchecked((byte)(ProgramWord(offset / 2) >> (8 * (offset & 1))));
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer == DeletePointer)
            return (ushort)CinematicSpriteInstruction.Delete;
        if (pointer is < StartPointer or >= (EndPointer - 1))
            throw new InvalidDataException(
                $"Intro egg effect read $8B:{pointer:X4} leaves its compiled lists.");
        return (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
    }
}
