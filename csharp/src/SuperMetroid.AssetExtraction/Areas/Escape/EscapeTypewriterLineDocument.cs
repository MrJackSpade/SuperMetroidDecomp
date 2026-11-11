namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Editable text and placement for a single escape-warning line.</summary>
public sealed record EscapeTypewriterLineDocument
{
    /// <summary>
    /// The pixel column, from the layer's left, the line's first glyph's left edge shows at; at least zero, with the whole
    /// line inside the layer when loaded.
    /// </summary>
    public required int X { get; init; }
    /// <summary>The pixel row, from the layer's top, the line's glyphs' top edge shows at; at least zero, with the glyphs inside the layer when loaded.</summary>
    public required int Y { get; init; }
    /// <summary>Nonempty string of at most 32 spaces, uppercase A-Z letters, or exclamation marks; lowercase and other punctuation are rejected.</summary>
    public required string Text { get; init; }
}
