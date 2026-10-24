namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Editable text and overlay placement for a single escape-warning line.</summary>
public sealed record EscapeTypewriterLineDocument
{
    /// <summary>The overlay cell the line starts at, at least zero, with the whole line inside the overlay when loaded.</summary>
    public required int Cell { get; init; }
    /// <summary>Nonempty string of at most 32 spaces, uppercase A-Z letters, or exclamation marks; lowercase and other punctuation are rejected.</summary>
    public required string Text { get; init; }
}
