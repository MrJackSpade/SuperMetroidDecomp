namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species owner that keeps room-scoped state. <see cref="RoomEnemySystem"/> drives every species through the same
/// lifecycle points instead of resetting their state itself.
/// </summary>
internal interface IEnemySpecies
{
    /// <summary>Discards the previous room's species state while a new population loads.</summary>
    void ResetForRoomLoad(IRoomEnemyLoad request)
    {
    }

    /// <summary>Clears the species' per-frame publications at the start of an enemy frame.</summary>
    void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
