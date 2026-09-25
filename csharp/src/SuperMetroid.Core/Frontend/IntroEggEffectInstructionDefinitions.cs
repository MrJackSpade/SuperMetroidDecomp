namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Fixed bank-$8B instruction lists for the six egg-shell fragments and
/// the slime drop's moving/impact phases. Visual spritemaps live in bank $8C.
/// </summary>
internal static class IntroEggEffectInstructionDefinitions
{
    /// <summary>$8B:CD39, first shell-fragment single-frame loop.</summary>
    internal const ushort StartPointer = CinematicCodePointers.Lists.MetroidEggParticle1;
    /// <summary>$8B:CD69, moving slime-drop frame loop.</summary>
    internal const ushort SlimeMovePointer = CinematicCodePointers.Lists.MetroidEggSlimeDrops;
    /// <summary>$8B:CD71, first slime impact frame.</summary>
    internal const ushort SlimeImpactPointer = CinematicCodePointers.Lists.MetroidEggParticleHitGround;
    /// <summary>$8B:CD83, exclusive end after the slime impact delete opcode.</summary>
    internal const ushort EndPointer = 0xcd83;
    /// <summary>$8B:CE53, shared cinematic sprite delete list used by shell fragments.</summary>
    internal const ushort DeletePointer = CinematicCodePointers.Lists.Delete;

    private static ReadOnlySpan<byte> Programs =>
    [
        0x01, 0x00, 0x7e, 0x8f, 0xbc, 0x94, 0x39, 0xcd,
        0x01, 0x00, 0x85, 0x8f, 0xbc, 0x94, 0x41, 0xcd,
        0x01, 0x00, 0x8c, 0x8f, 0xbc, 0x94, 0x49, 0xcd,
        0x01, 0x00, 0x93, 0x8f, 0xbc, 0x94, 0x51, 0xcd,
        0x01, 0x00, 0x9a, 0x8f, 0xbc, 0x94, 0x59, 0xcd,
        0x01, 0x00, 0xa1, 0x8f, 0xbc, 0x94, 0x61, 0xcd,
        0x01, 0x00, 0xa8, 0x8f, 0xbc, 0x94, 0x69, 0xcd,
        0x0a, 0x00, 0xaf, 0x8f, 0x0a, 0x00, 0xb6, 0x8f,
        0x0a, 0x00, 0xbd, 0x8f, 0x0a, 0x00, 0xc4, 0x8f,
        0x38, 0x94,
    ];

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer == DeletePointer)
            return (byte)(CinematicCodePointers.CinematicSpriteObject_Instruction_Delete & 0xff);
        if (pointer == DeletePointer + 1)
            return (byte)(CinematicCodePointers.CinematicSpriteObject_Instruction_Delete >> 8);
        if (pointer < StartPointer || pointer >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        return Programs[pointer - StartPointer];
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer == DeletePointer)
            return CinematicCodePointers.CinematicSpriteObject_Instruction_Delete;
        if (pointer < StartPointer || pointer >= EndPointer - 1)
            throw new InvalidDataException(
                $"Intro egg effect read $8B:{pointer:X4} leaves its compiled lists.");
        int offset = pointer - StartPointer;
        return (ushort)(Programs[offset] | Programs[offset + 1] << 8);
    }
}
