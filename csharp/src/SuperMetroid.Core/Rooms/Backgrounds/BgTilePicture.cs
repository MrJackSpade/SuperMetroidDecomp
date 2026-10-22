using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Where a pre-coloured image pictures one 8x8 tile (#1392): the image and the tile's top-left pixel in it.</summary>
/// <param name="Picture">The image picturing the tile.</param>
/// <param name="Corner">The tile's top-left pixel within <paramref name="Picture"/>.</param>
public readonly record struct BgTilePicture(SpriteSheetFrame Picture, Position16<ImagePixels> Corner);
