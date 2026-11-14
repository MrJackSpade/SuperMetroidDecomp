namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A walk Botwoon chooses: the authored path, which way he walks it, where he starts and the hole he reaches.</summary>
/// <param name="Path">The authored path walked.</param>
/// <param name="Traversal">Which way the path is walked.</param>
/// <param name="FirstStep">The step the walk starts at.</param>
/// <param name="Destination">The hole the walk reaches.</param>
internal readonly record struct BotwoonPathWalk(
    BotwoonPathDefinition Path,
    BotwoonPathTraversal Traversal,
    int FirstStep,
    BotwoonHole Destination);
