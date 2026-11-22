using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The factory for Mother Brain's body (#2554), which only a room whose tileset has the warning picture sheet builds: her
/// death types Zebes's warning on the room's background warning, which her body receives as it is placed. Her head joins
/// the encounter her body offers it, and the tubes of her fake death are only spawned joining her encounter. A room without
/// the sheet places neither.
/// </summary>
/// <param name="species">Mother Brain's species owner.</param>
/// <param name="zebesWarning">The room's background warning.</param>
[RoomLifetime]
internal sealed partial class MotherBrainBodyFactory(MotherBrainBehavior species, RoomWarning zebesWarning) : IEnemyEntityFactory
{
    /// <summary>The definitions this factory places or joins, which a room without the warning picture sheet cannot place.</summary>
    internal static IReadOnlyList<EnemyDefinitionId> Placed { get; } =
        [EnemyDefinitionId.MotherBrainBody, EnemyDefinitionId.MotherBrainHead, EnemyDefinitionId.MotherBrainTubes];

    /// <summary>Her body, built the same way wherever it is set up; her head and her fake death's tubes join her encounter.</summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<EnemySpawnSettings>([EnemyDefinitionId.MotherBrainBody], (occupant, _, place, common) =>
            species.CreateBody(occupant, place, common, zebesWarning));
        registrar.Joins([EnemyDefinitionId.MotherBrainHead, EnemyDefinitionId.MotherBrainTubes]);
    }
}
