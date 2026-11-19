using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// A frame of a loaded room that shows the room's ordinary layers (#2498): the layers shown, the room's backgrounds with
/// their pictures resolved, the room's blending and sky, the boss's body, and the effects that change how the room's
/// layers show (Phantoon's body, Fireflea darkness, the Tourian statues' descent and the X-ray reveal).
/// </summary>
/// <param name="Hud">The HUD's images this frame boundary latched.</param>
/// <param name="PicturedHud"><paramref name="Hud"/>'s images as their pictures in the HUD's own sheet.</param>
/// <param name="Scrolls">The foreground's and the background's scrolls and the room shake.</param>
/// <param name="RoomFxOverlay">The room effect's overlay state, whose liquid moves the background's waves, or null while the room's effect shows nothing on the overlay.</param>
/// <param name="EffectOverlay">What the room effect shows on the effect overlay, its artwork resolved (#2505).</param>
/// <param name="MorphBallEyeBeam">The Morph Ball eye beam's cone as the frame ends, or null while no beam exists.</param>
/// <param name="Tints">The tints the frame's effects blend with, each published by its own effect.</param>
/// <param name="ShownLayers">The room's layers and the actors gameplay shows.</param>
/// <param name="Rooms">The room backgrounds this frame boundary displays, resolved from level data, the boss's body among them.</param>
/// <param name="Scene">The room effect's blending, the scrolling sky, and the room and boss the X-ray's blending reads.</param>
/// <param name="Bosses">The body a boss shows as the background, where it is placed and whether it shows (#2031).</param>
/// <param name="Phantoon">Phantoon's body wave, visibility and pixelation as the frame shows them, when Phantoon exists.</param>
/// <param name="Fireflea">The room's Fireflea darkness with the shade it subtracts, or null in a room without it.</param>
/// <param name="TourianStatueVerticalOffset">
/// How far the Tourian statues move the background vertically, in pixels, up or down: the background shows the foreground
/// camera's top row plus this offset, a signed row above the background's origin where the sum is negative (#2305). Null
/// outside the statue room, where the background keeps its own vertical scroll (#2385).
/// </param>
/// <param name="XrayReveal">The X-ray reveal background shows while X-ray is active, drawn from the room's block pictures (#1392).</param>
public sealed record OrdinaryRoomFrameLatch(
    ShownHud Hud,
    PicturedHud PicturedHud,
    GameplayRenderSnapshot Scrolls,
    RoomFxOverlayRenderSnapshot? RoomFxOverlay,
    RoomEffectOverlay EffectOverlay,
    MorphBallEyeBeamCone? MorphBallEyeBeam,
    GameplayLatchedTints Tints,
    ScreenLayers ShownLayers,
    RoomBackgrounds Rooms,
    GameplayLatchedScene Scene,
    GameplayLatchedBossBody Bosses,
    PhantoonDisplaySnapshot? Phantoon,
    LatchedFirefleaDarkness? Fireflea,
    int? TourianStatueVerticalOffset,
    XrayRevealPlane? XrayReveal) : RoomFrameLatch(Hud, PicturedHud, Scrolls, RoomFxOverlay, EffectOverlay, MorphBallEyeBeam, Tints);
