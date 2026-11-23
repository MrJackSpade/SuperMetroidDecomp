namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// The rules that choose where Botwoon's walks lead from each hole (#2510, #2557), each stated once over the room's holes:
/// a walk in the open leads to each other hole in turn and then back into the hole he left; a walk inside the wall leads to
/// each other hole, the last taken by two rolls, since no wall path leads back into the hole he left.
/// </summary>
internal static partial class BotwoonWalkDestinations
{
    /// <summary>The choice of a walk in the open from each hole of <paramref name="holes"/>, read from <paramref name="draw"/>.</summary>
    /// <param name="holes">The holes in the order the choices list them.</param>
    /// <param name="draw">The rolls of the walk's random draw the choice reads.</param>
    internal static IReadOnlyDictionary<BotwoonHole, WeightedRandomChoice<BotwoonHole>> InTheOpen(IReadOnlyList<BotwoonHole> holes, RandomSampleField draw) =>
        Ensure.NotNull(holes).ToDictionary(from => from, from => WeightedRandomChoice<BotwoonHole>.InTurn(draw, [.. Others(holes, from), from]));

    /// <summary>The choice of a walk inside the wall from each hole of <paramref name="holes"/>, read from <paramref name="draw"/>.</summary>
    /// <param name="holes">The holes in the order the choices list them.</param>
    /// <param name="draw">The rolls of the walk's random draw the choice reads.</param>
    internal static IReadOnlyDictionary<BotwoonHole, WeightedRandomChoice<BotwoonHole>> InsideTheWall(IReadOnlyList<BotwoonHole> holes, RandomSampleField draw) =>
        Ensure.NotNull(holes).ToDictionary(from => from, from =>
        {
            BotwoonHole[] others = Others(holes, from);
            return new WeightedRandomChoice<BotwoonHole>(draw,
                [.. others[..^1].Select(hole => new WeightedRandomOutcome<BotwoonHole>(1, hole)), new(2, others[^1])]);
        });

    /// <summary>The holes of <paramref name="holes"/> other than <paramref name="from"/>, in order.</summary>
    private static BotwoonHole[] Others(IReadOnlyList<BotwoonHole> holes, BotwoonHole from) => [.. holes.Where(hole => hole != from)];
}
