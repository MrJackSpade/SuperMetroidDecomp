namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// The installed world-map character atlases a menu shows (#1409): how many foreground and background characters they
/// hold. The installed world-map tile sheets picture them; their pixels belong to the import pipeline that reads the
/// installed artwork.
/// </summary>
public class WorldMapArtwork
{
    /// <summary>Records how many characters each installed atlas holds.</summary>
    /// <param name="foregroundCharacterCount">The installed foreground atlas's characters.</param>
    /// <param name="backgroundCharacterCount">The installed background atlas's characters.</param>
    internal WorldMapArtwork(int foregroundCharacterCount, int backgroundCharacterCount)
    {
        ForegroundCharacterCount = foregroundCharacterCount;
        BackgroundCharacterCount = backgroundCharacterCount;
    }

    /// <summary>The installed foreground atlas's 4-bpp characters.</summary>
    public int ForegroundCharacterCount { get; }

    /// <summary>The installed background atlas's 2-bpp characters.</summary>
    public int BackgroundCharacterCount { get; }
}
