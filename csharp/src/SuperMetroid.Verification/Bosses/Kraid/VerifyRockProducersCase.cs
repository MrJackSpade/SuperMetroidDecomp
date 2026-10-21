using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The kind and program of one verify rock producers case.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Program">The program.</param>
internal readonly record struct VerifyRockProducersCase(RoomEnemyProjectileKind Kind, KraidRockProgram Program);
