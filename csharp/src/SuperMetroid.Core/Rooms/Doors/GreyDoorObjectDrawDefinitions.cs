using SuperMetroid.Core.Rooms.LevelData;

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
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.Cleared)] = DoorCapCollisionDefinitions.ClearedCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.Closed)] = DoorCapCollisionDefinitions.ShootableColumnCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.Ajar)] = DoorCapCollisionDefinitions.SolidCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.HalfOpen)] = DoorCapCollisionDefinitions.SolidCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Left, DoorCapLook.NearlyOpen)] = DoorCapCollisionDefinitions.MiddleClearCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.Cleared)] = DoorCapCollisionDefinitions.ClearedCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.Closed)] = DoorCapCollisionDefinitions.ShootableColumnCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.Ajar)] = DoorCapCollisionDefinitions.SolidCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.HalfOpen)] = DoorCapCollisionDefinitions.SolidCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Right, DoorCapLook.NearlyOpen)] = DoorCapCollisionDefinitions.SolidCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.Cleared)] = DoorCapCollisionDefinitions.ClearedCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.Closed)] = DoorCapCollisionDefinitions.ShootableRowCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.Ajar)] = DoorCapCollisionDefinitions.SolidCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.HalfOpen)] = DoorCapCollisionDefinitions.SolidCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Up, DoorCapLook.NearlyOpen)] = DoorCapCollisionDefinitions.SolidCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.Cleared)] = DoorCapCollisionDefinitions.ClearedCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.Closed)] = DoorCapCollisionDefinitions.ShootableRowCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.Ajar)] = DoorCapCollisionDefinitions.SolidCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.HalfOpen)] = DoorCapCollisionDefinitions.SolidCap,
            [new GreyDoorCaps.Draw(ColoredDoorOrientation.Down, DoorCapLook.NearlyOpen)] = DoorCapCollisionDefinitions.MiddleClearCap,
        };
}
