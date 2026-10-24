namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The installed font the credits, result screens and post-shot subtitle share (#1409): how many characters it holds.
/// The installed ending font tile sheets picture them; their pixels belong to the import pipeline that reads the
/// installed artwork.
/// </summary>
public class EndingFontAtlas
{
    /// <summary>Records how many characters the installed font holds.</summary>
    /// <param name="characterCount">The installed font's characters.</param>
    internal EndingFontAtlas(int characterCount) => CharacterCount = characterCount;

    /// <summary>The installed font's characters.</summary>
    public int CharacterCount { get; }
}
