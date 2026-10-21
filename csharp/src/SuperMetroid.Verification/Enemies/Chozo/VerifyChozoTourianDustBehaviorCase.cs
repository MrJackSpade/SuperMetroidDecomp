using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>The kind and program of one verify chozo tourian dust behavior case.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Program">The program.</param>
internal readonly record struct VerifyChozoTourianDustBehaviorCase(RoomEnemyProjectileKind Kind, ChozoTourianDustProgram Program);
