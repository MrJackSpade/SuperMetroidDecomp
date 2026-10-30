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
/// Display inputs the cartridge reads while the frame is on screen rather than at NMI, sampled
/// at publication after the main loop: IRQ-handler register writes and HDMA tables in WRAM.
/// Arrays are owned copies; simulation cannot change them after publication.
/// </summary>
/// <param name="DoorTransitionMainScreenLayers">The layers a door transition shows below the HUD, or null outside a door transition.</param>
/// <param name="ScrollingSky">The scrolling sky's rows and parallax bands, when the room spawned the sky's band HDMA object.</param>
/// <param name="CrocomireMelt">How Crocomire's melt sags his body layer (#1468), while it runs.</param>
/// <param name="MotherBrainAscentShowing">Whether Mother Brain's rising-body display owns the gameplay layers.</param>
/// <param name="RainbowBeam">Rainbow-beam window description; its fixed color comes from the NMI latch.</param>
/// <param name="CeresHaze">The Ceres haze's per-line fixed-colour gradient, when the room spawned one.</param>
/// <param name="RoomFxPaletteBlends">Installed presentation colors for the haze gradient.</param>
/// <param name="PowerBomb">Power-bomb window table inputs; its fixed color comes from the NMI latch.</param>
/// <param name="MessageBox">Bank-$85 message overlay, which runs its own display loop.</param>
/// <param name="SuitPickupBeam">Suit pickup light-beam window description; its fixed color comes from the NMI latch.</param>
/// <param name="Xray">X-ray HDMA object phases and window cone inputs.</param>
/// <param name="TourianStatuesEnabled">Whether the Tourian statue BG2VOFS HDMA object is running.</param>
[IndependentFlag("independent display facts of the frame end")]
public sealed record GameplayFrameEndDisplayInputs(
    ScreenLayers? DoorTransitionMainScreenLayers,
    ScrollingSkyLayer? ScrollingSky,
    CrocomireMeltDistortion? CrocomireMelt,
    bool MotherBrainAscentShowing,
    MotherBrainRainbowBeamRenderSnapshot? RainbowBeam,
    CeresHazeRenderSnapshot? CeresHaze,
    RoomFxBlendInks? RoomFxPaletteBlends,
    PowerBombRenderSnapshot PowerBomb,
    MessageBoxLayer? MessageBox,
    SamusSuitPickupBeam? SuitPickupBeam,
    SamusXrayDisplaySnapshot? Xray,
    bool TourianStatuesEnabled);
