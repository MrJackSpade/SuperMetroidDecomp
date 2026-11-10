using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1599): the graphics set each population a fixture's room places draws under, which only the import
/// knows. The game draws a population's enemies in the variants installed for it; a fixture that seeds a population of its
/// own records here the set its seeded room header names, and its artwork is composed from that set. A retail population
/// draws under the set the import decodes for it.
/// </summary>
internal static class FixturePopulationSets
{
    /// <summary>Each fixture population's set, by the population's definition, which lives as long as the fixture holds it.</summary>
    private static readonly ConditionalWeakTable<RoomEnemyPopulationDefinition, RoomEnemyGraphicsSetDefinition> sets = new();

    /// <summary>The population of no enemies each set's pictures are drawn through, by the set's definition, made once.</summary>
    private static readonly Dictionary<RoomEnemyGraphicsSetDefinition, RoomEnemyPopulationDefinition> drawing = new(ReferenceEqualityComparer.Instance);

    /// <summary>Records that fixture population <paramref name="population"/> draws under <paramref name="set"/>.</summary>
    /// <exception cref="InvalidDataException">The population's set is already recorded as another.</exception>
    internal static void Record(RoomEnemyPopulationDefinition population, RoomEnemyGraphicsSetDefinition set)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(set);
        if (!sets.TryAdd(population, set) && sets.TryGetValue(population, out RoomEnemyGraphicsSetDefinition? recorded) && !ReferenceEquals(recorded, set))
            throw new InvalidDataException($"Fixture population {population.Name} already draws under graphics set {recorded.Name}, not {set.Name}.");
    }

    /// <summary>
    /// A population whose artwork is <paramref name="set"/>'s, for a fixture that shows a set's pictures without loading a
    /// room: a retail population drawing under the set when one does, which the installation holds artwork for, or else a
    /// population of no enemies of the fixture's own, recorded as drawing under it.
    /// </summary>
    internal static RoomEnemyPopulationDefinition Drawing(RoomEnemyGraphicsSetDefinition set)
    {
        ArgumentNullException.ThrowIfNull(set);
        if (RetailDrawing(set) is { } retail)
            return retail;
        lock (drawing)
        {
            if (!drawing.TryGetValue(set, out RoomEnemyPopulationDefinition? population))
            {
                drawing.Add(set, population = new RoomEnemyPopulationDefinition($"fixture population drawing under {set.Name}", [], 0));
                Record(population, set);
            }
            return population;
        }
    }

    /// <summary>The first retail population, in the game's list order, that draws under <paramref name="set"/>; none when no retail room does.</summary>
    internal static RoomEnemyPopulationDefinition? RetailDrawing(RoomEnemyGraphicsSetDefinition set) =>
        SuperMetroid.Core.World.RoomStates.RoomEnemyPopulationDefinitions.Definitions.FirstOrDefault(population =>
            RoomEnemyGraphicsSetLists.PopulationSets.TryGetValue(population.Name, out RoomEnemyGraphicsSetDefinition? drawn) && drawn == set);

    /// <summary>
    /// The graphics set <paramref name="population"/> draws under: the set a fixture recorded for it, the set a retail room
    /// placing it loads, or the session start's for none, which a room that has placed no enemies draws under.
    /// </summary>
    /// <exception cref="InvalidDataException">The population is a fixture's with no set recorded, and no retail population.</exception>
    internal static RoomEnemyGraphicsSetDefinition SetOf(RoomEnemyPopulationDefinition? population)
    {
        if (population is null)
            return RoomEnemyGraphicsSetDefinitions.SessionStart;
        if (sets.TryGetValue(population, out RoomEnemyGraphicsSetDefinition? recorded))
            return recorded;
        return RoomEnemyGraphicsSetLists.PopulationSets.TryGetValue(population.Name, out RoomEnemyGraphicsSetDefinition? retail)
            && SuperMetroid.Core.World.RoomStates.RoomEnemyPopulationDefinitions.Definitions.Contains(population) ? retail
            : throw new InvalidDataException($"Population {population.Name} is no retail population, and no fixture recorded the graphics set it draws under.");
    }
}
