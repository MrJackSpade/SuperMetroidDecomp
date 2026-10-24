using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Editable health-band palette JSON, separate from rainbow cycling and death-fade artwork; array references, rows, and RGB5 entries remain caller-owned and mutable.</summary>
public sealed record MotherBrainHealthPaletteDocument
{
    /// <summary>Schema version required to match <see cref="MotherBrainHealthPaletteFormat.Version"/> during loading or writing.</summary>
    public required int Version { get; init; }
    /// <summary>Four rows of fifteen RGB5 colors in native <c>MotherBrainHealthBasedPalettes_BrainBody</c> order from <c>$AD:E6A2</c>; ascending row index denotes lower health, and each row supplies both body BG and brain/neck OBJ colors 1..15.</summary>
    public required PaletteRgb5[][] Body { get; init; }
    /// <summary>Four rows of fifteen RGB5 colors in native <c>MotherBrainHealthBasedPalettes_BackLeg</c> order from <c>$AD:E742</c>, aligned with the body health bands and supplying rear-leg OBJ palette colors 1..15.</summary>
    public required PaletteRgb5[][] BackLegs { get; init; }
}
