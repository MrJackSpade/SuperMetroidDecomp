using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The kind, pose and list of one fake kraid projectile instruction program definitions case.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Pose">The pose.</param>
/// <param name="List">The list.</param>
internal readonly record struct FakeKraidProjectileInstructionProgramDefinitionsCase(RoomEnemyProjectileKind Kind, FakeKraidProjectilePose Pose, ushort List);
