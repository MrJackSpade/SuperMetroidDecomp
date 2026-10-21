using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>The enemies, slot and state of one stoke behavior create result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="State">The state.</param>
internal readonly record struct StokeBehaviorCreateResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, StokeEnemyState State);
