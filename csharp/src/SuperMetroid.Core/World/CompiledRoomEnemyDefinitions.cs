using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>The compiled retail enemy headers, populations, graphics sets and falling-tube placements.</summary>
public sealed class CompiledRoomEnemyDefinitions : IRoomEnemyDefinitionSource
{
    /// <summary>The single compiled source; it holds no state.</summary>
    public static CompiledRoomEnemyDefinitions Instance { get; } = new();

    /// <summary>Restricts construction to <see cref="Instance"/>.</summary>
    private CompiledRoomEnemyDefinitions()
    {
    }

    /// <inheritdoc />
    public RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId definitionId) =>
        RoomEnemyAuxiliaryDefinitionCatalog.TryGet(definitionId, out RoomEnemyDefinition auxiliary)
            ? auxiliary
            : RoomEnemyDefinitionCatalog.Get(definitionId);

    /// <inheritdoc />
    public RoomEnemyPopulationRecord ReadFallingTubePopulation(MotherBrainTubePiece piece) =>
        MotherBrainFallingTubes.Get(piece);
}
