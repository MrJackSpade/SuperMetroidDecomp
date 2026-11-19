using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One part of a boss body as a gameplay frame shows it (#1466, #1514, #1540, #2031): a pre-coloured picture placed against
/// the body's placement, cut to the part of it that shows, as the acid cuts Crocomire's sinking body, and drawn whole at its
/// depth among the room's layers (#1634).
/// </summary>
/// <param name="Picture">The picture.</param>
/// <param name="Anchor">Where the picture's top-left corner lies from the body's placement.</param>
/// <param name="Size">How much of the picture shows, across and down from its top-left corner, in the picture's own pixels.</param>
/// <param name="Depth">The depth the picture is drawn at.</param>
/// <param name="Hidden">The picture's pixels that show nothing, as Crocomire's melt dissolve leaves them (#1652); null where all show.</param>
public sealed record ShownBossBodyPart(SpriteSheetFrame Picture, Offset16<RoomPixels> Anchor, Extent16<ImagePixels> Size,
    BossBodyDepth Depth, HiddenPicturePixels? Hidden = null);
