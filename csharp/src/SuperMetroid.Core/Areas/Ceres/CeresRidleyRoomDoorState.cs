namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The state of the door of Ceres Ridley's room (#1953): who draws it (#2545).</summary>
internal sealed class CeresRidleyRoomDoorState
{
    /// <summary>Who draws the door: itself, or Ridley's scene from its animation's handing over until it takes it back.</summary>
    public CeresRidleyRoomDoorDrawing Drawing { get; internal set; }
}
