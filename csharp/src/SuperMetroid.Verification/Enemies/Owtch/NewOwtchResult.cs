using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Owtch;

/// <summary>The enemies, slot and state of one new owtch result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="State">The state.</param>
internal readonly record struct NewOwtchResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, OwtchEnemyState State);
