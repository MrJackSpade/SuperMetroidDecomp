using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>The enemies and state of one new crocomire result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="State">The state.</param>
internal readonly record struct NewCrocomireResult(RoomEnemySystem Enemies, CrocomireEnemyState State);
