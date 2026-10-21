using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The definition, program and name of one verify installed enemy spritemaps case.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="Program">The program.</param>
/// <param name="Name">The name.</param>
internal readonly record struct VerifyInstalledEnemySpritemapsCase(EnemyDefinitionId Definition, PipeBugProgram Program, string Name);
