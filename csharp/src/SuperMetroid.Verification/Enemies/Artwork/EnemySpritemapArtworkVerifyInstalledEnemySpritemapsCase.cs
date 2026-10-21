using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Ripper;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The definition, program and name of one enemy spritemap artwork verify installed enemy spritemaps case.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="Program">The program.</param>
/// <param name="Name">The name.</param>
internal readonly record struct EnemySpritemapArtworkVerifyInstalledEnemySpritemapsCase(EnemyDefinitionId Definition, RipperProgram Program, string Name);
