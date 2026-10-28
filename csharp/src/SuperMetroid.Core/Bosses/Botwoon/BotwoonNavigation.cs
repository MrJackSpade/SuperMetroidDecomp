namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's room navigation: its holes, the path each choice follows and the movement samples along it.</summary>
internal static class BotwoonNavigation
{
    /// <summary>Returns the room hole at <paramref name="holeIndex"/>, zero through three.</summary>
    internal static BotwoonHoleDefinition HoleAt(int holeIndex) =>
        (uint)holeIndex < (uint)BotwoonNavigationDefinitions.Holes.Length
            ? BotwoonNavigationDefinitions.Holes[holeIndex]
            : throw new InvalidDataException($"Botwoon hole index {holeIndex} is invalid.");

    /// <summary>
    /// Computes the 32 descriptors of <c>BotwoonMovementTable</c> at $B3:E150-$E24F
    /// from the path choice: visible choices 0-15 and hidden choices 16-31, four per source hole.
    /// </summary>
    internal static BotwoonPathDescriptorDefinition PathForChoice(int choice)
    {
        if ((uint)choice >= BotwoonNavigationDefinitions.PathChoiceCount)
        {
            throw new InvalidDataException(
                $"Botwoon path choice {choice} is invalid.");
        }

        bool hidden = choice >= BotwoonNavigationDefinitions.HiddenPathChoiceBase;
        int source = choice % BotwoonNavigationDefinitions.HiddenPathChoiceBase / BotwoonNavigationDefinitions.PathVariantsPerHole;
        int destinationRank = choice % BotwoonNavigationDefinitions.PathVariantsPerHole;
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
            stream = BotwoonNavigationDefinitions.HiddenPathChoiceBase + lower * (7 - lower) / 2 + upper - lower - 1;
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
            MovementStep(packed >> BotwoonNavigationDefinitions.MovementXShift),
            MovementStep(packed & BotwoonNavigationDefinitions.MovementComponentMask));
    }

    /// <summary>Expands one packed movement nibble to an authored step or stream separator.</summary>
    private static sbyte MovementStep(int code) =>
        (uint)code < (uint)BotwoonNavigationDefinitions.MovementSteps.Length
            ? BotwoonNavigationDefinitions.MovementSteps[code]
            : throw new InvalidDataException($"Compiled Botwoon movement component code {code} is invalid.");
}
