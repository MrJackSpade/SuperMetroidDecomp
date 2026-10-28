using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The scene one accepted (or forced-blank) frame shows (#1407): the camera's layer scrolls and room shake, the layers
/// shown, the Ceres perspective floor, the room's and the bosses' backgrounds, the blends' fixed colours and the
/// effects' latched state. Gameplay latches it before Samus's graphics are copied, so a change the main loop makes
/// shows from the next accepted frame. A lag frame leaves the previous latch in place. Arrays are owned copies;
/// simulation cannot change them.
/// </summary>
/// <param name="Scrolls">The camera's layer-1 and layer-2 scrolls and the room shake.</param>
/// <param name="MainScreen">The layers gameplay shows on the main screen.</param>
/// <param name="PerspectiveFloor">The Ceres perspective floor the frame shows in place of the ordinary layers, or null.</param>
/// <param name="RoomLayer3Fx">Room-FX BG3 state.</param>
/// <param name="MorphBallEyeBeam">The Morph Ball eye beam's cone as the frame ends: its eye's position and aim, its width and colour; null while no beam exists.</param>
/// <param name="Phantoon">Phantoon's wave, blend, and mosaic latches, when Phantoon exists.</param>
/// <param name="TourianStatueVerticalOffset">
/// Tourian statue BG2 vertical offset in pixels: the statue state's signed offset, which the capture adds to the
/// layer-one position with the scroll's own wrap.
/// </param>
/// <param name="Scene">The room's layer-3 characters, blending, sky and X-ray context.</param>
/// <param name="Bosses">The bodies bosses draw as layer 2.</param>
/// <param name="FixedColors">The fixed colours the frame's blends use.</param>
/// <param name="XrayReveal">
/// The X-ray reveal the NMI queue uploads for BG2 while X-ray is active, drawn from the room's block pictures (#1392).
/// </param>
/// <param name="Rooms">The room backgrounds this NMI displays, resolved from level data; null before the first room.</param>
/// <param name="Layer3">The BG3 plane's words this NMI latched, which its pictures are resolved from and the pause screen keeps; null before the first room.</param>
public sealed record GameplayDisplayLatch(
    GameplayRenderSnapshot Scrolls,
    ScreenLayers MainScreen,
    PerspectiveFloor? PerspectiveFloor,
    RoomLayer3FxRenderSnapshot? RoomLayer3Fx,
    MorphBallEyeBeamCone? MorphBallEyeBeam,
    PhantoonDisplaySnapshot? Phantoon,
    int TourianStatueVerticalOffset,
    GameplayLatchedScene Scene,
    GameplayLatchedBossBackgrounds Bosses,
    GameplayLatchedFixedColors FixedColors,
    XrayRevealPlane? XrayReveal,
    RoomBackgrounds? Rooms,
    Layer3Plane? Layer3 = null)
{
    /// <summary>Whether the frame shows a Ceres perspective floor instead of the room's ordinary layers.</summary>
    public bool ShowsPerspectiveFloor => PerspectiveFloor is not null;
}
