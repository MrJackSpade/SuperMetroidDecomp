using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Core.Enemies;

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
    public RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId pointer) =>
        RoomEnemyAuxiliaryDefinitionCatalog.TryGet(pointer, out RoomEnemyDefinition auxiliary)
            ? auxiliary
            : RoomEnemyDefinitionCatalog.Get(pointer);

    /// <inheritdoc />
    public RoomEnemyPopulationRecord ReadFallingTubePopulation(MotherBrainTubePiece piece) =>
        MotherBrainFallingTubePopulationDefinitions.Get(piece);
}
