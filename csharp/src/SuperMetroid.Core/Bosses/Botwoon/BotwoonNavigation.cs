namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's room navigation: its holes, the walk each draw leads to and the steps along it.</summary>
internal static class BotwoonNavigation
{
    /// <summary>The room hole <paramref name="hole"/>.</summary>
    internal static BotwoonHoleDefinition Hole(BotwoonHole hole) =>
        BotwoonNavigationDefinitions.Holes.Single(definition => definition.Hole == hole);

    /// <summary>
    /// The walk draw <paramref name="draw"/>, zero through three, leads to from <paramref name="from"/> (#1868). A walk in
    /// the open follows its own path forward. A walk inside the wall follows the wall path between the two holes, forward
    /// from the path's first hole and backward from its second.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not one of the four draws.</exception>
    internal static BotwoonPathWalk WalkFrom(BotwoonHole from, bool insideWall, int draw)
    {
        BotwoonHole[] destinations = (insideWall
            ? BotwoonNavigationDefinitions.WallDestinations
            : BotwoonNavigationDefinitions.OpenDestinations)[from];
        if ((uint)draw >= (uint)destinations.Length)
            throw new InvalidDataException($"Botwoon path draw {draw} is not one of the {destinations.Length} draws.");

        BotwoonHole to = destinations[draw];
        if (!insideWall)
            return new(PathBetween(from, to, BotwoonPathRoute.Open), BotwoonPathTraversal.Forward, 0, to);

        BotwoonPathDefinition wall = BotwoonPathDefinitions.Paths.Single(path => path.Route == BotwoonPathRoute.InsideWall &&
            (path.From == from && path.To == to || path.From == to && path.To == from));
        return wall.From == from
            ? new(wall, BotwoonPathTraversal.Forward, 0, to)
            : new(wall, BotwoonPathTraversal.Backward, BackwardFirstStep(wall), to);
    }

    /// <summary>The authored path of <paramref name="route"/> from <paramref name="from"/> to <paramref name="to"/>.</summary>
    private static BotwoonPathDefinition PathBetween(BotwoonHole from, BotwoonHole to, BotwoonPathRoute route) =>
        BotwoonPathDefinitions.Paths.Single(path => path.Route == route && path.From == from && path.To == to);

    /// <summary>
    /// The step a backward walk of <paramref name="path"/> starts at:
    /// <see cref="BotwoonNavigationDefinitions.BackwardStartStepsBeforeEnd"/> steps before the path's end, so a
    /// backward walk never takes the path's last step.
    /// </summary>
    private static int BackwardFirstStep(BotwoonPathDefinition path) =>
        path.Steps.Length - BotwoonNavigationDefinitions.BackwardStartStepsBeforeEnd;

    /// <summary>
    /// Whether <paramref name="step"/> is one of <paramref name="path"/>'s steps. The two positions just outside the
    /// path, one before its first step and one after its last, are where a walk ends; any other position is corrupt
    /// state.
    /// </summary>
    internal static bool HasStep(BotwoonPathDefinition path, int step)
    {
        int length = path.Steps.Length;
        if (step < -1 || step > length)
            throw new InvalidDataException($"Botwoon's path from {path.From} to {path.To} has no position {step}.");
        return step >= 0 && step < length;
    }

    /// <summary>The movement of step <paramref name="step"/> of authored path <paramref name="path"/>, as authored.</summary>
    internal static Offset16<RoomPixels> Step(BotwoonPathDefinition path, int step) =>
        (uint)step < (uint)path.Steps.Length
            ? Movement(path.Steps[step])
            : throw new InvalidDataException($"Botwoon's path from {path.From} to {path.To} has no step {step}.");

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
