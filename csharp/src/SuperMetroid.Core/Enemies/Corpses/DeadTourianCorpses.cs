namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Finds a dead Tourian enemy's corpse variant.</summary>
internal static class DeadTourianCorpses
{
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
