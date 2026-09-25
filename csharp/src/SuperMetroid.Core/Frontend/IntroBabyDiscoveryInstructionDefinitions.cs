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

    private static ReadOnlySpan<byte> EggProgram =>
    [
        0x05, 0x00, 0x6f, 0x8d, 0xbc, 0x94, 0x33, 0xcb,
        0x20, 0x00, 0x6f, 0x8d, 0xd6, 0x94, 0x04, 0x00,
        0x05, 0x00, 0x6f, 0x8d, 0x05, 0x00, 0x8f, 0x8d,
        0x05, 0x00, 0x6f, 0x8d, 0x05, 0x00, 0xbe, 0x8d,
        0xc3, 0x94, 0x43, 0xcb, 0x0a, 0x00, 0x6f, 0x8d,
        0x0a, 0x00, 0xed, 0x8d, 0x0a, 0x00, 0x1c, 0x8e,
        0x0a, 0x00, 0x4b, 0x8e, 0x0a, 0x00, 0x7a, 0x8e,
        0x0a, 0x00, 0xa9, 0x8e, 0x50, 0x00, 0xd8, 0x8e,
        0x18, 0xa9, 0x0a, 0x00, 0x07, 0x8f, 0x0a, 0x00,
        0x18, 0x8f, 0x0a, 0x00, 0x29, 0x8f, 0x0a, 0x00,
        0x3a, 0x8f, 0x0a, 0x00, 0x4b, 0x8f, 0x0a, 0x00,
        0x5c, 0x8f, 0x40, 0x01, 0x6d, 0x8f, 0x3e, 0xb3,
        0x4c, 0x94, 0x03, 0xa9, 0x50, 0x00, 0x6d, 0x8f,
        0xbc, 0x94, 0x97, 0xcb,
    ];

    private static ReadOnlySpan<byte> BabyProgram =>
    [
        0x0a, 0x00, 0xcb, 0x8f, 0x0a, 0x00, 0xd2, 0x8f,
        0x0a, 0x00, 0xd9, 0x8f, 0x0a, 0x00, 0xd2, 0x8f,
        0xbc, 0x94, 0x2b, 0xcc, 0x0a, 0x00, 0x9d, 0x90,
        0xbc, 0x94, 0x3f, 0xcc,
    ];

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer == DeletePointer)
            return (byte)(CinematicCodePointers.CinematicSpriteObject_Instruction_Delete & 0xff);
        if (pointer == DeletePointer + 1)
            return (byte)(CinematicCodePointers.CinematicSpriteObject_Instruction_Delete >> 8);
        if (pointer >= EggStart && pointer < EggEnd)
            return EggProgram[pointer - EggStart];
        if (pointer >= BabyStart && pointer < BabyEnd)
            return BabyProgram[pointer - BabyStart];
        throw new ArgumentOutOfRangeException(nameof(pointer));
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer == DeletePointer)
            return CinematicCodePointers.CinematicSpriteObject_Instruction_Delete;
        ReadOnlySpan<byte> source;
        int offset;
        if (pointer >= EggStart && pointer < EggEnd - 1)
        {
            source = EggProgram;
            offset = pointer - EggStart;
        }
        else if (pointer >= BabyStart && pointer < BabyEnd - 1)
        {
            source = BabyProgram;
            offset = pointer - BabyStart;
        }
        else
            throw new InvalidDataException(
                $"Intro baby-discovery instruction read $8B:{pointer:X4} leaves its compiled lists.");
        return (ushort)(source[offset] | source[offset + 1] << 8);
    }
}
