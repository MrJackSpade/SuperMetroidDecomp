namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// Locates the four Tourian statues' pictures in the shared animated-tile strip (<see cref="TourianStatueTileStripDefinitions"/>):
/// the three oscillation pictures follow one another from the statue's first character, and the released picture
/// has its own.
/// </summary>
public static class TourianStatueTileStrip
{
    /// <summary>The bytes one picture of <paramref name="statue"/> shows.</summary>
    public static int WindowByteCount(AnimatedTileObject statue) => Window(statue).Tiles * TourianStatueTileStripDefinitions.BytesPerTile;

    /// <summary>The strip character <paramref name="picture"/> of <paramref name="statue"/> starts at.</summary>
    public static int FirstTile(AnimatedTileObject statue, TourianStatueTilePicture picture)
    {
        TourianStatueTileWindow window = Window(statue);
        return picture switch
        {
            TourianStatueTilePicture.OscillationFirst => window.First,
            TourianStatueTilePicture.OscillationSecond => window.First + window.Tiles,
            TourianStatueTilePicture.OscillationThird => window.First + 2 * window.Tiles,
            TourianStatueTilePicture.Released => window.Released,
            _ => throw new InvalidDataException($"Undefined {nameof(TourianStatueTilePicture)} {picture}."),
        };
    }

    /// <summary>The windows of <paramref name="statue"/>.</summary>
    private static TourianStatueTileWindow Window(AnimatedTileObject statue) =>
        TourianStatueTileStripDefinitions.Windows.TryGetValue(statue, out TourianStatueTileWindow window)
            ? window : throw new InvalidDataException($"Unknown Tourian statue {statue}.");
}
