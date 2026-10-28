namespace SuperMetroid.Core.Rendering;

/// <summary>How many inks a tile layer's characters draw with.</summary>
public enum TileInkDepth
{
    /// <summary>Each pixel is one of four inks.</summary>
    FourColours,

    /// <summary>Each pixel is one of sixteen inks.</summary>
    SixteenColours,
}
