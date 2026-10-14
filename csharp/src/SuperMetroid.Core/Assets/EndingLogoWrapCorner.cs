namespace SuperMetroid.Core.Assets;

/// <summary>Separately packed corner and side pieces in the ending logo atlas.</summary>
internal enum EndingLogoWrapCorner : ushort
{
    /// <summary>Tile$01, sixteen-pixel upper endpoint of the right side.</summary>
    UpperSideEndpoint = 0x01,
    /// <summary>Tile$20, sixteen-pixel lower endpoint of the right side.</summary>
    LowerSideEndpoint = 0x20,
    /// <summary>Tile$5B, eight-pixel left corner of the right-wrap composition.</summary>
    LeftCorner = 0x5b,
    /// <summary>Tile$6C, eight-pixel junction at the inner side of the right wrap.</summary>
    InnerSideJunction = 0x6c,
    /// <summary>Tile$6D, sixteen-pixel outer side junction.</summary>
    OuterSideJunction = 0x6d,
}
