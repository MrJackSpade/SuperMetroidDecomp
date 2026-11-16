using SuperMetroid.Core.Enemies.Corpses;
namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// The cartridge's dead Tourian corpse variants (#1810): a placement names its corpse by a variant number of its species,
/// and each variant has its own configuration and row upload list. The variant numbers are the import's knowledge; the
/// game knows each corpse by how it lies, and variants that show the same picture are one corpse.
/// </summary>
internal static class DeadTourianCorpses
{
    /// <summary>The corpse each of a species' variants lies as, by variant number.</summary>
    private static readonly Dictionary<DeadTourianCorpseSpecies, DeadTourianCorpseDefinition[]> CorpseByVariant = new()
    {
        [DeadTourianCorpseSpecies.Zoomer] =
            [DeadTourianCorpseDefinitions.ZoomerOnItsBack, DeadTourianCorpseDefinitions.ZoomerUpright, DeadTourianCorpseDefinitions.ZoomerOnItsBack],
        [DeadTourianCorpseSpecies.Ripper] = [DeadTourianCorpseDefinitions.RipperLevel, DeadTourianCorpseDefinitions.RipperTilted],
        [DeadTourianCorpseSpecies.Skree] = [DeadTourianCorpseDefinitions.Skree, DeadTourianCorpseDefinitions.Skree, DeadTourianCorpseDefinitions.Skree],
    };

    /// <summary>How many corpse variants <paramref name="species"/> has.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The species is not a dead Tourian corpse.</exception>
    internal static int VariantCount(DeadTourianCorpseSpecies species) => Variants(species).Length;

    /// <summary>The corpse each of <paramref name="species"/>'s variants lies as, by variant number.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The species is not a dead Tourian corpse.</exception>
    internal static DeadTourianCorpseDefinition[] ByVariant(DeadTourianCorpseSpecies species) => [.. Variants(species)];

    /// <summary>The corpse variant <paramref name="variantIndex"/> of <paramref name="species"/> lies as: its pose and height.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The species or its variant is not a dead Tourian corpse.</exception>
    internal static DeadTourianCorpseDefinition For(DeadTourianCorpseSpecies species, int variantIndex)
    {
        DeadTourianCorpseDefinition[] variants = Variants(species);
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

    /// <summary>The variants of <paramref name="species"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The species is not a dead Tourian corpse.</exception>
    private static DeadTourianCorpseDefinition[] Variants(DeadTourianCorpseSpecies species) =>
        CorpseByVariant.TryGetValue(species, out DeadTourianCorpseDefinition[]? variants)
            ? variants
            : throw new ArgumentOutOfRangeException(nameof(species), species, null);
}
