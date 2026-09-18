namespace SuperMetroid.Core.Game;

/// <summary>One rectangular Botwoon hole and its four-pixel inset movement target.</summary>
internal readonly record struct BotwoonHoleDefinition(
    ushort Left,
    ushort Right,
    ushort Top,
    ushort Bottom)
{
    internal ushort TargetX => unchecked((ushort)(Left + 4));
    internal ushort TargetY => unchecked((ushort)(Top + 4));
}

/// <summary>One authored Botwoon movement stream, traversal direction, and destination hole.</summary>
internal readonly record struct BotwoonPathDescriptorDefinition(
    ushort PathPointer,
    short Direction,
    ushort TargetHoleByteOffset);

/// <summary>Compiled fixed geometry and path metadata for Botwoon's room navigation.</summary>
internal static class BotwoonNavigationDefinitions
{
    /// <summary>
    /// The four eight-byte hole rectangles at <c>$B3:949B-$94BA</c>. Native callers retain
    /// their byte offsets so debugger-visible Botwoon state continues to match the cartridge.
    /// </summary>
    private static readonly BotwoonHoleDefinition[] Holes =
    [
        new(0x003c, 0x0044, 0x006c, 0x0074),
        new(0x007c, 0x0084, 0x00ac, 0x00b4),
        new(0x009c, 0x00a4, 0x005c, 0x0064),
        new(0x00dc, 0x00e4, 0x008c, 0x0094),
    ];

    /// <summary>
    /// The 32 eight-byte path descriptors at <c>$B3:E150-$E24F</c>. Each native record has
    /// a fourth zero word which is alignment padding, not movement state.
    /// </summary>
    private static readonly BotwoonPathDescriptorDefinition[] Paths =
    [
        new(0xa05a, 0, 0x0008),
        new(0xa32a, 0, 0x0010),
        new(0xa6bc, 0, 0x0018),
        new(0xaa24, 0, 0x0000),
        new(0xadfe, 0, 0x0000),
        new(0xb16a, 0, 0x0010),
        new(0xb556, 0, 0x0018),
        new(0xb956, 0, 0x0008),
        new(0xbc86, 0, 0x0000),
        new(0xc086, 0, 0x0008),
        new(0xc290, 0, 0x0018),
        new(0xc690, 0, 0x0010),
        new(0xc9cc, 0, 0x0000),
        new(0xcdcc, 0, 0x0008),
        new(0xd140, 0, 0x0010),
        new(0xd4a2, 0, 0x0018),
        new(0xd880, 0, 0x0008),
        new(0xda02, 0, 0x0010),
        new(0xdb9c, 0, 0x0018),
        new(0xdb9c, 0, 0x0018),
        new(0xda00, -1, 0x0000),
        new(0xdd42, 0, 0x0010),
        new(0xde7e, 0, 0x0018),
        new(0xde7e, 0, 0x0018),
        new(0xdb9a, -1, 0x0000),
        new(0xde7c, -1, 0x0008),
        new(0xdfe0, 0, 0x0018),
        new(0xdfe0, 0, 0x0018),
        new(0xdd40, -1, 0x0000),
        new(0xdfde, -1, 0x0008),
        new(0xe14e, -1, 0x0010),
        new(0xe14e, -1, 0x0010),
    ];

    /// <summary>Returns a hole selected by its native eight-byte table offset.</summary>
    internal static BotwoonHoleDefinition HoleForByteOffset(ushort byteOffset)
    {
        if ((byteOffset & 7) != 0 || byteOffset > 24)
        {
            throw new InvalidDataException(
                $"Botwoon hole-table byte offset ${byteOffset:X4} is invalid.");
        }

        return Holes[byteOffset >> 3];
    }

    /// <summary>Returns a path descriptor selected by its native eight-byte choice offset.</summary>
    internal static BotwoonPathDescriptorDefinition PathForChoiceByteOffset(ushort byteOffset)
    {
        if ((byteOffset & 7) != 0 || byteOffset > 248)
        {
            throw new InvalidDataException(
                $"Botwoon path-choice byte offset ${byteOffset:X4} is invalid.");
        }

        return Paths[byteOffset >> 3];
    }
}
