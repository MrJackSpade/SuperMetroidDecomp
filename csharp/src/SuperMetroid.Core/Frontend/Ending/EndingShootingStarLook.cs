namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What a post-credits shooting star shows as it flies (#2202, #2359), from its hidden lead-in to the full star.</summary>
internal enum EndingShootingStarLook
{
    /// <summary>The lead-in at the origin before the star shows: nothing is drawn.</summary>
    LeadIn,
    /// <summary>A faint speck, the star's first visible picture, which a still star keeps for its whole flight.</summary>
    FaintSpeck,
    /// <summary>A bright speck; from here on the star's acceleration doubles.</summary>
    BrightSpeck,
    /// <summary>A dim star.</summary>
    DimStar,
    /// <summary>The full star, which the star keeps for the rest of its flight.</summary>
    FullStar,
}
