using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The scene one shown (or loading) frame shows (#1407): the camera's layer scrolls and room shake, the layers
/// shown, the Ceres perspective floor, the room's and the bosses' backgrounds, the blends' fixed colours and the
/// effects' latched state. Gameplay latches it before Samus's graphics are copied, so a change the main loop makes
/// shows from the next shown frame. An update without a frame boundary leaves the previous latch in place. Arrays are owned copies;
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
/// The X-ray reveal BG2 shows while X-ray is active, drawn from the room's block pictures (#1392).
/// </param>
/// <param name="Rooms">The room backgrounds this frame boundary displays, resolved from level data; null before the first room.</param>
/// <param name="Layer3">The room effect's BG3 plane this frame boundary latched, which its pictures are resolved from; null before the first room.</param>
/// <param name="Hud">The HUD's images this frame boundary latched, which the pause screen keeps (#1466).</param>
/// <param name="PicturedHud">
/// <paramref name="Hud"/>'s images as their pictures in the room's BG3 sheet, which the frame's HUD layer draws (#1466); null
/// until the frame boundary that latched them resolves them, and before the first room.
/// </param>
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
    Layer3Plane? Layer3 = null,
    ShownHud? Hud = null,
    PicturedHud? PicturedHud = null)
{
    /// <summary>Whether the frame shows a Ceres perspective floor instead of the room's ordinary layers.</summary>
    public bool ShowsPerspectiveFloor => PerspectiveFloor is not null;
}
