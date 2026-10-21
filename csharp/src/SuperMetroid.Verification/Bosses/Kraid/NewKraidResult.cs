using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The enemies, state, body and arm of one new kraid result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="State">The state.</param>
/// <param name="Body">The body.</param>
/// <param name="Arm">The arm.</param>
internal readonly record struct NewKraidResult(RoomEnemySystem Enemies, KraidEnemyState State, RoomEnemySlot Body, RoomEnemySlot Arm);
