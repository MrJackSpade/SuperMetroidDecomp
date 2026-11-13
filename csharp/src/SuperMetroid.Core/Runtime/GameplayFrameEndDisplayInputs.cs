using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Display inputs read while the frame is on screen rather than at the frame boundary, sampled
/// at publication after the update: the door transition's layers and the per-line effects.
/// Arrays are owned copies; simulation cannot change them after publication.
/// </summary>
/// <param name="DoorTransitionShownLayers">The layers a door transition shows below the HUD, or null outside a door transition.</param>
/// <param name="ScrollingSky">The scrolling sky's drifting bands, when the room's sky bands drift.</param>
/// <param name="CrocomireMelt">How Crocomire's melt sags his body layer (#1468), while it runs.</param>
/// <param name="MotherBrainAscentShowing">Whether Mother Brain's rising-body display owns the gameplay layers.</param>
/// <param name="RainbowBeam">The shown rainbow beam's cone, or null while it is hidden; its colour comes from the frame-boundary latch.</param>
/// <param name="CeresHaze">The Ceres haze's per-line fixed-colour gradient, when the room spawned one.</param>
/// <param name="CeresHazeTints">The installed tints of the haze gradient, or null while no map presentation is bound; a haze that shows requires them.</param>
/// <param name="PowerBomb">Power-bomb glow window inputs; its glow colour comes from the frame-boundary latch.</param>
/// <param name="MessageBox">The message box overlay, which runs its own display loop.</param>
/// <param name="SuitPickupBeam">Suit pickup light-beam window description; its colour comes from the frame-boundary latch.</param>
/// <param name="Xray">X-ray beam phases and window cone inputs.</param>
/// <param name="TourianStatuesEnabled">Whether the Tourian statues' descent, the background's vertical scroll, is running.</param>
[IndependentFlag("independent display facts of the frame end")]
public sealed record GameplayFrameEndDisplayInputs(
    ScreenLayers? DoorTransitionShownLayers,
    ScrollingSkyLayer? ScrollingSky,
    CrocomireMeltDistortion? CrocomireMelt,
    bool MotherBrainAscentShowing,
    MotherBrainRainbowBeamRenderSnapshot? RainbowBeam,
    CeresHazeRenderSnapshot? CeresHaze,
    CeresHazeTints? CeresHazeTints,
    PowerBombRenderSnapshot PowerBomb,
    MessageBoxLayer? MessageBox,
    SamusSuitPickupBeam? SuitPickupBeam,
    SamusXrayDisplaySnapshot? Xray,
    bool TourianStatuesEnabled);
