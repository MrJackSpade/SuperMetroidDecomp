using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Verification access to <see cref="PauseSelectorPresentation"/> members production does not use.</summary>
internal static class PauseSelectorPresentationAccess
{
    /// <summary>Whether a selector frame stores authored parts rather than its calculated stock composition.</summary>
    private static bool StoresParts(PauseSelectorPresentation presentation, string frame) =>
        PauseSelectorSourcePresentation.Of(presentation).visuals[frame].authored is not null;

    /// <summary>The reserve, beam and equipment phase frame sequences.</summary>
    private static PauseSelectorPresentation.PhaseFrames[] Categories(PauseSelectorPresentation presentation) =>
        [presentation.reserve, presentation.beam, presentation.equipment];

    /// <summary>Inspects timing, composition, and anchor overrides across reserve, beam, and equipment selectors.</summary>
    extension(PauseSelectorPresentation self)
    {
        /// <summary>Counts explicit selector-frame duration overrides.</summary>
        internal int StoredDurationCount => self.durationOverrides?.Count ?? 0;

        /// <summary>Checks authored parts in any selector basis or override frame.</summary>
        internal bool StoresCompositionParts => Categories(self).Any(category =>
            StoresParts(self, category.basis) || (category.Overrides?.Values.Any(frame => StoresParts(self, frame)) ?? false));

        /// <summary>Counts replaced selector compositions across all three categories.</summary>
        internal int StoredCompositionOverrideCount => Categories(self).Sum(category =>
            category.Overrides?.Count ?? 0);

        /// <summary>Counts explicitly supplied X and Y anchor components.</summary>
        internal int StoredAnchorComponentCount => self.anchorOverrides.Values.Sum(value => (value.X.HasValue ? 1 : 0) + (value.Y.HasValue ? 1 : 0));
    }
}
