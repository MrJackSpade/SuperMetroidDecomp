using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The block shape of each of Phantoon's door frames (#1678): the door's column of four transition blocks down from the
/// door's top block and, for the fight's door, the column beside it, where the grey door's closed cap sits, collides as
/// that cap does: a shootable block over three vertical extensions. What each frame shows is its installed picture.
/// </summary>
internal static class PhantoonDoorObjectDrawDefinitions
{
    /// <summary>The fight's door: the door's transition column, and the shootable cap over its extensions beside it.</summary>
    private static readonly RoomBlockShape FightDoor = new(
    [
        new(new(0, 0), RoomCollisionType.DoorBlock), new(new(0, 1), RoomCollisionType.DoorBlock),
        new(new(0, 2), RoomCollisionType.DoorBlock), new(new(0, 3), RoomCollisionType.DoorBlock),
        new(new(1, 0), RoomCollisionType.ShootableBlock), new(new(1, 1), RoomCollisionType.VerticalExtension),
        new(new(1, 2), RoomCollisionType.VerticalExtension), new(new(1, 3), RoomCollisionType.VerticalExtension),
    ]);

    /// <summary>The restored door frame: the door's transition column.</summary>
    private static readonly RoomBlockShape RestoredDoor = new(
    [
        new(new(0, 0), RoomCollisionType.DoorBlock), new(new(0, 1), RoomCollisionType.DoorBlock),
        new(new(0, 2), RoomCollisionType.DoorBlock), new(new(0, 3), RoomCollisionType.DoorBlock),
    ]);

    /// <summary>The block shape each of Phantoon's door frames draws.</summary>
    internal static readonly Dictionary<PhantoonDoorDraw, RoomBlockShape> Shapes = new()
    {
        [PhantoonDoorDraw.FightDoor] = FightDoor,
        [PhantoonDoorDraw.RestoredDoor] = RestoredDoor,
    };
}
