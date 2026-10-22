using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// Everything one accepted (or forced-blank) NMI made PPU-visible: the register shadows and
/// effect latches of <c>$80:95A1..$95D0</c>, copied before the Samus graphics DMA. A lag NMI
/// leaves the previous latch in place. Arrays are owned copies; simulation cannot change them.
/// </summary>
/// <param name="Ppu">Ordinary BG scroll registers, room shake, and Draygon's BG2 window.</param>
/// <param name="Windows">Window and screen-selection bytes.</param>
/// <param name="SamusMode7Transform">Ceres elevator shaft Mode 7 matrix, when BGMODE is Mode 7.</param>
/// <param name="RoomLayer3Fx">Room-FX BG3 state.</param>
/// <param name="MorphBallEyeBeam">Morph Ball eye beam geometry and color.</param>
/// <param name="Phantoon">Phantoon's wave, blend, and mosaic latches, when Phantoon exists.</param>
/// <param name="TourianStatueVerticalOffset">Tourian statue BG2 vertical offset.</param>
/// <param name="Scene">Room-level BG mode, character-base, blend-selector, and sky shadows.</param>
/// <param name="Bosses">Boss-owned BG register shadows.</param>
/// <param name="FixedColors">COLDATA shadows.</param>
/// <param name="XrayReveal">
/// The X-ray reveal the NMI queue uploads for BG2 while X-ray is active, drawn from the room's block pictures (#1392).
/// </param>
/// <param name="Rooms">The room backgrounds this NMI displays, resolved from level data; null before the first room.</param>
public sealed record GameplayNmiDisplayLatch(
    GameplayPpuRenderSnapshot Ppu,
    GameplayWindowRegisterSnapshot Windows,
    SamusMode7Transform? SamusMode7Transform,
    RoomLayer3FxRenderSnapshot? RoomLayer3Fx,
    MorphBallEyeBeamRenderSnapshot? MorphBallEyeBeam,
    PhantoonDisplaySnapshot? Phantoon,
    short TourianStatueVerticalOffset,
    GameplayLatchedSceneRegisters Scene,
    GameplayLatchedBossBackgrounds Bosses,
    GameplayLatchedFixedColors FixedColors,
    XrayRevealPlane? XrayReveal,
    RoomBackgrounds? Rooms)
{
    /// <summary>Whether the latched BGMODE selects the Ceres elevator shaft's or Ridley getaway's Mode 7.</summary>
    public bool ShowsMode7 => Scene.CeresElevatorMode7 || Bosses.CeresRidleyMode7 is not null;
}
