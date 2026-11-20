using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The factory for Lower Norfair Ridley, Ceres Ridley and the breakup explosion actor. Each Ridley body runs its own
/// encounter over the parts of his body both share (#2229).
/// </summary>
/// <param name="norfair">Lower Norfair Ridley's encounter behaviour.</param>
/// <param name="ceres">Ceres Ridley's encounter behaviour.</param>
/// <param name="escape">The station's escape progress, which ends Ceres Ridley's touch and drawing.</param>
[RoomLifetime]
internal sealed class RidleyEntityFactory(NorfairRidleyBehavior norfair, CeresRidleyBehavior ceres, ICeresEscapeStatus escape) : IEnemySpawnFactory<EnemySpawnSettings>
{
    /// <summary>Lower Norfair Ridley's encounter behaviour.</summary>
    private NorfairRidleyBehavior Norfair { get; } = norfair;

    /// <summary>Ceres Ridley's encounter behaviour.</summary>
    private CeresRidleyBehavior Ceres { get; } = ceres;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Ridley, EnemyDefinitionId.RidleyCeres];

    /// <summary>
    /// Lower Norfair Ridley and Ceres Ridley, built the same way wherever they are set up; the pieces of the breakup
    /// explosion are only spawned joining the breakup, by Ridley's death (<see cref="NorfairRidleyDeath"/>).
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void Register(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<EnemySpawnSettings>(Definitions, Create);
        registrar.Joins([EnemyDefinitionId.RidleyBreakupPiece]);
    }

    /// <summary>Lower Norfair Ridley or Ceres Ridley (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Ridley definition.</exception>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(occupant).DefinitionId;
        if (definition == EnemyDefinitionId.Ridley)
            return EnemyOccupant.Running(new NorfairRidleyEntity(Norfair, occupant, place, common));
        if (definition == EnemyDefinitionId.RidleyCeres)
            return EnemyOccupant.Running(new CeresRidleyEntity(Ceres, escape, occupant, place, common));
        throw new InvalidOperationException($"The Ridley factory creates no {occupant.DefinitionId}.");
    }
}
