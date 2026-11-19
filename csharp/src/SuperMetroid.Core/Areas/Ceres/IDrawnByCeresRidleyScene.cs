using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// An occupant that can hand its drawing to Ceres Ridley's scene: while it has, the scene draws the room's private
/// overlay at the occupant's position after the Baby, and the occupant's own drawing stays hidden.
/// </summary>
internal interface IDrawnByCeresRidleyScene : IEnemyEntity
{
    /// <summary>Who draws the occupant now: itself, or Ceres Ridley's scene it has handed its drawing to (#2545).</summary>
    CeresRidleyRoomDoorDrawing Drawing { get; }
}
