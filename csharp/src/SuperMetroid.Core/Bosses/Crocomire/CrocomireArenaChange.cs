using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire asked for one of his arena's room objects at a block (#2562).</summary>
/// <param name="Request">The arena's change and its block.</param>
public sealed record CrocomireArenaChange(RoomObjectSpawnRequest<CrocomireArenaMutation> Request) : EnemyPassEvent;
