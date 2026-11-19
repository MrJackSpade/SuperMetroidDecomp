using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Shitroid asked for one of its wall's room objects at a block (#2562).</summary>
/// <param name="Request">The wall's change and its block.</param>
public sealed record ShitroidWallChange(RoomObjectSpawnRequest<ShitroidWallMutation> Request) : EnemyPassEvent;
