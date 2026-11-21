using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Harness;

/// <summary>Synthetic rooms and doors that regression fixtures reference without loading.</summary>
internal static class FixtureRooms
{
    /// <summary>The destination of a synthetic door whose room is never entered.</summary>
    public static RoomDefinition UnloadedDestination { get; } =
        new("unloaded fixture door destination", AreaId.Crateria, new(0, 0), new(1, 1)) { CameraLead = RoomCameraLeads.None };
}
