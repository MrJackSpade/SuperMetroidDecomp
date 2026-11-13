using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How the four cells of each blue-door cap frame collide. What each frame shows is its installed picture (#1557); the
/// room-object programs still own sound, frame timing, and deletion.
/// </summary>
internal static class BlueDoorObjectDrawDefinitions
{
    /// <summary>The opening frame whose picture the physical closed-cap alias shows, with only its first cell solid.</summary>
    internal const int AliasPictureFrame = 0;

    /// <summary>
    /// How the four cells of each of the twenty blue-cap draws collide, in authored run order. The alias makes only its
    /// first cell solid; frame zero carries the shootable and extension types; final vertical frames clear their middle
    /// cells, and horizontal frames stay solid.
    /// </summary>
    internal static IReadOnlyDictionary<BlueDoorCaps.Draw, IReadOnlyList<RoomCollisionType>> Collisions { get; } =
        new Dictionary<BlueDoorCaps.Draw, IReadOnlyList<RoomCollisionType>>
        {
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, null)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, null)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, null)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, null)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
        };
}
