namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Editable text and linear VRAM placement for a single escape-warning line.</summary>
public sealed record EscapeTypewriterLineDocument
{
    /// <summary>Starting VRAM word address, at least zero, with <c>Destination + Text.Length</c> no greater than $8000 when loaded.</summary>
    public required int Destination { get; init; }
    /// <summary>Nonempty string of at most 32 spaces, uppercase A-Z letters, or exclamation marks; lowercase and other punctuation are rejected.</summary>
    public required string Text { get; init; }
}
