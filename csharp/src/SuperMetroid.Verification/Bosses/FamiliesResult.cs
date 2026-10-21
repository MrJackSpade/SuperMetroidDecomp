using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Bosses;

/// <summary>The definition and frames of one families result.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="Frames">The frames.</param>
internal readonly record struct FamiliesResult(EnemyDefinitionId Definition, EnemySpritemapDefinition[] Frames);
