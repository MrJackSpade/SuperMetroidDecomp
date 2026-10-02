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

/// <summary>One signed X/Y movement sample from Botwoon's authored path corpus.</summary>
internal readonly record struct BotwoonMovementSample(sbyte X, sbyte Y);

/// <summary>Compiled fixed geometry and path metadata for Botwoon's room navigation.</summary>
internal static class BotwoonNavigationDefinitions
{
    /// <summary>First movement-component pair at native address <c>$B3:A058</c>.</summary>
    internal const ushort MovementDataStart = 0xa058;

    /// <summary>Exclusive end of the movement corpus at native address <c>$B3:E150</c>.</summary>
    internal const ushort MovementDataEndExclusive = 0xe150;

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

    /// <summary>
    /// Computes the 32 descriptors of <c>BotwoonMovementTable</c> at $B3:E150-$E24F
    /// from the native eight-byte choice offset. The fourth word is alignment padding.
    /// </summary>
    internal static BotwoonPathDescriptorDefinition PathForChoiceByteOffset(ushort byteOffset)
    {
        if ((byteOffset & 7) != 0 || byteOffset > 248)
        {
            throw new InvalidDataException(
                $"Botwoon path-choice byte offset ${byteOffset:X4} is invalid.");
        }

        int choice = byteOffset / 8;
        bool hidden = choice >= 16;
        int source = choice % 16 / 4;
        int destinationRank = choice % 4;
        // Visible choices visit the three other holes then return to the source.
        // Hidden choices repeat the last other-hole choice instead of a self path.
        int destination = !hidden && destinationRank == 3 ? source :
            Math.Min(destinationRank, 2) + (Math.Min(destinationRank, 2) >= source ? 1 : 0);
        bool backwards = hidden && source > destination;
        int stream;
        if (!hidden) stream = choice;
        else
        {
            int lower = Math.Min(source, destination);
            int upper = Math.Max(source, destination);
            // Lexicographic rank of an unordered pair among four holes.
            stream = 16 + lower * (7 - lower) / 2 + upper - lower - 1;
        }
        return new(PathBoundary(stream + (backwards ? 1 : 0), !backwards),
            backwards ? (short)-1 : (short)0, (ushort)(8 * destination));
    }

    // The stream corpus uses (-128, 0) separators. The second visible stream
    // also has an earlier (-128, -16) terminator followed by padding; it does
    // not start a new stream. Reverse descriptors point to the end separator;
    // the existing consumer applies its native four-byte rewind.
    private static ushort PathBoundary(int ordinal, bool afterSeparator)
    {
        for (int pointer = MovementDataStart; pointer < MovementDataEndExclusive; pointer += 2)
        {
            BotwoonMovementSample sample = MovementSampleForPointer((ushort)pointer);
            if (sample.X == sbyte.MinValue && sample.Y == 0 && ordinal-- == 0)
                return (ushort)(pointer + (afterSeparator ? 2 : 0));
        }
        throw new InvalidDataException("Botwoon movement stream separator is missing.");
    }

    /// <summary>Returns the compiled signed component pair selected by a native path pointer.</summary>
    internal static BotwoonMovementSample MovementSampleForPointer(ushort pointer)
    {
        int byteOffset = pointer - MovementDataStart;
        if (byteOffset < 0 || pointer >= MovementDataEndExclusive || (byteOffset & 1) != 0)
        {
            throw new InvalidDataException(
                $"Botwoon movement pointer $B3:{pointer:X4} is outside the authored sample corpus.");
        }

        int sampleIndex = byteOffset >> 1;
        if (BotwoonMovementSampleData.Count !=
            (MovementDataEndExclusive - MovementDataStart) / 2)
        {
            throw new InvalidDataException(
                $"Compiled Botwoon movement corpus has {BotwoonMovementSampleData.Count} samples; " +
                $"expected {(MovementDataEndExclusive - MovementDataStart) / 2}.");
        }

        byte packed = BotwoonMovementSampleData.At(sampleIndex);
        return new BotwoonMovementSample(
            DecodeMovementComponent(packed >> 4),
            DecodeMovementComponent(packed & 0x0f));
    }

    private static sbyte DecodeMovementComponent(int code) => code switch
    {
        0 => sbyte.MinValue,
        1 => -16,
        2 => -1,
        3 => 0,
        4 => 1,
        _ => throw new InvalidDataException(
            $"Compiled Botwoon movement component code {code} is invalid."),
    };
}
