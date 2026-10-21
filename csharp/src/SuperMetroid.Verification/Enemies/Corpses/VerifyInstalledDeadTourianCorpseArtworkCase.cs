using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>The definition and variant count of one verify installed dead tourian corpse artwork case.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="VariantCount">The variant count.</param>
internal readonly record struct VerifyInstalledDeadTourianCorpseArtworkCase(EnemyDefinitionId Definition, int VariantCount);
