using System.Collections.Frozen;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Looks up the compiled retail enemy definitions by identity: the definitions room states reference, checked when their
/// catalog is indexed, and the auxiliary definitions runtime-created actors use.
/// </summary>
public static class RoomEnemyDefinitionLookup
{
    /// <summary>Indexes the compiled definitions by identity, checking their count and that each identity is compiled once.</summary>
    /// <exception cref="InvalidDataException">The count changed or an identity repeats.</exception>
    internal static FrozenDictionary<EnemyDefinitionId, RoomEnemyDefinition> Index(RoomEnemyDefinitionEntry[] definitions, int count)
    {
        if (definitions.Length != count)
            throw new InvalidDataException(
                $"Expected {count} compiled enemy definitions, found {definitions.Length}.");
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

    /// <summary>
    /// The compiled definition for <paramref name="definitionId"/>: an auxiliary definition the retail population needs
    /// beyond the referenced definitions answers first, and every other identity is a referenced definition.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="definitionId"/> is neither auxiliary nor referenced.</exception>
    internal static RoomEnemyDefinition Read(EnemyDefinitionId definitionId) =>
        RoomEnemyAuxiliaryDefinitionCatalog.TryGet(definitionId, out RoomEnemyDefinition auxiliary)
            ? auxiliary
            : Get(definitionId);
}
