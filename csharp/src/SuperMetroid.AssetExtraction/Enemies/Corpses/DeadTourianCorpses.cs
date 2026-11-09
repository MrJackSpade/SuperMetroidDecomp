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

    /// <summary>One Zoomer, Ripper, or Skree corpse's pose and height.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The species or its variant is not a dead Tourian corpse.</exception>
    internal static DeadTourianCorpseDefinition For(DeadTourianCorpseSpecies species, int variantIndex)
    {
        if (!DeadTourianCorpseDefinitions.Variants.TryGetValue(species, out DeadTourianCorpseDefinition[]? variants))
            throw new ArgumentOutOfRangeException(nameof(species), species, null);
        if ((uint)variantIndex >= variants.Length)
            throw new ArgumentOutOfRangeException(nameof(variantIndex), variantIndex, null);
        return variants[variantIndex];
    }

    /// <summary>The cartridge row upload list corpse <paramref name="variantIndex"/> of <paramref name="species"/> names.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The species or its variant is not a dead Tourian corpse.</exception>
    internal static CorpseRowUploadList RowUploadList(DeadTourianCorpseSpecies species, int variantIndex)
    {
        _ = For(species, variantIndex);
        return new CorpseRowUploadList(species switch
        {
            DeadTourianCorpseSpecies.Zoomer => RottingCorpseSpecies.Zoomer,
            DeadTourianCorpseSpecies.Ripper => RottingCorpseSpecies.Ripper,
            DeadTourianCorpseSpecies.Skree => RottingCorpseSpecies.Skree,
            _ => throw new ArgumentOutOfRangeException(nameof(species), species, null),
        }, variantIndex);
    }
}
