using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How the four cells of each blue-door cap draw collide (#1994). What each frame shows is its installed picture (#1557); the
/// room-object sequences still own sound, frame timing, and deletion.
/// </summary>
internal static class BlueDoorObjectDrawDefinitions
{
    /// <summary>
    /// How the four blocks of each of the twenty blue-cap draws collide, from the cap's first block down its column or along its row. The solid closed cap makes only its
    /// first cell solid; the closed cap carries the shootable and extension types; nearly open vertical caps clear their
    /// middle cells, and horizontal caps stay solid.
    /// </summary>
    /// <remarks>
    /// A closed cap's three cells after its first continue the first (<see cref="ContinuedBlocks"/>) rather than each
    /// colliding and reacting as its own block (#2572): a shot, bomb or grapple striking any of them reacts at the cap's
    /// first block, where the blue cap's opening is spawned and draws its four cells, and the cells follow the first block as it stands. Giving each cell the cap's behaviour
    /// directly would move that reaction to the cell struck, opening a cap shifted down its column or along its row, which changes the game.
    /// </remarks>
    internal static IReadOnlyDictionary<BlueDoorCaps.Draw, IReadOnlyList<RoomCollisionType>> Collisions { get; } =
        new Dictionary<BlueDoorCaps.Draw, IReadOnlyList<RoomCollisionType>>
        {
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.ClosedSolid)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.ClosedSolid)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
                RoomCollisionType.VerticalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.ClosedSolid)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.ClosedSolid)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
                RoomCollisionType.HorizontalExtension,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.Ajar)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.HalfOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ],
        };
}
