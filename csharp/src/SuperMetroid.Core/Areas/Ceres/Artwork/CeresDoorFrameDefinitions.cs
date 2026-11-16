using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The presentation boundary of the door of Ceres Ridley's room (#641, #1989): the frame Ridley's scene draws the door
/// in while the door hands its drawing to him, which his encounter draws itself.
/// </summary>
internal static class CeresDoorFrameDefinitions
{
    /// <summary>The door of Ridley's room as his scene draws it.</summary>
    internal const EnemyFrameId RidleyRoomDoorDrawnByRidley = EnemyFrameId.CeresRidleyRoomDoorClosed;
}
