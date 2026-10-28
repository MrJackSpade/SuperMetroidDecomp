using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>A placed tile over what shows at its character: what a pre-coloured picture of the tile pictures (#1392).</summary>
/// <param name="Tile">The placed tile.</param>
/// <param name="Shown">What shows at the tile's character.</param>
public readonly record struct BgTileArtwork(BackgroundTile Tile, BgArtworkShown Shown);
