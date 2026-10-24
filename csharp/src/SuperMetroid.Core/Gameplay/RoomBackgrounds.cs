using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The room backgrounds one gameplay frame shows (#1346), resolved from level data with no tilemap: BG1 from the room's
/// level plane, BG2 from its layer-2 plane when that layer streams, the source room's screen during a door's opening
/// scroll, and words a room actor wrote over BG1. A null <see cref="Bg2"/> leaves BG2 to the sky's rows, a boss body
/// that owns BG2 (#1352), or the VRAM tilemap its owner loaded (a library background or the X-ray reveal) until that
/// owner gets its own layer.
/// </summary>
/// <param name="Bg1">The room's level plane on BG1.</param>
/// <param name="Bg2">The room's layer-2 plane on BG2, or null when BG2 shows a loaded tilemap.</param>
/// <param name="Bg3">The HUD and room effect plane on BG3 (#1348), as its cells' pictures (#1403); null until the accepted NMI that latched it resolves them.</param>
/// <param name="DoorSource">The source room's BG1 screen during a door's opening scroll, or null.</param>
/// <param name="Bg1Overlay">Words a room actor wrote over BG1, or null.</param>
/// <param name="Sky">
/// The scrolling sky on BG2 when the room has no layer-2 plane, drawn from its image (#1347, #1403); null without a sky,
/// and until the accepted NMI that latched the room resolves its pictures.
/// </param>
/// <param name="Bg2Overlay">Words a room actor wrote over BG2, or null (#1348).</param>
/// <param name="Mode7">The Mode 7 map a Ceres room shows, or null outside Ceres (#1349).</param>
/// <param name="BossBg2">A boss body's pictures on BG2 while the boss owns BG2, or null (#1352, #1403).</param>
/// <param name="Characters">Which character page BG1 and BG2 draw from (#1357).</param>
/// <param name="XrayReveal">The X-ray reveal BG2 shows in place of the room's own BG2, or null (#1392).</param>
/// <param name="Bg2Image">The room's library background drawn from its image, where BG2 shows the tilemap it loaded (#1392).</param>
[ArchitectureReview("context-bag", "The background layers one gameplay frame shows, published to the display; every member is a layer's picture data, not a service.")]
public sealed record RoomBackgrounds(RoomBackgroundLayer Bg1, RoomBackgroundLayer? Bg2, Layer3Cells? Bg3,
    RoomDoorBackground? DoorSource = null, RoomOverlayPictures? Bg1Overlay = null, SkyPictures? Sky = null,
    RoomOverlayPictures? Bg2Overlay = null, AffineFloor? Mode7 = null, BossBodyPictures? BossBg2 = null,
    RoomBackgroundCharacters Characters = RoomBackgroundCharacters.Tileset, XrayRevealPlane? XrayReveal = null,
    RoomBackgroundImagePlane? Bg2Image = null)
{
    /// <summary>The BG3 plane the frame shows (#1403).</summary>
    /// <exception cref="InvalidOperationException">The plane's pictures are not yet resolved.</exception>
    public Layer3Cells ShownBg3 => Bg3 ?? throw new InvalidOperationException("BG3 is shown once the accepted NMI that latched it resolves its pictures.");
}
