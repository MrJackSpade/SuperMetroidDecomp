using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>The rooms' enemy definitions, populations and falling-tube placements.</summary>
public sealed class CompiledRoomEnemyDefinitions : IRoomEnemyDefinitionSource
{
    /// <summary>The single source; it holds no state.</summary>
    public static CompiledRoomEnemyDefinitions Instance { get; } = new();

    /// <summary>Restricts construction to <see cref="Instance"/>.</summary>
    private CompiledRoomEnemyDefinitions()
    {
    }

    /// <inheritdoc />
    public RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId definitionId) => RoomEnemyDefinitionLookup.Read(definitionId);

    /// <inheritdoc />
    public RoomEnemyPlacement ReadFallingTubePopulation(MotherBrainTubePiece piece) =>
        MotherBrainFallingTubes.Get(piece);
}
