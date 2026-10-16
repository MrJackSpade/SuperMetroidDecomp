using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Where a room's enemy headers, populations, graphics sets and Mother Brain's falling-tube placements come from.
/// Gameplay binds <see cref="CompiledRoomEnemyDefinitions"/>; a constructed verifier room supplies its own records.
/// </summary>
public interface IRoomEnemyDefinitionSource
{
    /// <summary>The enemy header named by <paramref name="pointer"/>.</summary>
    RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId pointer);

    /// <summary>The room population at bank-$A1 <paramref name="pointer"/>.</summary>
    RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer);

    /// <summary>The room enemy graphics set at bank-$B4 <paramref name="pointer"/>.</summary>
    RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer);

    /// <summary>The population record Mother Brain's cutscene spawns for falling tube <paramref name="piece"/>.</summary>
    RoomEnemyPopulationRecord ReadFallingTubePopulation(MotherBrainTubePiece piece);
}
