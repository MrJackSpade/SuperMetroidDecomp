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
internal sealed class RidleyEntityFactory(NorfairRidleyBehavior norfair, CeresRidleyBehavior ceres, ICeresEscapeStatus escape) : IEnemyEntityFactory
{
    /// <summary>Lower Norfair Ridley's encounter behaviour.</summary>
    private NorfairRidleyBehavior Norfair { get; } = norfair;

    /// <summary>Ceres Ridley's encounter behaviour.</summary>
    private CeresRidleyBehavior Ceres { get; } = ceres;

    /// <summary>
    /// Lower Norfair Ridley and Ceres Ridley, built the same way wherever they are set up; the pieces of the breakup
    /// explosion are only spawned joining the breakup, by Ridley's death (<see cref="NorfairRidleyDeath"/>).
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<EnemySpawnSettings>([EnemyDefinitionId.Ridley], (occupant, _, place, common) =>
            EnemyOccupant.Running(new NorfairRidleyEntity(Norfair, occupant, place, common)));
        registrar.Spawns<EnemySpawnSettings>([EnemyDefinitionId.RidleyCeres], (occupant, _, place, common) =>
            EnemyOccupant.Running(new CeresRidleyEntity(Ceres, escape, occupant, place, common)));
        registrar.Joins([EnemyDefinitionId.RidleyBreakupPiece]);
    }

}
