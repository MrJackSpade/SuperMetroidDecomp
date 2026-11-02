namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The shared strip of characters the four Tourian statues' animated tiles show (#1395): each statue's three
/// oscillation pictures and its released picture are windows of the one strip the room-FX atlas packs (#1415: by
/// character position).
/// </summary>
public static class TourianStatueTileStripDefinitions
{
    /// <summary>The characters in the strip.</summary>
    public const int TileCount = 96;

    /// <summary>The bytes of one two-bit character.</summary>
    public const int BytesPerTile = 16;

    /// <summary>
    /// Each statue's windows of the strip: its first oscillation picture's first character, its pictures' width in
    /// characters and its released picture's first character (<see cref="TourianStatueTileStrip"/>).
    /// </summary>
    internal static readonly IReadOnlyDictionary<AnimatedTileObject, TourianStatueTileWindow> Windows =
        new Dictionary<AnimatedTileObject, TourianStatueTileWindow>
        {
            [AnimatedTileObject.TourianStatuePhantoon] = new(0, 8, 72),
            [AnimatedTileObject.TourianStatueRidley] = new(24, 4, 80),
            [AnimatedTileObject.TourianStatueKraid] = new(60, 4, 84),
            [AnimatedTileObject.TourianStatueDraygon] = new(36, 8, 88),
        };
}
