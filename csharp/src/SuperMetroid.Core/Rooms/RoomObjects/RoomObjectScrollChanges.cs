using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>How a retail scroll change writes its scroll states.</summary>
internal static class RoomObjectScrollChanges
{
    /// <summary>Applies <paramref name="change"/>'s ordered scroll writes, each to its screen of the room.</summary>
    internal static void Apply(RoomScrollChange change, Action<Position16<RoomScreens>, RoomScrollState> write)
    {
        Ensure.NotNull(write);
        foreach (ScreenScrollChange step in RoomObjectScrollChangeDefinitions.Changes[change])
            write(step.Screen, step.State);
    }
}
