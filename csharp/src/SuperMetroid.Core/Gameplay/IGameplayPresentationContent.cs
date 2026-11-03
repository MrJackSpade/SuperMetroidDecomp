using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The installed presentation content that gameplay reads: area maps, HUD and escape artwork, boss palettes, room-FX
/// tiles and Samus palette inputs. The frontend's complete presentation catalog supplies it.
/// </summary>
public interface IGameplayPresentationContent : IAreaMapSource
{
    /// <summary>The gameplay HUD's layout, which places its installed images (#1466).</summary>
    GameplayHudPresentation GameplayHud { get; }

    /// <summary>Escape countdown visual layout and digit presentation.</summary>
    EscapeTimerPresentation EscapeTimer { get; }

    /// <summary>Selected escape-sequence typewriter text and visual presentation.</summary>
    EscapeTypewriterPresentation EscapeTypewriter { get; }

    /// <summary>Mother Brain rainbow, drain, revival, normal-restoration, and fake-death colors.</summary>
    MotherBrainRainbowPalettePresentation MotherBrainRainbowPalette { get; }

    /// <summary>The Speed Booster delays the getaway's wing frames leave behind for the unauthored cadence.</summary>
    CeresRidleyGetawayBoostDelays CeresRidleyGetawayBoostDelays { get; }

    /// <summary>Room and cinematic palette-animation colors; step timing, colour destinations, and side effects remain compiled.</summary>
    RoomPaletteFxPresentation RoomPaletteFx { get; }

    /// <summary>Selected room-FX palette-blend presentation, separate from layer-3 tilemap geometry and animated character bytes.</summary>
    RoomFxBlendInks RoomFxPaletteBlends { get; }

    /// <summary>Selected animated room-FX character artwork, separate from layer-3 tilemaps and palette blending.</summary>
    RoomFxAnimatedTileAtlas RoomFxAnimatedTiles { get; }

    /// <summary>Fixed RGB5 colors for Power Bomb, Crystal Flash, and Ceres explosion effects, separate from Samus palette colors.</summary>
    InstalledPowerBombFixedColors PowerBombFixedColors { get; }

    /// <summary>Samus hurt-flash and cinematic-restoration color artwork.</summary>
    InstalledSamusHurtColors SamusHurtColors { get; }

    /// <summary>Crystal Flash body and bubble colors; timing, frame selection, and completion remain engine-owned.</summary>
    InstalledCrystalFlashColors CrystalFlashColors { get; }

    /// <summary>Visor colors shared by X-ray and room palette cycling, separate from complete suit palettes.</summary>
    InstalledSamusVisorColors SamusVisorColors { get; }

    /// <summary>Base Power, Varia, and Gravity Suit colors, separate from transient full-body and charge cycles.</summary>
    InstalledSamusSuitColors SamusSuitColors { get; }

    /// <summary>Samus full-body palette-cycle inputs, separate from static suit, visor, and charged-beam colors.</summary>
    SamusFullBodyCycleColours SamusFullBodyCycleColors { get; }

    /// <summary>Charged-beam and pseudo-Screw-Attack palette inputs, separate from base suit colors.</summary>
    SamusChargeColours SamusChargeColors { get; }

    /// <summary>Samus Hyper Beam palette-cycle colors, separate from charged-beam presentation and other suit cycles.</summary>
    SamusHyperBeamColours SamusHyperBeamColors { get; }
}
