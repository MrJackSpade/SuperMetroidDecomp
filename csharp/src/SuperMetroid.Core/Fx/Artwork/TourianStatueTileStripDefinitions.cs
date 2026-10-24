namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The shared bank-$87 strip the four Tourian statues' animated tiles read (#1395): each statue's three oscillation
/// windows and its released picture are windows of the one strip the room-FX atlas packs.
/// </summary>
public static class TourianStatueTileStripDefinitions
{
    /// <summary>The strip's first source byte.</summary>
    public const int FirstSource = 0x879364;

    /// <summary>The source byte after the strip.</summary>
    public const int SourceEnd = 0x879964;

    /// <summary>The strip's length in bytes.</summary>
    public const int ByteCount = SourceEnd - FirstSource;

    /// <summary>The statues whose pictures are windows of the strip.</summary>
    public static IReadOnlyList<AnimatedTileObject> Statues { get; } =
    [
        AnimatedTileObject.TourianStatuePhantoon,
        AnimatedTileObject.TourianStatueRidley,
        AnimatedTileObject.TourianStatueKraid,
        AnimatedTileObject.TourianStatueDraygon,
    ];

    /// <summary>The pictures each statue shows, each one window of the strip.</summary>
    public static IReadOnlyList<TourianStatueTilePicture> Pictures { get; } = Enum.GetValues<TourianStatueTilePicture>();

    /// <summary>The bytes one picture of <paramref name="statue"/> transfers.</summary>
    public static int WindowByteCount(AnimatedTileObject statue) => Window(statue).Width;

    /// <summary>A statue's first oscillation window, the window width and its released picture.</summary>
    private readonly record struct StatueWindow(int First, int Width, int Released);

    /// <summary>The source address of <paramref name="picture"/> of <paramref name="statue"/>.</summary>
    public static int SourceAddress(AnimatedTileObject statue, TourianStatueTilePicture picture)
    {
        (int first, int width, int released) = Window(statue);
        int source = picture switch
        {
            TourianStatueTilePicture.OscillationFirst => first,
            TourianStatueTilePicture.OscillationSecond => first + width,
            TourianStatueTilePicture.OscillationThird => first + 2 * width,
            TourianStatueTilePicture.Released => released,
            _ => throw new InvalidDataException($"Undefined {nameof(TourianStatueTilePicture)} {picture}."),
        };
        return RoomFxRomData.Banks.AnimatedTiles | source;
    }

    /// <summary>The windows of <paramref name="statue"/>.</summary>
    private static StatueWindow Window(AnimatedTileObject statue) => statue switch
    {
        AnimatedTileObject.TourianStatuePhantoon => new(0x9364, 0x80, 0x97e4),
        AnimatedTileObject.TourianStatueRidley => new(0x94e4, 0x40, 0x9864),
        AnimatedTileObject.TourianStatueKraid => new(0x9724, 0x40, 0x98a4),
        AnimatedTileObject.TourianStatueDraygon => new(0x95a4, 0x80, 0x98e4),
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
