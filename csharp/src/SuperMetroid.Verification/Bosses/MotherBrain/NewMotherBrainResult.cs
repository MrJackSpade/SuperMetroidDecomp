using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The enemies and state of one new mother brain result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="State">The state.</param>
internal readonly record struct NewMotherBrainResult(RoomEnemySystem Enemies, MotherBrainEnemyState State);
