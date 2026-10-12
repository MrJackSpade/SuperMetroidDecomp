namespace SuperMetroid.Core.Game;

/// <summary>One rectangular Botwoon hole and its four-pixel inset movement target.</summary>
/// <param name="TopLeft">The $B3:949B/$B3:949F left and top boundaries of the hole hitbox.</param>
internal readonly record struct BotwoonHoleDefinition(Position16<RoomPixels> TopLeft)
{
    /// <summary>
    /// $B3:949D/$B3:94A1 exclusive right and bottom boundaries: all four hole hitboxes are
    /// eight pixels square.
    /// </summary>
    internal Position16<RoomPixels> BottomRight => TopLeft + new Offset16<RoomPixels>(8, 8);

    /// <summary>The hole's centre, four pixels inside its top-left corner.</summary>
    internal Position16<RoomPixels> Target => TopLeft + new Offset16<RoomPixels>(4, 4);
}

/// <summary>One authored Botwoon movement stream, traversal direction, and destination hole.</summary>
internal readonly record struct BotwoonPathDescriptorDefinition(
    ushort PathPointer,
    short Direction,
    ushort TargetHoleByteOffset);

/// <summary>The four room holes selected by native eight-byte offsets.</summary>
internal enum BotwoonHoleLocation : ushort
{
    /// <summary><c>BotwoonHoleHitboxes</c> left hole at $B3:949B; center (64,112).</summary>
    Left = 0,
    /// <summary><c>BotwoonHoleHitboxes</c> bottom hole at $B3:94A3; center (128,176).</summary>
    Bottom = 8,
    /// <summary><c>BotwoonHoleHitboxes</c> top hole at $B3:94AB; center (160,96).</summary>
    Top = 16,
    /// <summary><c>BotwoonHoleHitboxes</c> right hole at $B3:94B3; center (224,144).</summary>
    Right = 24,
}
/// <summary>Compiled fixed geometry and path metadata for Botwoon's room navigation.</summary>
internal static class BotwoonNavigationDefinitions
{
    /// <summary>First movement-component pair at native address <c>$B3:A058</c>.</summary>
    internal const ushort MovementDataStart = 0xa058;

    /// <summary>Exclusive end of the movement corpus at native address <c>$B3:E150</c>.</summary>
    internal const ushort MovementDataEndExclusive = 0xe150;

    /// <summary>Returns the named room hole selected by its native eight-byte offset.</summary>
    internal static BotwoonHoleDefinition HoleForByteOffset(ushort byteOffset) =>
        (BotwoonHoleLocation)byteOffset switch
        {
            BotwoonHoleLocation.Left => new(new(64 - 4, 112 - 4)),
            BotwoonHoleLocation.Bottom => new(new(128 - 4, 176 - 4)),
            BotwoonHoleLocation.Top => new(new(160 - 4, 96 - 4)),
            BotwoonHoleLocation.Right => new(new(224 - 4, 144 - 4)),
            _ => throw new InvalidDataException(
                $"Botwoon hole-table byte offset ${byteOffset:X4} is invalid."),
        };
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
            Offset16<RoomPixels> sample = MovementSampleForPointer((ushort)pointer);
            if (sample.X == sbyte.MinValue && sample.Y == 0 && ordinal-- == 0)
                return (ushort)(pointer + (afterSeparator ? 2 : 0));
        }
        throw new InvalidDataException("Botwoon movement stream separator is missing.");
    }

    /// <summary>
    /// Returns the compiled signed movement sample selected by a native path pointer. A
    /// component of <see cref="sbyte.MinValue"/> is the authored stream separator/terminator.
    /// </summary>
    internal static Offset16<RoomPixels> MovementSampleForPointer(ushort pointer)
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
        return new Offset16<RoomPixels>(
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
