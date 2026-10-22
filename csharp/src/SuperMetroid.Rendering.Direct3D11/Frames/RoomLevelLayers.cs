using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>The room layers a Direct3D 11 level buffer last uploaded.</summary>
/// <param name="Bg1">The bg1.</param>
/// <param name="Bg2">The bg2.</param>
/// <param name="Door">The door.</param>
/// <param name="Overlay">The overlay.</param>
/// <param name="Sky">The sky.</param>
/// <param name="Bg3">The bg3.</param>
/// <param name="Bg2Overlay">The bg2 overlay.</param>
/// <param name="Mode7">The mode7.</param>
/// <param name="BossBg2">The boss bg2.</param>
/// <param name="XrayReveal">The X-ray reveal BG2 shows, or null (#1392).</param>
internal readonly record struct RoomLevelLayers(RoomBackgroundLayer? Bg1, RoomBackgroundLayer? Bg2, RoomDoorBackground? Door, RoomTilemapOverlay? Overlay, SkyTilemap? Sky, Layer3Plane? Bg3, RoomTilemapOverlay? Bg2Overlay, Mode7MapSnapshot? Mode7, BossBg2Tilemap? BossBg2, XrayRevealPlane? XrayReveal);
