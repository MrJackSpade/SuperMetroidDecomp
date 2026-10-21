using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>The definition and parameter of one verify pipe bug behavior case.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="Parameter">The parameter.</param>
internal readonly record struct VerifyPipeBugBehaviorCase(EnemyDefinitionId Definition, ushort Parameter);
