namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// The installed opening-narration font (#1409): how many glyph characters it holds. The installed intro font tile sheet
/// pictures them; their pixels belong to the import pipeline that reads the installed artwork.
/// </summary>
public class IntroFontAtlas
{
    /// <summary>Records how many glyph characters the installed font holds.</summary>
    /// <param name="characterCount">The installed font's glyph characters.</param>
    internal IntroFontAtlas(int characterCount) => CharacterCount = characterCount;

    /// <summary>The installed font's glyph characters.</summary>
    public int CharacterCount { get; }
}
