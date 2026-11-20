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
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.ClosedSolid)] = DoorCapCollisionDefinitions.SolidColumnCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.Closed)] = DoorCapCollisionDefinitions.ShootableColumnCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.Ajar)] = DoorCapCollisionDefinitions.SolidCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.HalfOpen)] = DoorCapCollisionDefinitions.SolidCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.NearlyOpen)] = DoorCapCollisionDefinitions.MiddleClearCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.ClosedSolid)] = DoorCapCollisionDefinitions.SolidColumnCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.Closed)] = DoorCapCollisionDefinitions.ShootableColumnCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.Ajar)] = DoorCapCollisionDefinitions.SolidCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.HalfOpen)] = DoorCapCollisionDefinitions.SolidCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.NearlyOpen)] = DoorCapCollisionDefinitions.MiddleClearCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.ClosedSolid)] = DoorCapCollisionDefinitions.SolidRowCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.Closed)] = DoorCapCollisionDefinitions.ShootableRowCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.Ajar)] = DoorCapCollisionDefinitions.SolidCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.HalfOpen)] = DoorCapCollisionDefinitions.SolidCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.NearlyOpen)] = DoorCapCollisionDefinitions.SolidCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.ClosedSolid)] = DoorCapCollisionDefinitions.SolidRowCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.Closed)] = DoorCapCollisionDefinitions.ShootableRowCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.Ajar)] = DoorCapCollisionDefinitions.SolidCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.HalfOpen)] = DoorCapCollisionDefinitions.SolidCap,
            [new BlueDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.NearlyOpen)] = DoorCapCollisionDefinitions.SolidCap,
        };
}
