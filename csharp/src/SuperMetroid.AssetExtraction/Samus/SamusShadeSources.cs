using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The installed colour tables Samus's sheets are pre-coloured from (#1512), loaded once for all her sheets.</summary>
/// <param name="Starting">Gameplay's starting colours, which hold her suit's and beams' before any is loaded.</param>
/// <param name="IntroSuit">Her suit's colours in the opening's own colours.</param>
/// <param name="IntroBeams">Her beams' colours in the opening's own colours.</param>
/// <param name="Suits">The three suits' normal colours.</param>
/// <param name="Charge">The charged-beam and pseudo Screw Attack colours.</param>
/// <param name="HyperBeamSuit">The Hyper Beam's full-body glow.</param>
/// <param name="FullBodyCycles">The Speed Booster, Screw Attack and shinespark cycles.</param>
/// <param name="SpeedBoosterOverrun">The colours Gravity's Speed Booster copies past its cycle.</param>
/// <param name="Hurt">The hurt flash's and the cinematic restoration's colours.</param>
/// <param name="Visor">The visor cycle's shades.</param>
/// <param name="BeamPalettes">The retail beam combinations' colours.</param>
/// <param name="Chainsaw">The glitched Chainsaw beam's colours.</param>
/// <param name="CrystalFlash">The Crystal Flash's body and bubble colours.</param>
/// <param name="HyperBeamCycle">The Hyper Beam's own beam colour cycle.</param>
/// <param name="Death">The death sequence's suited, suitless and whiteout colours.</param>
internal sealed record SamusShadeSources(GameplayStartingColours Starting, RgbColor[] IntroSuit, RgbColor[] IntroBeams,
    InstalledSamusSuitColors Suits, SamusChargeColours Charge, SamusHyperBeamColours HyperBeamSuit,
    SamusFullBodyCycleColours FullBodyCycles, SpeedBoosterOverrunColours<SpeedBoosterPaletteOverrun, RgbColor> SpeedBoosterOverrun,
    InstalledSamusHurtColors Hurt, InstalledSamusVisorColors Visor, InstalledBeamPalettes BeamPalettes,
    ChainsawBeamColours<RgbColor> Chainsaw, InstalledCrystalFlashColors CrystalFlash, HyperBeamFxColours HyperBeamCycle,
    InstalledSamusDeathPalettes Death);
