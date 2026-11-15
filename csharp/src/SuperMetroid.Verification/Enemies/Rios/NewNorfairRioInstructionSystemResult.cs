using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>The enemies, slot and state of one new norfair rio instruction system result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="State">The state.</param>
internal readonly record struct NewNorfairRioInstructionSystemResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, NorfairRioEnemyState State);
