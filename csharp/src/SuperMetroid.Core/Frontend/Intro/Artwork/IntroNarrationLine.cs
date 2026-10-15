namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Defines one line of opening-narration text and its tilemap row.</summary>
public sealed record IntroNarrationLine
{
    /// <summary>Even tilemap row from the first through last supported text row.</summary>
    public required int Row { get; init; }
    /// <summary>Supported narration glyphs, bounded to the page's available columns.</summary>
    public required string Text { get; init; }
}
