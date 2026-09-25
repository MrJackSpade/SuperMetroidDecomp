namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed bank-$8B actor programs for the delivery and examination scenes.</summary>
internal static class IntroScientistInstructionDefinitions
{
    /// <summary>$8B:CB9F, delivered baby's animation and page-four request.</summary>
    internal const ushort DeliveryStart = CinematicCodePointers.Lists.BabyMetroidBeingDelivered;
    /// <summary>$8B:CBCD, exclusive end of the delivered-baby program.</summary>
    internal const ushort DeliveryEnd = CinematicCodePointers.Lists.BabyMetroidBeingExamined;
    /// <summary>$8B:CBCD, examined baby's animation and page-five request.</summary>
    internal const ushort ExaminationStart = CinematicCodePointers.Lists.BabyMetroidBeingExamined;
    /// <summary>$8B:CC2B, exclusive end of the examined-baby program.</summary>
    internal const ushort ExaminationEnd = CinematicCodePointers.Lists.ConfusedBabyMetroid;
    /// <summary>$8B:CE53, the shared actor-delete instruction.</summary>
    internal const ushort DeletePointer = CinematicCodePointers.Lists.Delete;

    private static ReadOnlySpan<byte> DeliveryProgram =>
    [
        0xd6, 0x94, 0x0a, 0x00, 0x0a, 0x00, 0xf3, 0x8c,
        0x0a, 0x00, 0x13, 0x8d, 0x0a, 0x00, 0x33, 0x8d,
        0x0a, 0x00, 0x13, 0x8d, 0xc3, 0x94, 0xa3, 0xcb,
        0x46, 0xb3, 0x0a, 0x00, 0xf3, 0x8c, 0x0a, 0x00,
        0x13, 0x8d, 0x0a, 0x00, 0x33, 0x8d, 0x0a, 0x00,
        0x13, 0x8d, 0xbc, 0x94, 0xb9, 0xcb,
    ];

    private static ReadOnlySpan<byte> ExaminationProgram =>
    [
        0xd6, 0x94, 0x0a, 0x00, 0x0a, 0x00, 0x53, 0x8d,
        0x0a, 0x00, 0x5a, 0x8d, 0x0a, 0x00, 0x61, 0x8d,
        0x0a, 0x00, 0x5a, 0x8d, 0xc3, 0x94, 0xd1, 0xcb,
        0x4e, 0xb3, 0x0a, 0x00, 0x53, 0x8d, 0x0a, 0x00,
        0x5a, 0x8d, 0x0a, 0x00, 0x61, 0x8d, 0x0a, 0x00,
        0x5a, 0x8d, 0xbc, 0x94, 0xe7, 0xcb, 0x05, 0x00,
        0x68, 0x8d, 0xbc, 0x94, 0xfb, 0xcb, 0x05, 0x00,
        0x68, 0x8d, 0x05, 0x00, 0x00, 0x00, 0xbc, 0x94,
        0x03, 0xcc, 0x0a, 0x00, 0xcf, 0x8c, 0x0a, 0x00,
        0xdb, 0x8c, 0x0a, 0x00, 0xe7, 0x8c, 0x0a, 0x00,
        0xdb, 0x8c, 0xbc, 0x94, 0x0f, 0xcc, 0x3c, 0x00,
        0x00, 0x00, 0xbc, 0x94, 0x0f, 0xcc,
    ];

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer >= DeliveryStart && pointer < DeliveryEnd)
            return DeliveryProgram[pointer - DeliveryStart];
        if (pointer >= ExaminationStart && pointer < ExaminationEnd)
            return ExaminationProgram[pointer - ExaminationStart];
        if (pointer >= DeletePointer && pointer < DeletePointer + 2)
            return IntroBabyDiscoveryInstructionDefinitions.ReadByte(pointer);
        throw new InvalidDataException(
            $"Intro scientist actor byte $8B:{pointer:X4} leaves its compiled lists.");
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer == DeletePointer)
            return IntroBabyDiscoveryInstructionDefinitions.ReadWord(pointer);
        ReadOnlySpan<byte> source;
        int offset;
        if (pointer >= DeliveryStart && pointer < DeliveryEnd - 1)
        {
            source = DeliveryProgram;
            offset = pointer - DeliveryStart;
        }
        else if (pointer >= ExaminationStart && pointer < ExaminationEnd - 1)
        {
            source = ExaminationProgram;
            offset = pointer - ExaminationStart;
        }
        else
            throw new InvalidDataException(
                $"Intro scientist actor word $8B:{pointer:X4} leaves its compiled lists.");
        return (ushort)(source[offset] | source[offset + 1] << 8);
    }
}
