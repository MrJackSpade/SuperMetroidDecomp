using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The room-object definitions that install coloured and grey doors, and the door each installs. A definition absent
/// here installs no coloured or grey door.
/// </summary>
internal static class DoorObjectKindDefinitions
{
    /// <summary>The colour and facing of the door each coloured-door definition installs.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, ColoredDoorVariant> ColoredDoors { get; } = new Dictionary<RoomObjectKind, ColoredDoorVariant>
    {
        [RoomObjectKind.YellowDoorFacingLeft] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left),
        [RoomObjectKind.YellowDoorFacingRight] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right),
        [RoomObjectKind.YellowDoorFacingUp] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up),
        [RoomObjectKind.YellowDoorFacingDown] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down),
        [RoomObjectKind.GreenDoorFacingLeft] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Left),
        [RoomObjectKind.GreenDoorFacingRight] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Right),
        [RoomObjectKind.GreenDoorFacingUp] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Up),
        [RoomObjectKind.GreenDoorFacingDown] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Down),
        [RoomObjectKind.RedDoorFacingLeft] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Left),
        [RoomObjectKind.RedDoorFacingRight] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Right),
    };

    /// <summary>
    /// The grey cap each grey-door definition installs: its facing, and whether it is Bomb Torizo's door, which faces right
    /// like the generic right-facing one and whose closing waits for Bombs.
    /// </summary>
    internal static IReadOnlyDictionary<RoomObjectKind, GreyDoorVariant> GreyDoors { get; } = new Dictionary<RoomObjectKind, GreyDoorVariant>
    {
        [RoomObjectKind.BombTorizoGreyDoor] = new(ColoredDoorOrientation.Right, GreyDoorClosing.AfterBombs),
        [RoomObjectKind.GreyDoorFacingRight] = new(ColoredDoorOrientation.Right, GreyDoorClosing.AtOnce),
        [RoomObjectKind.GreyDoorFacingLeft] = new(ColoredDoorOrientation.Left, GreyDoorClosing.AtOnce),
        [RoomObjectKind.GreyDoorFacingUp] = new(ColoredDoorOrientation.Up, GreyDoorClosing.AtOnce),
        [RoomObjectKind.GreyDoorFacingDown] = new(ColoredDoorOrientation.Down, GreyDoorClosing.AtOnce),
    };
}
