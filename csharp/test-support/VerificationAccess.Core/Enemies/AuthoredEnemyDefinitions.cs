using System.Runtime.CompilerServices;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy definitions a check's room reads (#2554): the room's own source, except for each species a check bound on a
/// place it authored with a definition of its own (a stand-in answering a check's reactions, a species with a check's
/// health), which the room's population load then builds with that definition, as a room whose definitions held it would.
/// </summary>
internal sealed partial class AuthoredEnemyDefinitions(IRoomEnemyDefinitionSource room) : IRoomEnemyDefinitionSource
{
    /// <summary>Each fixture room's definitions, by its enemies.</summary>
    private static readonly ConditionalWeakTable<RoomEnemySystem, AuthoredEnemyDefinitions> rooms = new();

    /// <summary>The definitions the check authored, by species.</summary>
    private readonly Dictionary<EnemyDefinitionId, RoomEnemyDefinition> authored = [];

    /// <summary>Records <paramref name="definitions"/> as the definitions <paramref name="enemies"/>' room reads.</summary>
    internal static void Register(RoomEnemySystem enemies, AuthoredEnemyDefinitions definitions) =>
        rooms.AddOrUpdate(Ensure.NotNull(enemies), Ensure.NotNull(definitions));

    /// <summary>The definitions <paramref name="enemies"/>' room reads.</summary>
    /// <exception cref="InvalidOperationException">The enemies were built outside the enemy system fixtures.</exception>
    internal static AuthoredEnemyDefinitions Of(RoomEnemySystem enemies) =>
        rooms.TryGetValue(Ensure.NotNull(enemies), out AuthoredEnemyDefinitions? definitions)
            ? definitions
            : throw new InvalidOperationException("The check's enemies were built outside the enemy system fixtures, whose rooms read authored definitions.");

    /// <summary>
    /// Has the room's load build <paramref name="species"/> with <paramref name="definition"/>, the definition the check
    /// bound on a place it authored.
    /// </summary>
    /// <exception cref="InvalidOperationException">The check bound another definition of the species on an earlier place.</exception>
    internal void Author(EnemyDefinitionId species, RoomEnemyDefinition definition)
    {
        if (authored.TryGetValue(species, out RoomEnemyDefinition earlier) && !Equals(earlier, definition))
            throw new InvalidOperationException($"The check authored two definitions of {species} in one room.");
        authored[species] = definition;
    }

    /// <inheritdoc/>
    public RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId definitionId) =>
        authored.TryGetValue(definitionId, out RoomEnemyDefinition definition) ? definition : room.ReadEnemyDefinition(definitionId);

    /// <inheritdoc/>
    public RoomEnemyPlacement ReadFallingTubePopulation(MotherBrainTubePiece piece) => room.ReadFallingTubePopulation(piece);
}
