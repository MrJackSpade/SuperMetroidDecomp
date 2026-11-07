using SuperMetroid.Core.Enemies.Corpses;
namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>Finds a dead Tourian enemy's corpse variants, whose rotting pictures the import composes.</summary>
internal static class DeadTourianCorpses
{
    /// <summary>How many corpse variants <paramref name="species"/> has.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The species is not a dead Tourian corpse.</exception>
    internal static int VariantCount(DeadTourianCorpseSpecies species) =>
        DeadTourianCorpseDefinitions.Variants.TryGetValue(species, out DeadTourianCorpseDefinition[]? variants)
            ? variants.Length
            : throw new ArgumentOutOfRangeException(nameof(species), species, null);

    /// <summary>One Zoomer, Ripper, or Skree corpse's animation, rotting, kind, movement, and rotation identities.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The species or its variant is not a dead Tourian corpse.</exception>
    internal static DeadTourianCorpseDefinition For(DeadTourianCorpseSpecies species, int variantIndex)
    {
        if (!DeadTourianCorpseDefinitions.Variants.TryGetValue(species, out DeadTourianCorpseDefinition[]? variants))
            throw new ArgumentOutOfRangeException(nameof(species), species, null);
        if ((uint)variantIndex >= variants.Length)
            throw new ArgumentOutOfRangeException(nameof(variantIndex), variantIndex, null);
        return variants[variantIndex];
    }
}
