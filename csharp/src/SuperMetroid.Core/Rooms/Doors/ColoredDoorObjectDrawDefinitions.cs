using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How the yellow, green and red door caps' cells collide. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class ColoredDoorObjectDrawDefinitions
{
    /// <summary>
    /// How the four blocks of each colour's cap frames collide, from the cap's first block down its column or along its row. Closed caps keep the shootable and
    /// extension collision types. Final middle cells clear for every left cap, green and red right caps and yellow down
    /// caps; the other final caps stay solid.
    /// </summary>
    /// <remarks>
    /// A closed cap's three cells after its first continue the first (<see cref="ContinuedBlocks"/>) rather than each
    /// colliding and reacting as its own block (#2572): a shot, bomb or grapple striking any of them reacts at the cap's
    /// first block, where the cap's resident object hears the hit and counts it toward opening, and the cells follow the first block as it stands. Giving each cell the cap's behaviour
    /// directly would move that reaction to the cell struck, where no cap's object stands to hear it, which changes the game.
    /// </remarks>
    internal static IReadOnlyDictionary<ColoredDoorCaps.Draw, IReadOnlyList<RoomCollisionType>> Collisions { get; } =
        new Dictionary<ColoredDoorCaps.Draw, IReadOnlyList<RoomCollisionType>>
        {
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Left, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Right, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Up, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Green, ColoredDoorOrientation.Down, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Left, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Right, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Up, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new ColoredDoorCaps.Draw(ColoredDoorColor.Red, ColoredDoorOrientation.Down, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
        };
}
