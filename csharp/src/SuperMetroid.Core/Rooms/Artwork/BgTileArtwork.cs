namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>A tile word over what shows at its character: what a pre-coloured picture of the tile pictures (#1392).</summary>
/// <param name="Word">The tile word.</param>
/// <param name="Shown">What shows at the word's character.</param>
public readonly record struct BgTileArtwork(SnesBgTilemapWord Word, BgArtworkShown Shown);
