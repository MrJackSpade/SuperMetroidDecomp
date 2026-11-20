using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Places the elevator room's blocked door (#2426): built only for a room that shows a turning floor, whose elevator
/// platform the door cycles. The door's animations, look, escape and quake are those of the room's owner of the Ceres
/// doors and walls.
/// </summary>
/// <param name="doors">The room's owner of the Ceres doors and walls.</param>
/// <param name="floor">The room's turning floor.</param>
[RoomLifetime]
internal sealed class CeresElevatorDoorFactory(CeresDoorBehavior doors, RoomFloor floor) : IEnemyEntityFactory
{
    /// <summary>The room's owner of the Ceres doors and walls.</summary>
    private readonly CeresDoorBehavior _doors = Ensure.NotNull(doors);

    /// <summary>The room's turning floor, whose elevator platform the door cycles.</summary>
    private readonly RoomFloor _floor = Ensure.NotNull(floor);

    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.CeresElevatorDoor], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="place">The place, holding the new identity and the entry it is spawned from.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Of(new CeresElevatorDoorEntity(_doors, Ensure.NotNull(place), common, _floor));
}
