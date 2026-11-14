using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Where a room's enemy definitions and Mother Brain's falling-tube placements come from.
/// Gameplay binds <see cref="CompiledRoomEnemyDefinitions"/>; a constructed verifier room supplies its own entries.
/// </summary>
public interface IRoomEnemyDefinitionSource
{
    /// <summary>The enemy definition named by <paramref name="definitionId"/>.</summary>
    RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId definitionId);

    /// <summary>The population entry Mother Brain's cutscene spawns for falling tube <paramref name="piece"/>.</summary>
    RoomEnemyPlacement ReadFallingTubePopulation(MotherBrainTubePiece piece);
}
