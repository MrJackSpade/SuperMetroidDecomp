using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>One RGB5 palette payload whose array dimensions depend on its rainbow, drain, revival, or normal-restoration role; each channel must be from 0 through 31.</summary>
public sealed partial record MotherBrainRainbowPaletteFrameDocument
{
    /// <summary>Shared body-BG and brain/neck-OBJ inks copied starting at CGRAM $41 and $91: 15 colors for rainbow/drain/normal, or 13 for revival.</summary>
    public required PaletteRgb5[] Body { get; init; }
    /// <summary>Rear-leg OBJ inks: 15 copied to CGRAM starting at $B1 for rainbow/normal, or five starting at $B4 for drain/revival.</summary>
    public required PaletteRgb5[] BackLegs { get; init; }
}
