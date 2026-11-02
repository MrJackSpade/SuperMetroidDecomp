namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The installed escape and planet-explosion backdrops (#1409), which the ending requires bound before it shows them.
/// The ending draws their installed floor sheets; the backdrops' maps and characters belong to the import pipeline that
/// reads the installed artwork.
/// </summary>
public abstract class EndingFloorArtwork
{
    /// <summary>Marks the backdrops as read from installed artwork.</summary>
    internal EndingFloorArtwork()
    {
    }
}
