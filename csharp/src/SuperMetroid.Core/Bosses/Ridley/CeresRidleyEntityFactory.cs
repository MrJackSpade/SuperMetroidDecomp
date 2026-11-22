using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The factory for Ceres Ridley (#2554), which only a room whose tileset has the warning picture sheet builds: the
/// self-destruct his getaway starts writes the room's foreground warning, which he receives as he is placed. A room without
/// the sheet places no Ceres Ridley.
/// </summary>
/// <param name="ceres">Ceres Ridley's encounter behaviour.</param>
/// <param name="escape">The station's escape progress, which ends Ceres Ridley's touch and drawing.</param>
/// <param name="warning">The room's foreground warning.</param>
[RoomLifetime]
internal sealed partial class CeresRidleyEntityFactory(CeresRidleyBehavior ceres, ICeresEscapeStatus escape, RoomWarning warning) : IEnemyEntityFactory
{
    /// <summary>Ceres Ridley, built the same way wherever he is set up.</summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar) =>
        Ensure.NotNull(registrar).Spawns<EnemySpawnSettings>([EnemyDefinitionId.RidleyCeres], CreateRidley);

    /// <summary>Ceres Ridley, who receives the room's foreground warning as he is placed.</summary>
    private EnemyOccupant CreateRidley(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Running(new CeresRidleyEntity(ceres, escape, warning, occupant, place, common));
}
