using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Frontend.Artwork;

/// <summary>The installed screens' content an <see cref="InstalledScreenContent"/> snapshot is built from (#633, #2203).</summary>
internal sealed record InstalledScreenContentParts
{
    /// <summary>Uppercase SHA-256 identity of length-framed map rules, station masks, and installed presentation resources in fixed order; an edit in place changes the identity.</summary>
    public required string ContentIdentity { get; init; }

    /// <summary>The area maps with their exploration rules, and the content the map screens draw them with.</summary>
    public required AreaMapPresentation Maps { get; init; }

    /// <summary>The solid colour the pause, menu and world-map screens show where nothing is drawn (#1512).</summary>
    public required MenuBackdrops Backdrops { get; init; }

    /// <summary>Pause selection-indicator artwork and presentation parameters; inventory and selection logic remain engine-owned.</summary>
    public required PauseSelectorPresentation PauseSelectors { get; init; }

    /// <summary>Reserve-strip artwork and drawing origins, independent of reserve energy and fill selection.</summary>
    public required PauseReserveTankPresentation PauseReserveTanks { get; init; }

    /// <summary>Escape countdown visual layout and digit presentation.</summary>
    public required EscapeTimerPresentation EscapeTimer { get; init; }

    /// <summary>Gameplay HUD picture and display presentation, separate from the HUD sheet's pictures.</summary>
    public required GameplayHudPresentation GameplayHud { get; init; }

    /// <summary>Selected game-over screen presentation resources.</summary>
    public required GameOverPresentation GameOver { get; init; }

    /// <summary>Selected options-menu artwork and display presentation; option values remain runtime state.</summary>
    public required GameOptionsPresentation GameOptions { get; init; }

    /// <summary>Selected file-select screen presentation, separate from per-area maps and save-marker coordinates.</summary>
    public required FileSelectPresentation FileSelect { get; init; }

    /// <summary>Selected escape-sequence typewriter text and visual presentation.</summary>
    public required EscapeTypewriterPresentation EscapeTypewriter { get; init; }

    /// <summary>Opening narration text and presentation, separate from its selected font characters.</summary>
    public required IntroNarrationPresentation IntroNarration { get; init; }

    /// <summary>The characters the ending's typewriter types, compiled by the import from the installed ending text (#1467).</summary>
    public required EndingTypewriterText EndingText { get; init; }

    /// <summary>The colour the title shows behind its floor and objects (#1512).</summary>
    public required RgbColor TitleBackdrop { get; init; }

    /// <summary>Title-screen gradient presentation, separate from its static colours and pictures.</summary>
    public required TitleGradientPresentation TitleGradient { get; init; }

    /// <summary>Mother Brain's rainbow beam backdrop colours.</summary>
    public required MotherBrainRainbowBeamColours MotherBrainRainbowBeamColours { get; init; }

    /// <summary>The Ceres haze's installed tints.</summary>
    public required CeresHazeTints CeresHaze { get; init; }

    /// <summary>The glow colours Power Bomb, Crystal Flash, and Ceres explosions add to the scene, separate from Samus's suit colours.</summary>
    public required InstalledPowerBombGlowColors PowerBombGlowColors { get; init; }

    /// <summary>The Speed Booster delays the getaway's wing frames leave behind for the unauthored cadence.</summary>
    public required CeresRidleyGetawayBoostDelays CeresRidleyGetawayBoostDelays { get; init; }
}
