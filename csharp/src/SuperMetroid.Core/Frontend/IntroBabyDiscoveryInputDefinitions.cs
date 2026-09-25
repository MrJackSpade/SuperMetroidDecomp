namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed bank-$91 object header and input lists for SR388 baby discovery.</summary>
internal static class IntroBabyDiscoveryInputDefinitions
{
    /// <summary>$91:860D, first running-left demo input record.</summary>
    internal const ushort ListStart = 0x860d;
    /// <summary>$91:864F, exclusive end after the terminal custom and delete opcodes.</summary>
    internal const ushort ListEnd = 0x864f;
    /// <summary>$91:877E, six-byte SR388 discovery demo object definition.</summary>
    internal const ushort HeaderStart = 0x877e;
    /// <summary>$91:8784, exclusive end of the SR388 demo object definition.</summary>
    internal const ushort HeaderEnd = 0x8784;

    private static ReadOnlySpan<byte> List =>
    [
        0x5a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
        0x00, 0x02, 0x00, 0x02, 0x01, 0x00, 0x00, 0x02,
        0x00, 0x00, 0x48, 0x84, 0x19, 0x86, 0x2c, 0x01,
        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x10, 0x00, 0x10,
        0x00, 0xaa, 0x00, 0x10, 0x00, 0x00, 0x00, 0xf0,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
        0x02, 0x00, 0x02, 0x01, 0x00, 0x00, 0x02, 0x00,
        0x00, 0x48, 0x84, 0x41, 0x86, 0x82, 0x86, 0x27,
        0x84,
    ];

    private static ReadOnlySpan<byte> Header =>
    [
        0xbf, 0x83, 0x4f, 0x86, 0x0d, 0x86,
    ];

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer >= ListStart && pointer < ListEnd)
            return List[pointer - ListStart];
        if (pointer >= HeaderStart && pointer < HeaderEnd)
            return Header[pointer - HeaderStart];
        throw new InvalidDataException(
            $"SR388 discovery demo byte $91:{pointer:X4} leaves its compiled program.");
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
                $"SR388 discovery demo word $91:{pointer:X4} leaves its compiled program.");
        return (ushort)(source[offset] | source[offset + 1] << 8);
    }
}
