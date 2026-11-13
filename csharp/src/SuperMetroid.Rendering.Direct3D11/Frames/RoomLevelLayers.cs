using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>The room layers a Direct3D 11 level buffer last uploaded.</summary>
/// <param name="Foreground">The room's foreground plane, or null.</param>
/// <param name="Background">The room's background plane, or null.</param>
/// <param name="Door">The door.</param>
/// <param name="ForegroundText">The text an escape shows over the foreground, or null (#1403, #1555).</param>
/// <param name="Sky">The scrolling sky drawn from its image, or null (#1347, #1403).</param>
/// <param name="FxOverlay">The artwork the room effect's overlay shows, or null (#1514, #1706).</param>
/// <param name="BackgroundText">The text an escape shows over the background, or null (#1403, #1555).</param>
/// <param name="Floor">The turning floor map, or null.</param>
/// <param name="BossBody">The boss body shown, its parts' pictures layered in draw order, or null (#1540).</param>
/// <param name="XrayReveal">The X-ray reveal the background shows, or null (#1392).</param>
/// <param name="BackgroundImage">The room's library background drawn from its image, or null (#1392).</param>
/// <param name="MessagePicture">The image of the message box the frame shows, or null (#1392).</param>
/// <param name="Hud">The HUD's placed pictures the frame draws, or null (#1466).</param>
/// <param name="Planes">The pictured planes the frame draws, or null (#1467).</param>
/// <param name="Grids">The picture grids the frame draws, or null (#1467).</param>
/// <param name="HudColouring">How the HUD's pictures are coloured: gameplay's HUD shading, or the pause screen's own shading of each picture (#1512).</param>
/// <param name="FloorColouring">How the turning floor's pictures are coloured: the gameplay room's shading, or the shading a frontend screen's floor names (#1512).</param>
internal readonly record struct RoomLevelLayers(RoomBackgroundLayer? Foreground, RoomBackgroundLayer? Background, RoomDoorBackground? Door, RoomOverlayPictures? ForegroundText, SkyPictures? Sky, ShownFxOverlay? FxOverlay, RoomOverlayPictures? BackgroundText, AffineFloor? Floor, ShownBossBody? BossBody, XrayRevealPlane? XrayReveal, RoomBackgroundImagePlane? BackgroundImage, SuperMetroid.Core.Assets.SpriteSheetFrame? MessagePicture, PicturedHud? Hud, PicturedPlaneTables? Planes, PictureGridTables? Grids,
    D3D11PictureColouring HudColouring, D3D11PictureColouring FloorColouring);
