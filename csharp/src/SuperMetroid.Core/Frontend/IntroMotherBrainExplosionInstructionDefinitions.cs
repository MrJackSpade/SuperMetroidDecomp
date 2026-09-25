namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Fixed bank-$8B instruction lists for the small and large intro Mother Brain
/// explosions. Spritemap payloads live separately in bank $8C.
/// </summary>
internal static class IntroMotherBrainExplosionInstructionDefinitions
{
    /// <summary>$8B:CDAB, first large-explosion animation record.</summary>
    internal const ushort StartPointer = CinematicCodePointers.Lists.IntroMotherBrainExplosionBig;
    /// <summary>$8B:CDCB, first small-explosion animation record.</summary>
    internal const ushort SmallPointer = CinematicCodePointers.Lists.IntroMotherBrainExplosionSmall;
    /// <summary>$8B:CDEB, exclusive end before the next actor's stream.</summary>
    internal const ushort EndPointer = 0xcdeb;
    /// <summary>$8B:CE53, shared sprite-object delete list.</summary>
    internal const ushort DeletePointer = CinematicCodePointers.Lists.Delete;
    /// <summary>One native six-frame large loop occupies 52 frames including blank hold.</summary>
    internal const int BigLoopFrames = 52;
    /// <summary>One native six-frame small loop occupies 34 frames including blank hold.</summary>
    internal const int SmallLoopFrames = 34;

    private static ReadOnlySpan<byte> Program =>
    [
        0x06, 0x00, 0x5d, 0x98, 0x06, 0x00, 0x64, 0x98,
        0x06, 0x00, 0x7a, 0x98, 0x06, 0x00, 0x90, 0x98,
        0x06, 0x00, 0xa6, 0x98, 0x06, 0x00, 0xbc, 0x98,
        0x10, 0x00, 0x00, 0x00, 0xbc, 0x94, 0xab, 0xcd,
        0x03, 0x00, 0xf7, 0x97, 0x03, 0x00, 0xfe, 0x97,
        0x03, 0x00, 0x05, 0x98, 0x03, 0x00, 0x1b, 0x98,
        0x03, 0x00, 0x31, 0x98, 0x03, 0x00, 0x47, 0x98,
        0x10, 0x00, 0x00, 0x00, 0xbc, 0x94, 0xcb, 0xcd,
    ];

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer == DeletePointer)
            return (byte)(CinematicCodePointers.CinematicSpriteObject_Instruction_Delete & 0xff);
        if (pointer == DeletePointer + 1)
            return (byte)(CinematicCodePointers.CinematicSpriteObject_Instruction_Delete >> 8);
        if (pointer < StartPointer || pointer >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        return Program[pointer - StartPointer];
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer == DeletePointer)
            return CinematicCodePointers.CinematicSpriteObject_Instruction_Delete;
        if (pointer < StartPointer || pointer >= EndPointer - 1)
            throw new InvalidDataException(
                $"Intro Mother Brain explosion instruction read $8B:{pointer:X4} leaves its compiled program.");
        int offset = pointer - StartPointer;
        return (ushort)(Program[offset] | Program[offset + 1] << 8);
    }
}
