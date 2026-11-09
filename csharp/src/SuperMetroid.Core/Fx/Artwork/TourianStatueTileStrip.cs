namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// Locates the four Tourian statues' pictures in the shared animated-tile strip (<see cref="TourianStatueTileStripDefinitions"/>):
/// the three oscillation pictures follow one another from the statue's first character, and the released picture
/// has its own.
/// </summary>
public static class TourianStatueTileStrip
{
    /// <summary>The size of the character artwork one picture of <paramref name="statue"/> shows.</summary>
    public static int PictureArtworkSize(AnimatedTileObject statue) => Window(statue).Tiles * TourianStatueTileStripDefinitions.CharacterArtworkSize;

    /// <summary>The windows of <paramref name="statue"/>.</summary>
    private static TourianStatueTileWindow Window(AnimatedTileObject statue) =>
        TourianStatueTileStripDefinitions.Windows.TryGetValue(statue, out TourianStatueTileWindow window)
            ? window : throw new InvalidDataException($"Unknown Tourian statue {statue}.");
}
