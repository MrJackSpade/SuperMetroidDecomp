using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Where a pre-coloured image pictures one 8x8 tile (#1392), as oracles lay the tile words the original game wrote: the
/// image and the tile's top-left pixel in it. The display model places whole pictures (#1514); only verification lays
/// them out cell by cell.
/// </summary>
/// <param name="Picture">The image picturing the tile.</param>
/// <param name="Corner">The tile's top-left pixel within <paramref name="Picture"/>.</param>
internal readonly record struct BgTilePicture(SpriteSheetFrame Picture, Position16<ImagePixels> Corner);
