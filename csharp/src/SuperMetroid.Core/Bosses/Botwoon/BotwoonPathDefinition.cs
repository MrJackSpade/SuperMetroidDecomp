namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One of Botwoon's authored paths: the holes it joins, where it runs and its steps (#1868).</summary>
/// <param name="From">The hole a forward walk starts from.</param>
/// <param name="To">The hole a forward walk reaches; a path in the open may lead back into the hole it left.</param>
/// <param name="Route">Whether the path runs in the open or inside the wall.</param>
/// <param name="Steps">The path's one-pixel steps from <paramref name="From"/> to <paramref name="To"/>.</param>
internal sealed record BotwoonPathDefinition(BotwoonHole From, BotwoonHole To, BotwoonPathRoute Route, BotwoonPathStep[] Steps);
