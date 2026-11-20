namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What the room's species take from the room's load before its population loads (#2571): the scenario a species plays out
/// in the room and the destination camera their initialization reads.
/// </summary>
internal interface IEnemyLoadPreparation
{
    /// <summary>Hands the species what <paramref name="request"/> loads the room with.</summary>
    /// <param name="request">The room's load.</param>
    void PrepareRoomLoad(IRoomEnemyLoad request);
}
