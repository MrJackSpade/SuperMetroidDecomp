using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>One pulse phase's two arrow bevel colors, each with red, green, and blue components from zero through 31.</summary>
public sealed partial record PauseReserveArrowFrame
{
    /// <summary>RGB5 color written to arrow palette slot six for this phase.</summary>
    public required PaletteRgb5 Shade { get; init; }
    /// <summary>RGB5 color written to arrow palette slot eleven for this phase.</summary>
    public required PaletteRgb5 Light { get; init; }
}
