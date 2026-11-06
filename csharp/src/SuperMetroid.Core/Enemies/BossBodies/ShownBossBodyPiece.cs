using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One piece of a boss body as a gameplay frame shows it (#1466, #1514): a rectangle of the body's area that shows a
/// picture, the picture repeating across and down from the pixel it shows at the rectangle's top-left corner.
/// </summary>
/// <param name="Picture">The picture.</param>
/// <param name="Corner">The body pixel the rectangle's top-left corner lies on.</param>
/// <param name="Size">The rectangle's size.</param>
/// <param name="PictureCorner">The picture pixel the rectangle's top-left corner shows.</param>
public sealed record ShownBossBodyPiece(SpriteSheetFrame Picture, Position16<BackgroundPixels> Corner, Extent16<BackgroundPixels> Size,
    Position16<ImagePixels> PictureCorner);
