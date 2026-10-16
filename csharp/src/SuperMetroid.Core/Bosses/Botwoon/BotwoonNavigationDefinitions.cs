namespace SuperMetroid.Core.Bosses.Botwoon;
/// <summary>Compiled fixed geometry and path metadata for Botwoon's room navigation.</summary>
internal static class BotwoonNavigationDefinitions
{
    /// <summary>First movement-component pair at native address <c>$B3:A058</c>.</summary>
    internal const ushort MovementDataStart = 0xa058;

    /// <summary>Exclusive end of the movement corpus at native address <c>$B3:E150</c>.</summary>
    internal const ushort MovementDataEndExclusive = 0xe150;

    /// <summary>
    /// <c>$B3:989D</c>: random bits 1-3 pick an even action offset 0-14 when Botwoon leaves a hole outside
    /// its health phases; offsets 0, 2 and 4 follow an authored path and the rest spit from the hole.
    /// </summary>
    internal const ushort ActionChoiceMask = 0x000e;

    /// <summary>Random bits 3-4 pick one of the target hole's four path descriptors (byte offsets 0, 8, 16, 24).</summary>
    internal const ushort PathChoiceMask = 0x0018;

    /// <summary>The head-position history ring's X/Y samples.</summary>
    internal const int HistoryEntries = 256;

    /// <summary>Bytes per history sample in native WRAM; native cursors and spacings are entry indices times this.</summary>
    internal const int HistoryEntryBytes = 4;

    /// <summary>Wraps a history-ring index, as the native <c>AND #$03FF</c> does to its byte offset.</summary>
    internal static int WrapHistoryIndex(int index) => index & (HistoryEntries - 1);

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

    /// <summary>
    /// Finds the ordinal (-128, 0) stream separator; the earlier (-128, -16) terminator
    /// in the second visible stream is followed by padding and does not start another stream.
    /// Reverse descriptors point at the separator for the consumer's native four-byte rewind.
    /// </summary>
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

    /// <summary>Expands one packed movement nibble to an authored step or stream separator.</summary>
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
