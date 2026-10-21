using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Botwoon;

/// <summary>The enemies, head and state of one new botwoon head result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Head">The head.</param>
/// <param name="State">The state.</param>
internal readonly record struct NewBotwoonHeadResult(RoomEnemySystem Enemies, RoomEnemySlot Head, BotwoonEnemyState State);
