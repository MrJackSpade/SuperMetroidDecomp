using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Harness;

/// <summary>Synthetic rooms and doors that regression fixtures reference without loading.</summary>
internal static class FixtureRooms
{
    /// <summary>The destination of a synthetic door whose room is never entered.</summary>
    public static RoomDefinition UnloadedDestination { get; } =
        new("unloaded fixture door destination", 0, AreaId.Crateria, new(0, 0), new(1, 1), 0, 0, RoomDoorTransitionStyle.Standard);

    /// <summary>A synthetic door that no library-background transfer or room-FX record names.</summary>
    public static DoorDefinition UnmatchedDoor { get; } = new(
        Name: "fixture door matching no transfer",
        DestinationRoom: UnloadedDestination,
        Orientation: new(DoorDirection.Right, DoorClosingBehavior.None),
        RoomObjectBlock: default,
        DestinationScreen: default,
        SamusSpeed: DoorSamusSpeed.None,
        Setup: DoorSetupDefinitions.None);
}
