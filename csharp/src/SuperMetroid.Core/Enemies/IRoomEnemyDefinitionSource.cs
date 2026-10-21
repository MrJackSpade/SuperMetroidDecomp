using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Where a room's enemy headers and Mother Brain's falling-tube placements come from.
/// Gameplay binds <see cref="CompiledRoomEnemyDefinitions"/>; a constructed verifier room supplies its own records.
/// </summary>
public interface IRoomEnemyDefinitionSource
{
    /// <summary>The enemy header named by <paramref name="pointer"/>.</summary>
    RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId definitionId);

    /// <summary>The population record Mother Brain's cutscene spawns for falling tube <paramref name="piece"/>.</summary>
    RoomEnemyPopulationRecord ReadFallingTubePopulation(MotherBrainTubePiece piece);
}
