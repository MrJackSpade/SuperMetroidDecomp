using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Editable door palette and Mode 7 platform-strip schema, separate from the indexed 4bpp character PNG and engine-owned door/escape timing.</summary>
public sealed record CeresDoorVisualDocument
{
    /// <summary>Visual schema revision; loading and serialization currently require version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Fifteen ordered nontransparent RGB5 inks corresponding to $A6:F4EE; every red, green, and blue channel must be 0–31.</summary>
    public required PaletteRgb5[] Normal { get; init; }
    /// <summary>Fifteen ordered nontransparent escape RGB5 inks corresponding to $A6:F50E, independent of the normal setup image.</summary>
    public required PaletteRgb5[] Escape { get; init; }
    /// <summary>Eight rows of six RGB5 inks corresponding to the $A6:F871 animation table, installed at CGRAM 41–46 in actor-selected frame order.</summary>
    public required PaletteRgb5[][] Animation { get; init; }
    /// <summary>Two ordered four-cell strips corresponding to $A6:F918/$F91C, each containing byte-valued Mode 7 character indices rather than ordinary BG palette/flip words.</summary>
    public required int[][] Mode7DoorFrames { get; init; }
}
