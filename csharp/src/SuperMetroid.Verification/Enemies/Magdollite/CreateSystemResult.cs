using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>The enemies, slot and state of one create system result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="State">The state.</param>
internal readonly record struct CreateSystemResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, MagdolliteEnemyState State);
