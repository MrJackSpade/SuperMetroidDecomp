using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One part of a boss body as a gameplay frame shows it (#1466, #1514, #1540): a pre-coloured picture placed on the body's
/// layer, cut to the part of it that shows, as the acid cuts Crocomire's sinking body.
/// </summary>
/// <param name="Picture">The picture.</param>
/// <param name="Corner">The body pixel the picture's top-left corner lies on.</param>
/// <param name="Size">How much of the picture shows, across and down from its top-left corner.</param>
public sealed record ShownBossBodyPart(SpriteSheetFrame Picture, Position16<BackgroundPixels> Corner, Extent16<BackgroundPixels> Size);
