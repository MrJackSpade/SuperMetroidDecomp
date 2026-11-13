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
/// palette blends and Samus palette inputs. The frontend's complete presentation catalog supplies it.
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

    /// <summary>The Ceres haze's installed tints.</summary>
    CeresHazeTints CeresHaze { get; }

    /// <summary>The glow colours Power Bomb, Crystal Flash, and Ceres explosions add to the scene, separate from Samus palette colors.</summary>
    InstalledPowerBombGlowColors PowerBombGlowColors { get; }

}
