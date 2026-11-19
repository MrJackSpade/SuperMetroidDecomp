using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// A frame of a Ceres room that shows a perspective floor in place of the room's ordinary layers (#1407, #2498): the
/// floor and its map, and the band of the room's background Ceres Ridley's getaway shows below the floor (#1545).
/// </summary>
/// <param name="Hud">The HUD's images this frame boundary latched.</param>
/// <param name="PicturedHud"><paramref name="Hud"/>'s images as their pictures in the HUD's own sheet.</param>
/// <param name="Scrolls">The foreground's and the background's scrolls and the room shake.</param>
/// <param name="RoomFxOverlay">The room effect's overlay state, whose liquid moves the background's waves, or null while the room's effect shows nothing on the overlay.</param>
/// <param name="EffectOverlay">What the room effect shows on the effect overlay, its artwork resolved (#1706, #2505).</param>
/// <param name="MorphBallEyeBeam">The Morph Ball eye beam's cone as the frame ends, or null while no beam exists.</param>
/// <param name="Tints">The tints the frame's effects blend with, each published by its own effect.</param>
/// <param name="Floor">How the floor appears on the screen before room shake, and the pictures fixed in the room behind it.</param>
/// <param name="FloorMap">The room's turning floor the frame shows (#1349, #1467), its pictures resolved.</param>
/// <param name="BackgroundBand">
/// The room's background plane shown in a band below the floor, as Ridley's getaway shows it (#1545), or null where the
/// floor fills the gameplay view, as the elevator shaft's does.
/// </param>
public sealed record PerspectiveFloorFrameLatch(
    ShownHud Hud,
    PicturedHud PicturedHud,
    GameplayRenderSnapshot Scrolls,
    RoomFxOverlayRenderSnapshot? RoomFxOverlay,
    RoomEffectOverlay EffectOverlay,
    MorphBallEyeBeamCone? MorphBallEyeBeam,
    GameplayLatchedTints Tints,
    PerspectiveFloor Floor,
    AffineFloor FloorMap,
    RoomBackgroundLayer? BackgroundBand) : RoomFrameLatch(Hud, PicturedHud, Scrolls, RoomFxOverlay, EffectOverlay, MorphBallEyeBeam, Tints);
