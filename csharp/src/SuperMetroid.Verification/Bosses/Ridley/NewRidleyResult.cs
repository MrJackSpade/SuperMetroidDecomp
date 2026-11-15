using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The enemies, state and body of one new ridley result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="State">The state.</param>
/// <param name="Body">The body.</param>
internal readonly record struct NewRidleyResult(RoomEnemySystem Enemies, RidleyEnemyState State, RoomEnemySlot Body);
