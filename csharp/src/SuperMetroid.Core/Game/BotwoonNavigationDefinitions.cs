namespace SuperMetroid.Core.Game;

/// <summary>One rectangular Botwoon hole and its four-pixel inset movement target.</summary>
/// <param name="Left">Left edge of the eight-pixel-wide hole hitbox.</param>
/// <param name="Top">Top edge of the eight-pixel-tall hole hitbox.</param>
internal readonly record struct BotwoonHoleDefinition(
    ushort Left,
    ushort Top)
{
    /// <summary>$B3:949D right boundaries: all four hole hitboxes are eight pixels wide.</summary>
    internal ushort Right => unchecked((ushort)(Left + 8));
    /// <summary>$B3:94A1 bottom boundaries: all four hole hitboxes are eight pixels tall.</summary>
    internal ushort Bottom => unchecked((ushort)(Top + 8));
    /// <summary>Horizontal center used as Botwoon's destination within this hole.</summary>
    internal ushort TargetX => unchecked((ushort)(Left + 4));
    /// <summary>Vertical center used as Botwoon's destination within this hole.</summary>
    internal ushort TargetY => unchecked((ushort)(Top + 4));
}

/// <summary>One authored Botwoon movement stream, traversal direction, and destination hole.</summary>
/// <param name="PathPointer">Bank-$B3 pointer to the first movement sample used by the native consumer.</param>
/// <param name="Direction">Traversal selector: zero advances through the stream and negative one selects reverse traversal.</param>
/// <param name="TargetHoleByteOffset">Eight-byte table offset identifying the destination hole.</param>
internal readonly record struct BotwoonPathDescriptorDefinition(
    ushort PathPointer,
    short Direction,
    ushort TargetHoleByteOffset);

/// <summary>One signed X/Y movement sample from Botwoon's authored path corpus.</summary>
/// <param name="X">Signed horizontal movement component decoded from the packed sample.</param>
/// <param name="Y">Signed vertical movement component decoded from the packed sample.</param>
internal readonly record struct BotwoonMovementSample(sbyte X, sbyte Y);

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
            BotwoonHoleLocation.Left => new(64 - 4, 112 - 4),
            BotwoonHoleLocation.Bottom => new(128 - 4, 176 - 4),
            BotwoonHoleLocation.Top => new(160 - 4, 96 - 4),
            BotwoonHoleLocation.Right => new(224 - 4, 144 - 4),
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
    /// <summary>Finds a movement stream's separator by its ordinal in the compiled sample corpus.</summary>
    /// <param name="ordinal">Zero-based separator number to locate.</param>
    /// <param name="afterSeparator">Whether to return the pointer immediately after the separator pair.</param>
    /// <returns>The bank-local pointer at the selected boundary.</returns>
    /// <exception cref="InvalidDataException">The requested separator does not exist in the movement corpus.</exception>
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

    /// <summary>Maps a packed four-bit movement code to its authored signed component value.</summary>
    /// <param name="code">Nibble from a compiled Botwoon movement sample.</param>
    /// <returns>The signed movement amount or the minimum-value separator sentinel.</returns>
    /// <exception cref="InvalidDataException">The nibble does not encode a supported movement value.</exception>
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
