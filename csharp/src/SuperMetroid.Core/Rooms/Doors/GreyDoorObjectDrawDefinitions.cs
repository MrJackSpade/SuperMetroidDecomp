using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How the four cells of the grey door caps and the four shared clear-cap frames collide. What each frame shows is its
/// installed picture (#1557). Bomb Torizo's special resident door uses the ordinary right-facing frames.
/// </summary>
internal static class GreyDoorObjectDrawDefinitions
{
    /// <summary>
    /// How the four cells of each of the twenty grey-cap draws collide, in authored run order. The clear frame has no
    /// collision; frame zero carries the shootable and extension types; opening frames are solid except the middle cells
    /// of final left and down frames, which are air.
    /// </summary>
    internal static IReadOnlyDictionary<GreyDoorCaps.Draw, IReadOnlyList<RoomCollisionType>> Collisions { get; } =
        new Dictionary<GreyDoorCaps.Draw, IReadOnlyList<RoomCollisionType>>
        {
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, null)] =
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, null)] =
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, null)] =
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, null)] =
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
        };
}
