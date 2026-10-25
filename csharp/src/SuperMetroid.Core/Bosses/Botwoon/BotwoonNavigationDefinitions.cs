namespace SuperMetroid.Core.Bosses.Botwoon;
/// <summary>Compiled fixed geometry and path metadata for Botwoon's room navigation.</summary>
internal static class BotwoonNavigationDefinitions
{

    /// <summary>
    /// <c>$B3:989D</c>: random bits 1-3 pick an even action offset 0-14 when Botwoon leaves a hole outside
    /// its health phases; offsets 0, 2 and 4 follow an authored path and the rest spit from the hole.
    /// </summary>
    internal const ushort ActionChoiceMask = 0x000e;

    /// <summary>Random bits 3-4 pick one of the target hole's four path descriptors.</summary>
    internal const ushort PathChoiceMask = 0x0018;

    /// <summary>The head-position history ring's X/Y samples.</summary>
    internal const int HistoryEntries = 256;

    /// <summary>Bytes per history sample in native WRAM; native cursors and spacings are entry indices times this.</summary>
    internal const int HistoryEntryBytes = 4;

    /// <summary>Wraps a history-ring index, as the native <c>AND #$03FF</c> does to its byte offset.</summary>
    internal static int WrapHistoryIndex(int index) => index & (HistoryEntries - 1);

    /// <summary>Random bits below the path variant: bits 3-4 select one of a hole's four path descriptors.</summary>
    internal const int PathChoiceVariantShift = 3;

    /// <summary>Path descriptors authored for each source hole and visibility.</summary>
    internal const int PathVariantsPerHole = 4;

    /// <summary>The first of the sixteen path choices that begin inside a hole; the visible choices precede them.</summary>
    internal const int HiddenPathChoiceBase = 16;

    /// <summary>The number of authored path choices: four variants from each of four holes, visible or hidden.</summary>
    internal const int PathChoiceCount = 32;

    /// <summary>
    /// The four room holes in their authored order: left (centre 64,112), bottom (128,176), top (160,96) and right
    /// (224,144). A hole's index here is its position in the path table's per-hole groups.
    /// </summary>
    private static readonly BotwoonHoleDefinition[] Holes =
    [
        new(new(64 - 4, 112 - 4)),
        new(new(128 - 4, 176 - 4)),
        new(new(160 - 4, 96 - 4)),
        new(new(224 - 4, 144 - 4)),
    ];

    /// <summary>The number of room holes.</summary>
    internal static int HoleCount => Holes.Length;

    /// <summary>Returns the room hole at <paramref name="holeIndex"/>, zero through three.</summary>
    internal static BotwoonHoleDefinition HoleAt(int holeIndex) =>
        (uint)holeIndex < (uint)Holes.Length
            ? Holes[holeIndex]
            : throw new InvalidDataException($"Botwoon hole index {holeIndex} is invalid.");

    /// <summary>
    /// Computes the 32 descriptors of <c>BotwoonMovementTable</c> at $B3:E150-$E24F
    /// from the path choice: visible choices 0-15 and hidden choices 16-31, four per source hole.
    /// </summary>
    internal static BotwoonPathDescriptorDefinition PathForChoice(int choice)
    {
        if ((uint)choice >= PathChoiceCount)
        {
            throw new InvalidDataException(
                $"Botwoon path choice {choice} is invalid.");
        }

        bool hidden = choice >= HiddenPathChoiceBase;
        int source = choice % HiddenPathChoiceBase / PathVariantsPerHole;
        int destinationRank = choice % PathVariantsPerHole;
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
            stream = HiddenPathChoiceBase + lower * (7 - lower) / 2 + upper - lower - 1;
        }
        return new(PathBoundary(stream + (backwards ? 1 : 0), !backwards),
            backwards ? (short)-1 : (short)0, destination);
    }

    /// <summary>
    /// Finds the ordinal (-128, 0) stream separator; the earlier (-128, -16) terminator
    /// in the second visible stream is followed by padding and does not start another stream.
    /// Reverse descriptors start at the separator, for the consumer's two-sample rewind.
    /// </summary>
    private static int PathBoundary(int ordinal, bool afterSeparator)
    {
        for (int sampleIndex = 0; sampleIndex < BotwoonMovementSampleData.Count; sampleIndex++)
        {
            Offset16<RoomPixels> sample = MovementSample(sampleIndex);
            if (sample.X == sbyte.MinValue && sample.Y == 0 && ordinal-- == 0)
                return sampleIndex + (afterSeparator ? 1 : 0);
        }
        throw new InvalidDataException("Botwoon movement stream separator is missing.");
    }

    /// <summary>
    /// Returns the compiled signed movement sample at <paramref name="sampleIndex"/> in the authored corpus. A
    /// component of <see cref="sbyte.MinValue"/> is the authored stream separator/terminator.
    /// </summary>
    internal static Offset16<RoomPixels> MovementSample(int sampleIndex)
    {
        if ((uint)sampleIndex >= (uint)BotwoonMovementSampleData.Count)
        {
            throw new InvalidDataException(
                $"Botwoon movement sample {sampleIndex} is outside the authored sample corpus.");
        }

        int packed = BotwoonMovementSampleData.At(sampleIndex);
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
