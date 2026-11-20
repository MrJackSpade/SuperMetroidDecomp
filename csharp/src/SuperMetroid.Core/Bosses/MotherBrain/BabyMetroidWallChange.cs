using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid asked for one of its wall's room objects at a block (#2562).</summary>
/// <param name="Request">The wall's change and its block.</param>
public sealed record BabyMetroidWallChange(RoomObjectSpawnRequest<BabyMetroidWallMutation> Request) : EnemyPassEvent;
