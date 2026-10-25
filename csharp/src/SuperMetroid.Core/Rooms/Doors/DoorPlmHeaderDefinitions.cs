using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The PLM headers that install coloured and grey doors, and the door each installs (#1414: named by member, replacing
/// the header range and six-byte stride the cartridge's header words implied). A header absent here installs no
/// coloured or grey door.
/// </summary>
internal static class DoorPlmHeaderDefinitions
{
    /// <summary>The colour and facing of the door each coloured-door header installs.</summary>
    internal static IReadOnlyDictionary<PlmHeaderId, ColoredDoorVariant> ColoredDoors { get; } = new Dictionary<PlmHeaderId, ColoredDoorVariant>
    {
        [PlmHeaderId.YellowDoorFacingLeft] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left),
        [PlmHeaderId.YellowDoorFacingRight] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right),
        [PlmHeaderId.YellowDoorFacingUp] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up),
        [PlmHeaderId.YellowDoorFacingDown] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down),
        [PlmHeaderId.GreenDoorFacingLeft] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Left),
        [PlmHeaderId.GreenDoorFacingRight] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Right),
        [PlmHeaderId.GreenDoorFacingUp] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Up),
        [PlmHeaderId.GreenDoorFacingDown] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Down),
        [PlmHeaderId.RedDoorFacingLeft] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Left),
        [PlmHeaderId.RedDoorFacingRight] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Right),
        [PlmHeaderId.RedDoorFacingUp] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Up),
        [PlmHeaderId.RedDoorFacingDown] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Down),
    };

    /// <summary>
    /// The facing of the door each grey-door header installs; Bomb Torizo's grey door faces right like the generic
    /// right-facing one.
    /// </summary>
    internal static IReadOnlyDictionary<PlmHeaderId, ColoredDoorOrientation> GreyDoors { get; } = new Dictionary<PlmHeaderId, ColoredDoorOrientation>
    {
        [PlmHeaderId.BombTorizoGreyDoor] = ColoredDoorOrientation.Right,
        [PlmHeaderId.GreyDoorFacingRight] = ColoredDoorOrientation.Right,
        [PlmHeaderId.GreyDoorFacingLeft] = ColoredDoorOrientation.Left,
        [PlmHeaderId.GreyDoorFacingUp] = ColoredDoorOrientation.Up,
        [PlmHeaderId.GreyDoorFacingDown] = ColoredDoorOrientation.Down,
    };
}
