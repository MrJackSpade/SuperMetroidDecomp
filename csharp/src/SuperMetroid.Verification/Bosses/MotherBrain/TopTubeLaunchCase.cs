using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>A top tube projectile kind, its program and where it launches.</summary>
/// <param name="Kind">The projectile kind.</param>
/// <param name="Program">The instruction program.</param>
/// <param name="X">The launch X coordinate.</param>
/// <param name="Y">The launch Y coordinate.</param>
internal readonly record struct TopTubeLaunchCase(RoomEnemyProjectileKind Kind, ushort Program, ushort X, ushort Y);
