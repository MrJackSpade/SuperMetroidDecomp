using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>A Chozo statue's ask for its hand, bound to the statue whose wake the hand's trigger starts (#2554).</summary>
/// <param name="Request">The hand room object and its block.</param>
/// <param name="Hand">The statue the hand belongs to.</param>
public sealed record ChozoStatueHandAsked(RoomObjectSpawnRequest<ChozoStatueRoomObject> Request, IChozoStatueHandActivation Hand) : EnemyPassEvent;
