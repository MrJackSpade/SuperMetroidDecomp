using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.DefinitionRun;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How the four cells of the grey door caps and the four shared clear-cap frames collide. What each frame shows is its
/// installed picture (#1557). Bomb Torizo's special resident door uses the ordinary right-facing frames.
/// </summary>
internal static class GreyDoorObjectDrawDefinitions
{
    /// <summary>
    /// How the four blocks of each of the twenty grey-cap draws collide, from the cap's first block on. The cleared opening has no
    /// collision; the closed cap carries the shootable and extension types; the sliding caps are solid except the middle
    /// cells of nearly open left and down caps, which are air.
    /// </summary>
    internal static IReadOnlyDictionary<GreyDoorCaps.Draw, IReadOnlyList<RoomCollisionType>> Collisions { get; } =
        new Dictionary<GreyDoorCaps.Draw, IReadOnlyList<RoomCollisionType>>
        {
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.Cleared)] =
            [
                .. Repeated(RoomCollisionType.Air, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                .. Repeated(RoomCollisionType.VerticalExtension, 3),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.Ajar)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.HalfOpen)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.Cleared)] =
            [
                .. Repeated(RoomCollisionType.Air, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                .. Repeated(RoomCollisionType.VerticalExtension, 3),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.Ajar)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.HalfOpen)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.NearlyOpen)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.Cleared)] =
            [
                .. Repeated(RoomCollisionType.Air, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                .. Repeated(RoomCollisionType.HorizontalExtension, 3),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.Ajar)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.HalfOpen)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.NearlyOpen)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.Cleared)] =
            [
                .. Repeated(RoomCollisionType.Air, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.Closed)] =
            [
                RoomCollisionType.ShootableBlock,
                .. Repeated(RoomCollisionType.HorizontalExtension, 3),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.Ajar)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.HalfOpen)] =
            [
                .. Repeated(RoomCollisionType.SolidBlock, 4),
            ],
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.NearlyOpen)] =
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.SolidBlock,
            ],
        };
}
