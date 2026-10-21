using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.MorphBallEye;

/// <summary>The enemies, slot and state of one morph ball eye behavior create result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="State">The state.</param>
internal readonly record struct MorphBallEyeBehaviorCreateResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, MorphBallEyeEnemyState State);
