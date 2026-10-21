using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>The enemies, slot and state of one new etecoon result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="State">The state.</param>
internal readonly record struct NewEtecoonResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, EtecoonEnemyState State);
