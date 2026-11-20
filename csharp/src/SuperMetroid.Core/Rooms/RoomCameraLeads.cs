namespace SuperMetroid.Core.Rooms;

/// <summary>The camera leads rooms share; a room that names none takes <see cref="Standard"/>.</summary>
public static class RoomCameraLeads
{
    /// <summary>The lead nearly every room uses: the camera's top 112 pixels above Samus as she falls and 160 as she rises.</summary>
    public static RoomCameraLead Standard { get; } = new(112, 160);

    /// <summary>A lead showing less below a falling Samus: the camera's top 144 pixels above her as she falls and 160 as she rises.</summary>
    public static RoomCameraLead ShallowerFall { get; } = new(144, 160);

    /// <summary>A lead showing least below a falling Samus: the camera's top 160 pixels above her both as she falls and as she rises.</summary>
    public static RoomCameraLead ShallowestFall { get; } = new(160, 160);

    /// <summary>No lead: the camera aims its top at Samus herself whichever way she moves.</summary>
    public static RoomCameraLead None { get; } = new(0, 0);
}
