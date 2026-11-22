using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The factory for Lower Norfair Ridley and the breakup explosion actor. Each Ridley body runs its own encounter over the
/// parts of his body both share (#2229); Ceres Ridley has his own factory, which only a room with a warning picture sheet
/// builds (<see cref="CeresRidleyEntityFactory"/>).
/// </summary>
/// <param name="norfair">Lower Norfair Ridley's encounter behaviour.</param>
[RoomLifetime]
internal sealed partial class RidleyEntityFactory(NorfairRidleyBehavior norfair) : IEnemyEntityFactory
{
    /// <summary>Lower Norfair Ridley's encounter behaviour.</summary>
    internal NorfairRidleyBehavior Norfair { get; } = norfair;

    /// <summary>
    /// Lower Norfair Ridley, built the same way wherever he is set up; the pieces of the breakup explosion are only spawned
    /// joining the breakup, by Ridley's death (<see cref="NorfairRidleyDeath"/>).
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<EnemySpawnSettings>([EnemyDefinitionId.Ridley], CreateRidley);
        registrar.Joins([EnemyDefinitionId.RidleyBreakupPiece]);
    }

    /// <summary>Lower Norfair Ridley.</summary>
    private EnemyOccupant CreateRidley(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Running(new NorfairRidleyEntity(Norfair, occupant, place, common));
}
