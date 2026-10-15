namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The ending logo atlas tiles the right wrap at $8C:BAA9 places, other than its separately packed corners.</summary>
internal enum EndingLogoWrapTile : ushort
{
    /// <summary>Tile $00, the top of the inner side.</summary>
    InnerSideTop = 0x00,
    /// <summary>Tile $09, the right span of the half-turned closing arc.</summary>
    ClosingRightSpan = 0x09,
    /// <summary>Tile $0B, the middle span of the closing arc.</summary>
    ClosingMiddleSpan = 0x0b,
    /// <summary>Tile $0D, the left span of the closing arc.</summary>
    ClosingLeftSpan = 0x0d,
    /// <summary>Tile $0F, the outer left tip of the closing arc.</summary>
    ClosingOuterLeftTip = 0x0f,
    /// <summary>Tile $10, the bottom of the inner side.</summary>
    InnerSideBottom = 0x10,
    /// <summary>Tile $29, the corner at the right end of the closing arc.</summary>
    ClosingRightCorner = 0x29,
    /// <summary>Tile $2B, the inner left tip of the closing arc.</summary>
    ClosingInnerLeftTip = 0x2b,
    /// <summary>Tile $2C, the inner right tip of the closing arc.</summary>
    ClosingInnerRightTip = 0x2c,
    /// <summary>Tile $48, the left top cap, which the wrap also places cropped to eight pixels.</summary>
    LeftCap = 0x48,
    /// <summary>Tile $49, the right top cap.</summary>
    RightCap = 0x49,
    /// <summary>Tile $4C, the left half of the upper span.</summary>
    UpperSpanLeft = 0x4c,
    /// <summary>Tile $4E, the right half of the upper span.</summary>
    UpperSpanRight = 0x4e,
    /// <summary>Tile $68, the upper-right patch's top-left cell.</summary>
    PatchTopLeft = 0x68,
    /// <summary>Tile $69, the upper-right patch's top-right cell.</summary>
    PatchTopRight = 0x69,
    /// <summary>Tile $6A, the left cell of the inner edge.</summary>
    InnerEdgeLeft = 0x6a,
    /// <summary>Tile $6B, the right cell of the inner edge.</summary>
    InnerEdgeRight = 0x6b,
    /// <summary>Tile $78, the upper-right patch's bottom-left cell.</summary>
    PatchBottomLeft = 0x78,
    /// <summary>Tile $79, the upper-right patch's bottom-right cell.</summary>
    PatchBottomRight = 0x79,
}
