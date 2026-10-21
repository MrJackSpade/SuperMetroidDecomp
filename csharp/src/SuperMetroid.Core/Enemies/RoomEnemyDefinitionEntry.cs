
namespace SuperMetroid.Core.Enemies;

/// <summary>A room enemy definition and its identity.</summary>
/// <param name="Id">The id.</param>
/// <param name="Definition">The definition.</param>
public readonly record struct RoomEnemyDefinitionEntry(EnemyDefinitionId Id, RoomEnemyDefinition Definition);
