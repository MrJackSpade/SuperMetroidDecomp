using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Verification.Harness;

/// <summary>Synthetic rooms that regression fixtures reference without loading.</summary>
internal static class FixtureRoomHeaders
{
    /// <summary>The destination of a synthetic door whose room is never entered.</summary>
    public static RoomHeaderDefinition UnloadedDestination { get; } =
        new("unloaded fixture door destination", 0, AreaId.Crateria, new(0, 0), new(1, 1), 0, 0, 0, 0);
}
