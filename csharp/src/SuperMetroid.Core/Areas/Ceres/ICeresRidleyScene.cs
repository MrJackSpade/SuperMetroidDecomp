using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Ceres Ridley's scene, which draws the door of his room while the door hands its drawing to it (#1877). The door of
/// Ridley's room attaches itself to the scene as it initializes.
/// </summary>
internal interface ICeresRidleyScene
{
    /// <summary>Attaches <paramref name="door"/> as the door of Ridley's room as it initializes.</summary>
    void AttachRoomDoor(RoomEnemySlot door);
}
