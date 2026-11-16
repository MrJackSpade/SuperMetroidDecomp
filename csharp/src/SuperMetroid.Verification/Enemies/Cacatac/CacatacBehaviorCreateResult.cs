using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>The enemies, slot and state of one cacatac behavior create result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="State">The state.</param>
internal readonly record struct CacatacBehaviorCreateResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, CacatacEnemyState State);
