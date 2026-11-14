using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The room backgrounds one gameplay frame shows (#1346), resolved from level data with no stored picture grid: foreground from the room's
/// level plane, background from its background plane when that layer streams, the source room's screen during a door's opening
/// scroll, and the text an escape shows over either layer (#1555). A null <see cref="Background"/> leaves background to the sky's rows, a boss body
/// that owns background (#1352), or the picture its owner loaded (a library background or the X-ray reveal) until that
/// owner gets its own layer.
/// </summary>
/// <param name="Foreground">The room's level plane on the foreground.</param>
/// <param name="Background">The room's background plane on the background, or null when the background shows a loaded picture.</param>
/// <param name="FxOverlay">The room effect's artwork on the effect overlay (#1348, #1514, #1706), as its pictures (#1403); null until the frame boundary that latched it resolves it. The HUD is its own layer (#1466).</param>
/// <param name="DoorSource">The source room's foreground screen during a door's opening scroll, or null.</param>
/// <param name="ForegroundText">The text an escape shows over the foreground (Ceres's warning), or null (#1555).</param>
/// <param name="Sky">
/// The scrolling sky on the background when the room has no background plane, drawn from its image (#1347, #1403); null without a sky,
/// and until the frame boundary that latched the room resolves its pictures.
/// </param>
/// <param name="BackgroundText">The text an escape shows over the background (Zebes's warning), or null (#1348, #1555).</param>
/// <param name="Floor">The turning floor the room shows, or null when it shows none (#1349, #1467, #1649).</param>
/// <param name="BossBody">The boss body shown while a boss shows one, as its pictures placed where they show, or null (#1352, #1466, #1514).</param>
/// <param name="XrayReveal">The X-ray reveal background shows in place of the room's own background, or null (#1392).</param>
/// <param name="BackgroundImage">The room's library background drawn from its image, where background shows the picture it loaded (#1392).</param>
[ArchitectureReview("context-bag", "The background layers one gameplay frame shows, published to the display; every member is a layer's picture data, not a service.")]
public sealed record RoomBackgrounds(RoomBackgroundLayer Foreground, RoomBackgroundLayer? Background, FxOverlayArtwork? FxOverlay,
    RoomDoorBackground? DoorSource = null, RoomOverlayPictures? ForegroundText = null, SkyPictures? Sky = null,
    RoomOverlayPictures? BackgroundText = null, AffineFloor? Floor = null, ShownBossBody? BossBody = null,
    XrayRevealPlane? XrayReveal = null,
    RoomBackgroundImagePlane? BackgroundImage = null)
{
    /// <summary>The room effect's artwork the frame shows on the effect overlay (#1403, #1514, #1706).</summary>
    /// <exception cref="InvalidOperationException">The artwork is not yet resolved.</exception>
    public FxOverlayArtwork ShownFxOverlay => FxOverlay ?? throw new InvalidOperationException("The effect overlay is shown once the frame boundary that latched it resolves its artwork.");
}
