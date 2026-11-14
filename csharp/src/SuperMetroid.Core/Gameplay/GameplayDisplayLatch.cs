using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The scene one shown (or loading) frame shows (#1407): the camera's layer scrolls and room shake, the layers
/// shown, the Ceres perspective floor, the room's and the bosses' backgrounds, the effects' tints and the
/// effects' latched state. Gameplay latches it at the frame boundary, before the update's controller latch, so a change
/// the main loop makes shows from the next shown frame. An update without a frame boundary leaves the previous latch in place. Arrays are owned copies;
/// simulation cannot change them.
/// </summary>
/// <param name="Scrolls">The foreground's and the background's scrolls and the room shake.</param>
/// <param name="ShownLayers">The room's layers and the actors gameplay shows.</param>
/// <param name="PerspectiveFloor">The Ceres perspective floor the frame shows in place of the ordinary layers, or null.</param>
/// <param name="RoomFxOverlay">Room-FX effect overlay state.</param>
/// <param name="MorphBallEyeBeam">The Morph Ball eye beam's cone as the frame ends: its eye's position and aim, its width and colour; null while no beam exists.</param>
/// <param name="Phantoon">Phantoon's body wave, visibility and pixelation as the frame shows them, when Phantoon exists.</param>
/// <param name="TourianStatueVerticalOffset">
/// How far the Tourian statues move the background vertically, in pixels, up or down: the background shows the foreground
/// camera's top row plus this offset, wrapping as room positions do.
/// </param>
/// <param name="Scene">The room effect's blending, the scrolling sky, and the room and boss the X-ray's blending reads.</param>
/// <param name="Bosses">The bodies bosses draw as the background.</param>
/// <param name="Tints">The tints the frame's effects blend with, each published by its own effect.</param>
/// <param name="XrayReveal">
/// The X-ray reveal background shows while X-ray is active, drawn from the room's block pictures (#1392).
/// </param>
/// <param name="Rooms">The room backgrounds this frame boundary displays, resolved from level data; null before the first room.</param>
/// <param name="FxOverlay">The room effect's effect overlay plane this frame boundary latched, which its pictures are resolved from; null before the first room.</param>
/// <param name="Hud">The HUD's images this frame boundary latched, which the pause screen keeps (#1466).</param>
/// <param name="PicturedHud">
/// <paramref name="Hud"/>'s images as their pictures in the room's effect overlay sheet, which the frame's HUD layer draws (#1466); null
/// until the frame boundary that latched them resolves them, and before the first room.
/// </param>
public sealed record GameplayDisplayLatch(
    GameplayRenderSnapshot Scrolls,
    ScreenLayers ShownLayers,
    PerspectiveFloor? PerspectiveFloor,
    RoomFxOverlayRenderSnapshot? RoomFxOverlay,
    MorphBallEyeBeamCone? MorphBallEyeBeam,
    PhantoonDisplaySnapshot? Phantoon,
    int TourianStatueVerticalOffset,
    GameplayLatchedScene Scene,
    GameplayLatchedBossBackgrounds Bosses,
    GameplayLatchedTints Tints,
    XrayRevealPlane? XrayReveal,
    RoomBackgrounds? Rooms,
    FxOverlayPlane? FxOverlay = null,
    ShownHud? Hud = null,
    PicturedHud? PicturedHud = null)
{
    /// <summary>Whether the frame shows a Ceres perspective floor instead of the room's ordinary layers.</summary>
    public bool ShowsPerspectiveFloor => PerspectiveFloor is not null;
}
