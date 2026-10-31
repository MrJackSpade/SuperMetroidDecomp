namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's room navigation: its holes, the path each choice follows and the steps along it.</summary>
internal static class BotwoonNavigation
{
    /// <summary>Returns the room hole at <paramref name="holeIndex"/>, zero through three.</summary>
    internal static BotwoonHoleDefinition HoleAt(int holeIndex) =>
        (uint)holeIndex < (uint)BotwoonNavigationDefinitions.Holes.Length
            ? BotwoonNavigationDefinitions.Holes[holeIndex]
            : throw new InvalidDataException($"Botwoon hole index {holeIndex} is invalid.");

    /// <summary>
    /// The path choice <paramref name="choice"/> leads to: visible choices 0-15 and hidden choices 16-31, four per
    /// source hole. A visible choice walks its own open path forward. A hidden choice walks the wall path between its
    /// two holes, forward from the lower-numbered hole and backward from the higher.
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
        if (!hidden)
            return new(choice, BotwoonPathTraversal.Forward, 0, destination);

        int lower = Math.Min(source, destination);
        int upper = Math.Max(source, destination);
        // Lexicographic rank of an unordered pair among four holes.
        int path = BotwoonNavigationDefinitions.HiddenPathChoiceBase + lower * (7 - lower) / 2 + upper - lower - 1;
        return source > destination
            ? new(path, BotwoonPathTraversal.Backward, BackwardFirstStep(path), destination)
            : new(path, BotwoonPathTraversal.Forward, 0, destination);
    }

    /// <summary>
    /// The step a backward walk of <paramref name="path"/> starts at:
    /// <see cref="BotwoonNavigationDefinitions.BackwardStartStepsBeforeEnd"/> steps before the path's end, so a
    /// backward walk never takes the path's last step.
    /// </summary>
    private static int BackwardFirstStep(int path) =>
        PathAt(path).Length - BotwoonNavigationDefinitions.BackwardStartStepsBeforeEnd;

    /// <summary>
    /// Whether <paramref name="step"/> is one of <paramref name="path"/>'s steps. The two positions just outside the
    /// path, one before its first step and one after its last, are where a walk ends; any other position is corrupt
    /// state.
    /// </summary>
    internal static bool HasStep(int path, int step)
    {
        int length = PathAt(path).Length;
        if (step < -1 || step > length)
            throw new InvalidDataException($"Botwoon path {path} has no position {step}.");
        return step >= 0 && step < length;
    }

    /// <summary>The movement of step <paramref name="step"/> of authored path <paramref name="path"/>, as authored.</summary>
    internal static Offset16<RoomPixels> Step(int path, int step)
    {
        BotwoonPathStep[] steps = PathAt(path);
        if ((uint)step >= (uint)steps.Length)
            throw new InvalidDataException($"Botwoon path {path} has no step {step}.");
        return Movement(steps[step]);
    }

    /// <summary>The authored path numbered <paramref name="path"/>.</summary>
    private static BotwoonPathStep[] PathAt(int path) =>
        (uint)path < (uint)BotwoonPathDefinitions.Paths.Length
            ? BotwoonPathDefinitions.Paths[path]
            : throw new InvalidDataException($"Botwoon path {path} is not an authored path.");

    /// <summary>The one-pixel movement a path step stands for.</summary>
    private static Offset16<RoomPixels> Movement(BotwoonPathStep step) => step switch
    {
        BotwoonPathStep.Up => new(0, -1),
        BotwoonPathStep.UpRight => new(1, -1),
        BotwoonPathStep.Right => new(1, 0),
        BotwoonPathStep.DownRight => new(1, 1),
        BotwoonPathStep.Down => new(0, 1),
        BotwoonPathStep.DownLeft => new(-1, 1),
        BotwoonPathStep.Left => new(-1, 0),
        BotwoonPathStep.UpLeft => new(-1, -1),
        _ => throw new InvalidDataException($"Undefined Botwoon path step {step}."),
    };
}
