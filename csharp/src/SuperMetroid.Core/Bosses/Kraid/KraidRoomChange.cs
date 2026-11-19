using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid asked for one of his ceiling's or platforms' room objects at a block (#2562).</summary>
/// <param name="Request">The room's change and its block.</param>
public sealed record KraidRoomChange(RoomObjectSpawnRequest<KraidRoomMutation> Request) : EnemyPassEvent;
