using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.FaceBlocks;

/// <summary>The enemies, slot and state of one blue brinstar face block behavior create result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="State">The state.</param>
internal readonly record struct BlueBrinstarFaceBlockBehaviorCreateResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, BlueBrinstarFaceBlockEnemyState State);
