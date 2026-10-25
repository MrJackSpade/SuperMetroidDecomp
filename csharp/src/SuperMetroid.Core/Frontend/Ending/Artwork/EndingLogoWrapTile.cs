namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The ending logo atlas tiles the right wrap at $8C:BAA9 places, other than its separately packed corners.</summary>
internal enum EndingLogoWrapTile
{
    /// <summary>The top of the inner side.</summary>
    InnerSideTop,
    /// <summary>The right span of the half-turned closing arc.</summary>
    ClosingRightSpan,
    /// <summary>The middle span of the closing arc.</summary>
    ClosingMiddleSpan,
    /// <summary>The left span of the closing arc.</summary>
    ClosingLeftSpan,
    /// <summary>The outer left tip of the closing arc.</summary>
    ClosingOuterLeftTip,
    /// <summary>The bottom of the inner side.</summary>
    InnerSideBottom,
    /// <summary>The corner at the right end of the closing arc.</summary>
    ClosingRightCorner,
    /// <summary>The inner left tip of the closing arc.</summary>
    ClosingInnerLeftTip,
    /// <summary>The inner right tip of the closing arc.</summary>
    ClosingInnerRightTip,
    /// <summary>The left top cap, which the wrap also places cropped to eight pixels.</summary>
    LeftCap,
    /// <summary>The right top cap.</summary>
    RightCap,
    /// <summary>The left half of the upper span.</summary>
    UpperSpanLeft,
    /// <summary>The right half of the upper span.</summary>
    UpperSpanRight,
    /// <summary>The upper-right patch's top-left cell.</summary>
    PatchTopLeft,
    /// <summary>The upper-right patch's top-right cell.</summary>
    PatchTopRight,
    /// <summary>The left cell of the inner edge.</summary>
    InnerEdgeLeft,
    /// <summary>The right cell of the inner edge.</summary>
    InnerEdgeRight,
    /// <summary>The upper-right patch's bottom-left cell.</summary>
    PatchBottomLeft,
    /// <summary>The upper-right patch's bottom-right cell.</summary>
    PatchBottomRight,
}
