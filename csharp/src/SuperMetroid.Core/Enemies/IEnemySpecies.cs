using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species owner that keeps room-scoped state. Each room builds its own species owners with its enemy system, so a
/// species starts every room fresh; <see cref="RoomEnemySystem"/> drives every species through the same frame points.
/// </summary>
internal interface IEnemySpecies
{
    /// <summary>Clears the species' per-frame publications at the start of an enemy frame.</summary>
    void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
