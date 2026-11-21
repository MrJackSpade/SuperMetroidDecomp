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
    /// <summary>The restored door frame: the door's transition column, four door blocks down from its top block.</summary>
    private static readonly RoomBlockShape RestoredDoor = RoomBlockShapes.Column(4, RoomCollisionType.DoorBlock);

    /// <summary>The fight's door: the door's transition column, and the shootable cap over its extensions beside it.</summary>
    private static readonly RoomBlockShape FightDoor = RoomBlockShapes.Joined(RestoredDoor, RoomBlockShapes.Moved(RoomBlockShapes.Line(down: true,
        [RoomCollisionType.ShootableBlock, .. DefinitionRun.Repeated(RoomCollisionType.VerticalExtension, 3)]),
        new(1, 0)));

    /// <summary>The block shape each of Phantoon's door frames draws.</summary>
    internal static readonly Dictionary<PhantoonDoorDraw, RoomBlockShape> Shapes = new()
    {
        [PhantoonDoorDraw.FightDoor] = FightDoor,
        [PhantoonDoorDraw.RestoredDoor] = RestoredDoor,
    };
}
