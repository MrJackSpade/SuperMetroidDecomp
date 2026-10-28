using System.Collections.Frozen;

namespace SuperMetroid.Core.Enemies;

/// <summary>Finds the retail enemy definitions room states reference, and checks their compiled catalog.</summary>
public static class RoomEnemyHeaders
{
    /// <summary>Indexes the compiled definitions by identity, checking their count and that each identity is compiled once.</summary>
    /// <exception cref="InvalidDataException">The count changed or an identity repeats.</exception>
    internal static FrozenDictionary<EnemyDefinitionId, RoomEnemyDefinition> Index(RoomEnemyDefinitionEntry[] definitions, int count)
    {
        if (definitions.Length != count)
            throw new InvalidDataException(
                $"Expected {count} compiled enemy headers, found {definitions.Length}.");
        var byId = new Dictionary<EnemyDefinitionId, RoomEnemyDefinition>(definitions.Length);
        foreach (RoomEnemyDefinitionEntry entry in definitions)
        {
            if (!byId.TryAdd(entry.Id, entry.Definition))
                throw new InvalidDataException($"Compiled enemy definitions repeat {entry.Id}.");
        }
        return byId.ToFrozenDictionary();
    }

    /// <summary>Gets an application-owned retail definition, rejecting an unreferenced identity.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="definitionId"/> is not referenced by a room state.</exception>
    public static RoomEnemyDefinition Get(EnemyDefinitionId definitionId) =>
        RoomEnemyDefinitionCatalog.ById.TryGetValue(definitionId, out RoomEnemyDefinition definition)
            ? definition
            : throw new ArgumentOutOfRangeException(nameof(definitionId), definitionId,
                "Not a referenced retail enemy definition.");
}
