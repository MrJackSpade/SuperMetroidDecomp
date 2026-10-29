namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>One tile word a tile word sheet pictures for one set of values of its appearance inputs.</summary>
/// <param name="Tile">The tile word and the values it is pictured for.</param>
/// <param name="Frame">The frame picturing it.</param>
/// <param name="Corner">The tile's top-left pixel within the frame.</param>
public readonly record struct TileWordSheetTile(TileWordAppearance Tile, string Frame, Position16<ImagePixels> Corner);
