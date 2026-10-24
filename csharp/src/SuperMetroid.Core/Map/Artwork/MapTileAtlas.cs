namespace SuperMetroid.Core.Map.Artwork;

/// <summary>
/// The installed map-screen or pause-interface characters a menu shows (#1409): how many there are. The installed screen
/// tile sheets picture them; their pixels belong to the import pipeline that reads the installed artwork.
/// </summary>
public class MapTileAtlas
{
    /// <summary>Records how many characters the installed artwork holds.</summary>
    /// <param name="characterCount">The installed artwork's characters.</param>
    internal MapTileAtlas(int characterCount) => CharacterCount = characterCount;

    /// <summary>The installed artwork's characters.</summary>
    public int CharacterCount { get; }
}
