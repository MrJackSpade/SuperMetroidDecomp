using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>How a retail scroll program writes its scroll states.</summary>
internal static class RoomObjectScrollChanges
{
    /// <summary>Applies <paramref name="program"/>'s ordered scroll writes.</summary>
    internal static void Apply(RoomScrollChange program, Action<int, RoomScrollState> write)
    {
        Ensure.NotNull(write);
        foreach (RoomScrollWrite step in RoomObjectScrollChangeDefinitions.Changes[program])
            write(step.Index, step.State);
    }
}
