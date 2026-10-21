using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>The kind and program of one verify shaktool projectile behavior case.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Program">The program.</param>
internal readonly record struct VerifyShaktoolProjectileBehaviorCase(RoomEnemyProjectileKind Kind, ShaktoolProjectileProgram Program);
