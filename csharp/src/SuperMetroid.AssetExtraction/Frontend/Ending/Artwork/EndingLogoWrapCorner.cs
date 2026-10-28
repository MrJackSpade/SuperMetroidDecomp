namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Separately packed corner and side pieces in the ending logo atlas.</summary>
internal enum EndingLogoWrapCorner
{
    /// <summary>Sixteen-pixel upper endpoint of the right side.</summary>
    UpperSideEndpoint,
    /// <summary>Sixteen-pixel lower endpoint of the right side.</summary>
    LowerSideEndpoint,
    /// <summary>Eight-pixel left corner of the right-wrap composition.</summary>
    LeftCorner,
    /// <summary>Eight-pixel junction at the inner side of the right wrap.</summary>
    InnerSideJunction,
    /// <summary>Sixteen-pixel outer side junction.</summary>
    OuterSideJunction,
}
