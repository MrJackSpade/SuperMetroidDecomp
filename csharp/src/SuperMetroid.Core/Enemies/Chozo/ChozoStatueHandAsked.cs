using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>A Chozo statue asked its room to spawn its hand at a block, carrying what triggering the hand wakes (#2554).</summary>
/// <param name="Request">The hand and the block it spawns at.</param>
/// <param name="Hand">The statue the hand belongs to, which triggering it wakes.</param>
internal sealed partial record ChozoStatueHandAsked(RoomObjectSpawnRequest<ChozoStatueHandObject> Request, ChozoStatueHandTrigger Hand) : EnemyPassEvent;
