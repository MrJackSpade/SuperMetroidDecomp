namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed bank-$91 demo-controller program for the intro Mother Brain battle.</summary>
internal static class IntroMotherBrainInputDefinitions
{
    /// <summary>$91:8694, first no-input record before Samus begins firing.</summary>
    internal const ushort ListStart = 0x8694;
    /// <summary>$91:86FE, exclusive end after the last private opcode and delete.</summary>
    internal const ushort ListEnd = 0x86fe;
    /// <summary>$91:8784, six-byte Mother Brain demo-controller object header.</summary>
    internal const ushort HeaderStart = 0x8784;
    /// <summary>$91:878A, exclusive end of the object header.</summary>
    internal const ushort HeaderEnd = 0x878a;

    private static ReadOnlySpan<byte> List =>
    [
        0x5a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
        0x40, 0x00, 0x40, 0x00, 0x28, 0x00, 0x40, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x40, 0x00, 0x40, 0x00,
        0x1d, 0x00, 0x40, 0x00, 0x00, 0x00, 0x46, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x00, 0x02, 0x00, 0x02,
        0x07, 0x00, 0x00, 0x02, 0x00, 0x00, 0x01, 0x00,
        0x80, 0x02, 0x80, 0x00, 0x07, 0x00, 0x80, 0x02,
        0x00, 0x00, 0x04, 0x00, 0x00, 0x02, 0x00, 0x00,
        0x3c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
        0x40, 0x00, 0x40, 0x00, 0x28, 0x00, 0x40, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x40, 0x00, 0x40, 0x00,
        0x13, 0x00, 0x40, 0x00, 0x00, 0x00, 0x39, 0x87,
        0x27, 0x84,
    ];

    private static ReadOnlySpan<byte> Header =>
    [
        0xbf, 0x83, 0xbf, 0x83, 0x94, 0x86,
    ];

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer >= ListStart && pointer < ListEnd)
            return List[pointer - ListStart];
        if (pointer >= HeaderStart && pointer < HeaderEnd)
            return Header[pointer - HeaderStart];
        throw new InvalidDataException(
            $"Intro Mother Brain demo byte $91:{pointer:X4} leaves its compiled program.");
    }

    internal static ushort ReadWord(ushort pointer)
    {
        ReadOnlySpan<byte> source;
        int offset;
        if (pointer >= ListStart && pointer < ListEnd - 1)
        {
            source = List;
            offset = pointer - ListStart;
        }
        else if (pointer >= HeaderStart && pointer < HeaderEnd - 1)
        {
            source = Header;
            offset = pointer - HeaderStart;
        }
        else
            throw new InvalidDataException(
                $"Intro Mother Brain demo word $91:{pointer:X4} leaves its compiled program.");
        return (ushort)(source[offset] | source[offset + 1] << 8);
    }
}
