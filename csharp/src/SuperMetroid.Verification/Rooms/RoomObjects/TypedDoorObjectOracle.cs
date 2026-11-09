using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// Relates the typed door objects (#1554) to the native bank-$84 lists the cartridge's headers name: the list a closer
/// places, and the list a door object just placed or just asked to close stands for.
/// </summary>
internal static class TypedDoorObjectOracle
{
    /// <summary>The native list the closer <paramref name="definition"/> places starts at.</summary>
    internal static ushort ClosingListOf(DoorClosingObjectDefinition definition) =>
        definition.BlueCapFacing is { } facing
            ? DoorObjectNativeListAddresses.Blue(facing).Closing
            : DoorObjectNativeListAddresses.EscapeGateClosing;

    /// <summary>
    /// The native list a door object that has not yet updated stands for: the blue cap's opening or closing, the escape
    /// gate shut or closing, or a coloured or grey cap's first look or closing.
    /// </summary>
    /// <exception cref="InvalidOperationException">The object is no door, or has already moved on.</exception>
    internal static ushort StartListOf(RoomObjectBehaviour? behaviour) => behaviour switch
    {
        BlueDoorCapObject { Phase: BlueDoorCapPhase.Opening } blue => DoorObjectNativeListAddresses.Blue(blue.Orientation).Opening,
        BlueDoorCapObject { Phase: BlueDoorCapPhase.Closing } blue => DoorObjectNativeListAddresses.Blue(blue.Orientation).Closing,
        MotherBrainEscapeGateObject gate when gate.Change == MotherBrainEscapeGateDefinitions.Shut => DoorObjectNativeListAddresses.EscapeGateShut,
        MotherBrainEscapeGateObject gate when gate.Change == MotherBrainEscapeGateDefinitions.Closing => DoorObjectNativeListAddresses.EscapeGateClosing,
        ColoredDoorObject { Phase: ColoredDoorPhase.Waiting } colored => DoorObjectNativeListAddresses.Colored(colored.Color, colored.Orientation).Waiting,
        ColoredDoorObject { Phase: ColoredDoorPhase.Closing } colored => DoorObjectNativeListAddresses.Colored(colored.Color, colored.Orientation).Closing,
        GreyDoorObject { Phase: GreyDoorPhase.Locked } grey => DoorObjectNativeListAddresses.Grey(grey.Variant).Waiting,
        GreyDoorObject { Phase: GreyDoorPhase.Closing } grey => DoorObjectNativeListAddresses.Grey(grey.Variant).Closing,
        _ => throw new InvalidOperationException($"{behaviour?.GetType().Name ?? "An empty place"} stands for no native door list."),
    };

    /// <summary>Whether <paramref name="behaviour"/> is a door closing behind Samus.</summary>
    internal static bool IsClosing(RoomObjectBehaviour? behaviour) => behaviour switch
    {
        ColoredDoorObject colored => colored.Phase == ColoredDoorPhase.Closing,
        GreyDoorObject grey => grey.Phase == GreyDoorPhase.Closing,
        BlueDoorCapObject blue => blue.Phase == BlueDoorCapPhase.Closing,
        MotherBrainEscapeGateObject gate => gate.Change == MotherBrainEscapeGateDefinitions.Closing,
        _ => false,
    };
}
