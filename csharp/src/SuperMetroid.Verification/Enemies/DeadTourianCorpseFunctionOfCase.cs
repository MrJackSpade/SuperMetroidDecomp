using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Verification.Enemies;

/// <summary>The species and word of one dead tourian corpse function of case.</summary>
/// <param name="Species">The species.</param>
/// <param name="Word">The word.</param>
internal readonly record struct DeadTourianCorpseFunctionOfCase(DeadTourianCorpseSpecies Species, ushort Word);
