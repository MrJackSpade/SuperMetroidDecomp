using SuperMetroid.Core.Assets;
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
/// tiles and Samus palette inputs, plus the installed VRAM assets they upload. The frontend's complete presentation
/// catalog supplies it.
/// </summary>
public interface IGameplayPresentationContent : IAreaMapSource, IInstalledArtworkContent
{
    /// <summary>Standard gameplay HUD character transfers, including the four BG3 quarters restored after Kraid.</summary>
    HudTileAtlas HudTiles { get; }

    /// <summary>Gameplay HUD tilemap and display presentation, separate from the standard HUD character atlas.</summary>
    GameplayHudPresentation GameplayHud { get; }

    /// <summary>Selected character bytes for the two escape-timer VRAM transfers.</summary>
    EscapeTimerTileAtlas EscapeTimerTiles { get; }

    /// <summary>Escape countdown visual layout and digit presentation, separate from the character bytes in <see cref="EscapeTimerTiles"/>.</summary>
    EscapeTimerPresentation EscapeTimer { get; }

    /// <summary>Gameplay-message title text and tile presentation, separate from message panels and short notices.</summary>
    GameplayMessageTitlePresentation GameplayMessageTitles { get; }

    /// <summary>Gameplay-message panel artwork and layout, separate from title content and short notice presentation.</summary>
    GameplayMessagePanelPresentation GameplayMessagePanels { get; }

    /// <summary>Short gameplay-notice presentation, separate from full message titles and panels.</summary>
    GameplayMessageNoticePresentation GameplayMessageNotices { get; }

    /// <summary>Selected escape-sequence typewriter text and visual presentation.</summary>
    EscapeTypewriterPresentation EscapeTypewriter { get; }

    /// <summary>Mother Brain health-selected palette artwork, separate from rainbow and room-color families.</summary>
    MotherBrainHealthPalettePresentation MotherBrainHealthPalette { get; }

    /// <summary>Mother Brain rainbow, drain, revival, normal-restoration, and fake-death colors.</summary>
    MotherBrainRainbowPalettePresentation MotherBrainRainbowPalette { get; }

    /// <summary>Mother Brain arena color presentation, separate from boss health and rainbow palettes.</summary>
    MotherBrainRoomColorPresentation MotherBrainRoomColors { get; }

    /// <summary>Ceres Ridley colors, shared Norfair Ridley health colors, and private Baby drawing colors.</summary>
    CeresRidleyColorCatalog CeresRidleyColors { get; }

    /// <summary>Ceres Ridley Mode 7 zoom shades, separate from ordinary Ridley colors.</summary>
    CeresRidleyFloorColorCatalog CeresRidleyFloorColors { get; }

    /// <summary>Where the getaway's floor runs take their tile numbers from, which the floor updates it queues record.</summary>
    CeresRidleyFloorUpdateSources CeresRidleyFloorUpdates { get; }

    /// <summary>Room and cinematic palette-animation colors; instruction timing, CGRAM destinations, and side effects remain compiled.</summary>
    RoomPaletteFxPresentation RoomPaletteFx { get; }

    /// <summary>Selected layer-3 room-FX tilemaps, separate from animated characters and palette-blend colors.</summary>
    RoomFxLayer3Pages RoomFxLayer3Tilemaps { get; }

    /// <summary>Selected room-FX palette-blend presentation, separate from layer-3 tilemap geometry and animated character bytes.</summary>
    RoomFxBlendInks RoomFxPaletteBlends { get; }

    /// <summary>Selected animated room-FX character artwork, separate from layer-3 tilemaps and palette blending.</summary>
    RoomFxAnimatedTileAtlas RoomFxAnimatedTiles { get; }

    /// <summary>Fixed RGB5 colors for Power Bomb, Crystal Flash, and Ceres explosion effects, separate from Samus palette colors.</summary>
    PowerBombFixedColorCatalog PowerBombFixedColors { get; }

    /// <summary>Samus hurt-flash and cinematic-restoration color artwork.</summary>
    SamusHurtColorCatalog SamusHurtColors { get; }

    /// <summary>Crystal Flash body and bubble colors; native timing, frame selection, and completion remain engine-owned.</summary>
    CrystalFlashColorCatalog CrystalFlashColors { get; }

    /// <summary>Visor colors shared by X-ray and room palette cycling, separate from complete suit palettes.</summary>
    SamusVisorColorCatalog SamusVisorColors { get; }

    /// <summary>Base Power, Varia, and Gravity Suit colors, separate from transient full-body and charge cycles.</summary>
    SamusSuitColorCatalog SamusSuitColors { get; }

    /// <summary>Samus full-body palette-cycle inputs, separate from static suit, visor, and charged-beam colors.</summary>
    SamusFullBodyCycleColorCatalog SamusFullBodyCycleColors { get; }

    /// <summary>Charged-beam and pseudo-Screw-Attack palette inputs, separate from base suit colors.</summary>
    SamusChargeColorCatalog SamusChargeColors { get; }

    /// <summary>Samus Hyper Beam palette-cycle colors, separate from charged-beam presentation and other suit cycles.</summary>
    SamusHyperBeamColorCatalog SamusHyperBeamColors { get; }
}
