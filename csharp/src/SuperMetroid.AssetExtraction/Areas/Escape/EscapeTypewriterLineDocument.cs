using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Editable text and placement for a single escape-warning line.</summary>
public sealed record EscapeTypewriterLineDocument
{
    /// <summary>
    /// The layer pixel the line's first glyph's upper-left corner shows at, from the layer's upper-left corner; at least
    /// zero on both axes, with the whole line inside the layer when loaded.
    /// </summary>
    public required Position16<PlanePixels> Start { get; init; }
    /// <summary>Nonempty string of at most 32 spaces, uppercase A-Z letters, or exclamation marks; lowercase and other punctuation are rejected.</summary>
    public required string Text { get; init; }
}
