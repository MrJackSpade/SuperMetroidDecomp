using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>The room layers a Direct3D 11 level buffer last uploaded.</summary>
/// <param name="Bg1">The bg1.</param>
/// <param name="Bg2">The bg2.</param>
/// <param name="Door">The door.</param>
/// <param name="Overlay">What room actors wrote over BG1, or null (#1403).</param>
/// <param name="Sky">The scrolling sky drawn from its image, or null (#1347, #1403).</param>
/// <param name="Bg3">The bg3.</param>
/// <param name="Bg2Overlay">What room actors wrote over BG2, or null (#1403).</param>
/// <param name="Mode7">The mode7.</param>
/// <param name="BossBg2">The boss bg2.</param>
/// <param name="XrayReveal">The X-ray reveal BG2 shows, or null (#1392).</param>
/// <param name="Bg2Image">The room's library background drawn from its image, or null (#1392).</param>
/// <param name="MessagePicture">The image of the message box the frame shows, or null (#1392).</param>
/// <param name="Screens">The frontend layers' character tiles the frame draws, or null (#1392).</param>
internal readonly record struct RoomLevelLayers(RoomBackgroundLayer? Bg1, RoomBackgroundLayer? Bg2, RoomDoorBackground? Door, RoomOverlayPictures? Overlay, SkyPictures? Sky, Layer3Cells? Bg3, RoomOverlayPictures? Bg2Overlay, AffineFloor? Mode7, BossBodyPictures? BossBg2, XrayRevealPlane? XrayReveal, RoomBackgroundImagePlane? Bg2Image, SuperMetroid.Core.Assets.SpriteSheetFrame? MessagePicture, ScreenPictureTables? Screens);
