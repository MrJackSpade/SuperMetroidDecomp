using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>The definition and variant of one dead tourian corpse artwork copy.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="Variant">The variant.</param>
internal readonly record struct DeadTourianCorpseArtworkCopy(EnemyDefinitionId Definition, int Variant);
