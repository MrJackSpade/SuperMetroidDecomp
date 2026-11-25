using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// What a frame boundary latches for a frame of a loaded room (#2498), whichever way the room shows: the camera's layer
/// scrolls and room shake, the HUD with its pictures, the room effect's overlay state and what it shows, the Morph Ball eye's beam and the
/// effects' tints. Every picture is resolved when the frame boundary latches it.
/// </summary>
/// <param name="Hud">The HUD's images this frame boundary latched, which the pause screen keeps (#1466).</param>
/// <param name="PicturedHud"><paramref name="Hud"/>'s images as their pictures in the HUD's own sheet, which the frame's HUD layer draws (#1466).</param>
/// <param name="Scrolls">The foreground's and the background's scrolls and the room shake.</param>
/// <param name="RoomFxOverlay">The room effect's overlay state, whose liquid moves the background's waves, or null while the room's effect shows nothing on the overlay.</param>
/// <param name="EffectOverlay">What the room effect shows on the effect overlay, its artwork resolved (#2505).</param>
/// <param name="MorphBallEyeBeam">The Morph Ball eye beam's cone as the frame ends: its eye's position and aim, its width and colour; null while no beam exists.</param>
/// <param name="Tints">The tints the frame's effects blend with, each published by its own effect.</param>
public abstract partial record RoomFrameLatch(
    ShownHud Hud,
    PicturedHud PicturedHud,
    GameplayRenderSnapshot Scrolls,
    RoomFxOverlayRenderSnapshot? RoomFxOverlay,
    RoomEffectOverlay EffectOverlay,
    MorphBallEyeBeamCone? MorphBallEyeBeam,
    GameplayLatchedTints Tints) : GameplayDisplayLatch(Hud)
{
    /// <summary>
    /// The frame's colours, latched with its scene (#3036): the room's shading under the display's fades and Samus's own
    /// colours on the sprite draws the boundary made visible, as the cartridge uploads an update's colours in the NMI that
    /// uploads its sprites and scrolls.
    /// </summary>
    public required FramePaint Colours { get; init; }
}
