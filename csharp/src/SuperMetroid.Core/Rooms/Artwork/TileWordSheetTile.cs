namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>One tile word a tile word sheet pictures over one artwork.</summary>
/// <param name="Tile">The tile word and what shows at its character.</param>
/// <param name="Frame">The frame picturing it.</param>
/// <param name="Corner">The tile's top-left pixel within the frame.</param>
public readonly record struct TileWordSheetTile(BgTileArtwork Tile, string Frame, Position16<ImagePixels> Corner);
