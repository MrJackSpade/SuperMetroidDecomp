using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Editable door palette schema, separate from the indexed 4bpp character PNG and engine-owned door/escape timing; the elevator platform's strips are pictures of the room's floor (#1467).</summary>
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
}
