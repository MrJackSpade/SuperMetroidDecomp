namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>How many inks a frontend screen's characters draw with.</summary>
internal enum TileInkDepth
{
    /// <summary>Each pixel is one of four inks.</summary>
    FourColours,

    /// <summary>Each pixel is one of sixteen inks.</summary>
    SixteenColours,
}
