using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The room-object kind a grey door cap is placed with, read back from the typed cap (#1691): a live room object keeps no
/// kind, but a grey cap's variant names exactly one, which verification needs to compare a cap with the header native
/// memory holds for it.
/// </summary>
internal static class GreyDoorPlacedKinds
{
    /// <summary>The one grey-door kind whose placement sets up a cap of <paramref name="variant"/>.</summary>
    /// <exception cref="InvalidOperationException">No grey-door kind, or more than one, sets up that variant.</exception>
    internal static RoomObjectKind Of(GreyDoorVariant variant) =>
        DoorObjectKindDefinitions.GreyDoors
            .Single(placed => placed.Value == variant.Orientation && (placed.Key == RoomObjectKind.BombTorizoGreyDoor) == variant.BombTorizo)
            .Key;
}
