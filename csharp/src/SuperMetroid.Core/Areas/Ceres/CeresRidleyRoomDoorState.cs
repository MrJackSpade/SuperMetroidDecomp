namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The state of the door of Ceres Ridley's room (#1953).</summary>
internal sealed class CeresRidleyRoomDoorState
{
    /// <summary>Whether the door's animation has handed its drawing to Ridley's scene and not yet taken it back.</summary>
    public bool HandsDrawingToRidley { get; internal set; }
}
