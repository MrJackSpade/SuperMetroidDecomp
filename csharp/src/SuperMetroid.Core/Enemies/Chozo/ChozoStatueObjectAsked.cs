using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>A Chozo statue asked its room to spawn one of its room objects at a block (#2562).</summary>
/// <param name="Request">The room object and the block it spawns at.</param>
public sealed partial record ChozoStatueObjectAsked(RoomObjectSpawnRequest<ChozoStatueRoomObject> Request) : EnemyPassEvent;
