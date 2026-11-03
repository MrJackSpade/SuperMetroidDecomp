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


    /// <summary>Crystal Flash body and bubble colors; timing, frame selection, and completion remain engine-owned.</summary>
    InstalledCrystalFlashColors CrystalFlashColors { get; }

}
