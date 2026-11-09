using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How the yellow, green and red door caps' cells collide. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class ColoredDoorObjectDrawDefinitions
{
    /// <summary>
    /// How the four cells of each colour's cap frames collide, in authored run order. Closed caps keep the shootable and
    /// extension collision types. Final middle cells clear for every left cap, green and red right caps and yellow down
    /// caps; the other final caps stay solid.
    /// </summary>
    internal static IReadOnlyDictionary<ColoredDoorCaps.Draw, IReadOnlyList<RoomCollisionType>> Collisions { get; } =
        new Dictionary<ColoredDoorCaps.Draw, IReadOnlyList<RoomCollisionType>>
        {
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 0)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 1)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 2)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, 3)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
        };
}
