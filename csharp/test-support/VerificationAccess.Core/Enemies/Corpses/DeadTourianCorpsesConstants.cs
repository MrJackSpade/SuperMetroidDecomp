using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>Cartridge values and helpers of <see cref="DeadTourianCorpses"/> that only verification reads.</summary>
internal static class DeadTourianCorpsesConstants
{
    /// <summary>The corpse each of <paramref name="species"/>'s variants lies as, by variant number.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The species is not a dead Tourian corpse.</exception>
    internal static DeadTourianCorpseDefinition[] ByVariant(DeadTourianCorpseSpecies species) => [.. DeadTourianCorpses.Variants(species)];
}
