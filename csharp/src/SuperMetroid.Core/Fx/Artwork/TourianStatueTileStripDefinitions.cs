namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The shared strip of characters the four Tourian statues' animated tiles show (#1395): each statue's three
/// oscillation pictures and its released picture are windows of the one strip the room-FX atlas packs (#1415: by
/// character position, not by cartridge address).
/// </summary>
public static class TourianStatueTileStripDefinitions
{
    /// <summary>The characters in the strip.</summary>
    public const int TileCount = 96;

    /// <summary>The bytes of one two-bit character.</summary>
    public const int BytesPerTile = 16;

    /// <summary>The strip's length in bytes.</summary>
    public const int ByteCount = TileCount * BytesPerTile;


    /// <summary>The pictures each statue shows, each one window of the strip.</summary>
    public static IReadOnlyList<TourianStatueTilePicture> Pictures { get; } = Enum.GetValues<TourianStatueTilePicture>();

    /// <summary>The bytes one picture of <paramref name="statue"/> shows.</summary>
    public static int WindowByteCount(AnimatedTileObject statue) => Window(statue).Tiles * BytesPerTile;

    /// <summary>A statue's first oscillation picture's first character, its pictures' width in characters and its released picture's first character.</summary>
    private readonly record struct StatueWindow(int First, int Tiles, int Released);

    /// <summary>The strip character <paramref name="picture"/> of <paramref name="statue"/> starts at.</summary>
    public static int FirstTile(AnimatedTileObject statue, TourianStatueTilePicture picture)
    {
        StatueWindow window = Window(statue);
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
    private static StatueWindow Window(AnimatedTileObject statue) => statue switch
    {
        AnimatedTileObject.TourianStatuePhantoon => new(0, 8, 72),
        AnimatedTileObject.TourianStatueRidley => new(24, 4, 80),
        AnimatedTileObject.TourianStatueKraid => new(60, 4, 84),
        AnimatedTileObject.TourianStatueDraygon => new(36, 8, 88),
        AnimatedTileObject.None or
            AnimatedTileObject.Empty or
            AnimatedTileObject.HorizontalSpikes or
            AnimatedTileObject.VerticalSpikes or
            AnimatedTileObject.CrateriaLake or
            AnimatedTileObject.UnusedCrateriaLava or
            AnimatedTileObject.BrinstarPlant or
            AnimatedTileObject.WreckedShipScreen or
            AnimatedTileObject.MaridiaSandCeiling or
            AnimatedTileObject.MaridiaSandFalling or
            AnimatedTileObject.WreckedShipTreadmillRightwards or
            AnimatedTileObject.WreckedShipTreadmillLeftwards or
            AnimatedTileObject.Lava or
            AnimatedTileObject.Acid or
            AnimatedTileObject.Rain or
            AnimatedTileObject.Spores => throw new InvalidDataException($"Unknown Tourian statue {statue}."),
        _ => throw new InvalidDataException($"Unknown Tourian statue {statue}."),
    };
}
