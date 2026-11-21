namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's room navigation: its holes, the walk each draw leads to and the steps along it.</summary>
internal static class BotwoonNavigation
{
    /// <summary>The room hole <paramref name="hole"/>.</summary>
    internal static BotwoonHoleDefinition Hole(BotwoonHole hole) =>
        BotwoonNavigationDefinitions.Holes.Single(definition => definition.Hole == hole);

    /// <summary>
    /// The walk the random sample <paramref name="sample"/> leads to from <paramref name="from"/> (#1868, #2510), its
    /// destination chosen by <see cref="BotwoonNavigationDefinitions.OpenDestinations"/> or
    /// <see cref="BotwoonNavigationDefinitions.WallDestinations"/>. A walk in the open follows its own path forward. A walk
    /// inside the wall follows the wall path between the two holes, forward from the path's first hole and backward from its
    /// second.
    /// </summary>
    /// <param name="from">The hole the walk leaves.</param>
    /// <param name="insideWall">Whether the walk travels inside the wall.</param>
    /// <param name="sample">The walk's random sample, 0 through 65535.</param>
    internal static BotwoonPathWalk WalkFrom(BotwoonHole from, bool insideWall, int sample)
    {
        BotwoonHole to = (insideWall
            ? BotwoonNavigationDefinitions.WallDestinations
            : BotwoonNavigationDefinitions.OpenDestinations)[from].Choose(sample);
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
}
