using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>The enemies, slot and state of one install result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="State">The state.</param>
internal readonly record struct InstallResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, DragonEnemyState State);
