using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies.Ripper;

/// <summary>The record and definition of one verify ripper behavior case.</summary>
/// <param name="Record">The record.</param>
/// <param name="Definition">The definition.</param>
internal readonly record struct VerifyRipperBehaviorCase(int Record, EnemyDefinitionId Definition);
