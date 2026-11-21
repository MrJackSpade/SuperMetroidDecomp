namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The state of the door of Ceres Ridley's room (#1953): how it shows (#2545, #2573).</summary>
internal sealed partial class CeresRidleyRoomDoorState
{
    /// <summary>How the door shows: its actor frames, or its overlay picture from its animation's showing it until it shows its actor again.</summary>
    public CeresRidleyRoomDoorLook Look { get; internal set; }
}
