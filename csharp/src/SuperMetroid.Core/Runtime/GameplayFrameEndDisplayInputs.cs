using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx.Artwork;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Display inputs the cartridge reads while the frame is on screen rather than at NMI, sampled
/// at publication after the main loop: IRQ-handler register writes and HDMA tables in WRAM.
/// Arrays are owned copies; simulation cannot change them after publication.
/// </summary>
/// <param name="DoorTransitionMainScreenLayers">TM the door-transition IRQ handlers (<c>$80:96F1</c> and peers) write below the HUD, or null outside a door transition.</param>
/// <param name="ScrollingSkyHorizontalScrolls">The sky's per-gameplay-line BG2HOFS HDMA values before room shake, when the sky's HDMA object is configured.</param>
/// <param name="CrocomireMeltingScrolls">Crocomire's melting BG2VOFS HDMA table, while its channel is active.</param>
/// <param name="MotherBrainRisingHdmaActive">Whether Mother Brain's ascent HDMA owns the gameplay layers.</param>
/// <param name="RainbowBeam">Rainbow-beam window description; its fixed color comes from the NMI latch.</param>
/// <param name="CeresHaze">Ceres haze per-line COLDATA gradient, when the room spawned one.</param>
/// <param name="RoomFxPaletteBlends">Installed presentation colors for the haze gradient.</param>
/// <param name="PowerBomb">Power-bomb window table inputs; its fixed color comes from the NMI latch.</param>
/// <param name="MessageBox">Bank-$85 message overlay, which runs its own display loop.</param>
/// <param name="SuitPickupBeam">Suit pickup light-beam window description; its fixed color comes from the NMI latch.</param>
/// <param name="Xray">X-ray HDMA object phases and window cone inputs.</param>
/// <param name="TourianStatuesEnabled">Whether the Tourian statue BG2VOFS HDMA object is running.</param>
public sealed record GameplayFrameEndDisplayInputs(
    SnesMainScreenLayers? DoorTransitionMainScreenLayers,
    ImmutableArray<ushort>? ScrollingSkyHorizontalScrolls,
    ImmutableArray<ushort>? CrocomireMeltingScrolls,
    bool MotherBrainRisingHdmaActive,
    MotherBrainRainbowBeamRenderSnapshot? RainbowBeam,
    CeresHazeRenderSnapshot? CeresHaze,
    RoomFxPaletteBlendCatalog? RoomFxPaletteBlends,
    PowerBombRenderSnapshot PowerBomb,
    MessageBoxRenderLayer? MessageBox,
    SamusSuitPickupBeam? SuitPickupBeam,
    SamusXrayDisplaySnapshot? Xray,
    bool TourianStatuesEnabled);
