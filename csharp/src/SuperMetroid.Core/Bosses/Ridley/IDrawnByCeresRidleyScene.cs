using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// An occupant that can hand its drawing to Ceres Ridley's scene: while it has, the scene draws the room's private
/// overlay at the occupant's position after the Baby, and the occupant's own drawing stays hidden.
/// </summary>
internal interface IDrawnByCeresRidleyScene : IEnemyEntity
{
    /// <summary>Whether the occupant has currently handed its drawing to Ceres Ridley's scene.</summary>
    bool HandsDrawingToRidley { get; }
}
