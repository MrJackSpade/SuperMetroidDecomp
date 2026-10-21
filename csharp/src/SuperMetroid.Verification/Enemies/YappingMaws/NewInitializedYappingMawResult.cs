using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>The enemies, slot and state of one new initialized yapping maw result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="State">The state.</param>
internal readonly record struct NewInitializedYappingMawResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, YappingMawEnemyState State);
