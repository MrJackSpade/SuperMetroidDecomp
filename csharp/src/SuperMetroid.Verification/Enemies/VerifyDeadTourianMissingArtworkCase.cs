using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies;

/// <summary>The definition and variants of one verify dead tourian missing artwork case.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="Variants">The variants.</param>
internal readonly record struct VerifyDeadTourianMissingArtworkCase(EnemyDefinitionId Definition, int Variants);
