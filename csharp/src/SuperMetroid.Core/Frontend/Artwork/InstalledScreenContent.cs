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
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Artwork;

/// <summary>
/// The installed content of the game's screens, scenes and HUD, as one immutable snapshot (#2203): the area maps and the
/// map screens' own content, the pause, file-select, options and game-over screens, the HUD, the escape timer and
/// typewriter, the title, the opening narration, the ending's text, and the colours and delays the gameplay scenes read.
/// Edits in place never alter exploration rules. It holds only what the game reads: the import keeps the source artwork,
/// pages and compositions those screens are pictured from to itself and never hands them to the game (#1466, #1583,
/// #1616). Each screen binds its own part.
/// </summary>
public sealed class InstalledScreenContent : IGameplayPresentationContent
{
    /// <summary>Captures the installed screens' content as one immutable snapshot.</summary>
    internal InstalledScreenContent(InstalledScreenContentParts parts)
    {
        Ensure.NotNull(parts);
        ContentIdentity = parts.ContentIdentity;
        Maps = parts.Maps;
        Backdrops = parts.Backdrops;
        PauseSelectors = parts.PauseSelectors;
        PauseReserveTanks = parts.PauseReserveTanks;
        EscapeTimer = parts.EscapeTimer;
        GameplayHud = parts.GameplayHud;
        GameOver = parts.GameOver;
        GameOptions = parts.GameOptions;
        FileSelect = parts.FileSelect;
        EscapeTypewriter = parts.EscapeTypewriter;
        IntroNarration = parts.IntroNarration;
        EndingText = parts.EndingText;
        TitleBackdrop = parts.TitleBackdrop;
        TitleGradient = parts.TitleGradient;
        MotherBrainRainbowBeamColours = parts.MotherBrainRainbowBeamColours;
        CeresHaze = parts.CeresHaze;
        PowerBombGlowColors = parts.PowerBombGlowColors;
        CeresRidleyGetawayBoostDelays = parts.CeresRidleyGetawayBoostDelays;
    }

    /// <summary>Uppercase SHA-256 identity of length-framed map rules, station masks, and installed presentation resources in fixed order; an edit in place changes the identity.</summary>
    public string ContentIdentity { get; }
    /// <summary>The area maps with their exploration rules, and the content the pause and file-select map screens draw them with.</summary>
    public AreaMapPresentation Maps { get; }
    /// <summary>The solid colour the pause, menu and world-map screens show where nothing is drawn (#1512).</summary>
    public MenuBackdrops Backdrops { get; }
    /// <summary>Pause selection-indicator artwork and presentation parameters; what is selected and owned is the game's, not the artwork's.</summary>
    public PauseSelectorPresentation PauseSelectors { get; }
    /// <summary>Reserve-strip artwork and drawing origins, independent of reserve energy and fill selection.</summary>
    public PauseReserveTankPresentation PauseReserveTanks { get; }
    /// <summary>Escape countdown visual layout and digit presentation.</summary>
    public EscapeTimerPresentation EscapeTimer { get; }
    /// <summary>Gameplay HUD picture and display presentation, separate from the HUD sheet's pictures.</summary>
    public GameplayHudPresentation GameplayHud { get; }
    /// <summary>Selected game-over screen presentation resources.</summary>
    public GameOverPresentation GameOver { get; }
    /// <summary>Selected options-menu artwork and display presentation; option values remain runtime state.</summary>
    public GameOptionsPresentation GameOptions { get; }
    /// <summary>Selected file-select screen presentation, separate from per-area maps and save-marker coordinates.</summary>
    public FileSelectPresentation FileSelect { get; }
    /// <summary>Selected escape-sequence typewriter text and visual presentation.</summary>
    public EscapeTypewriterPresentation EscapeTypewriter { get; }
    /// <summary>Opening narration text and presentation, separate from its selected font characters.</summary>
    public IntroNarrationPresentation IntroNarration { get; }
    /// <summary>The characters the ending's typewriter types, compiled by the import from the installed ending text (#1467).</summary>
    public EndingTypewriterText EndingText { get; }
    /// <summary>
    /// The colour the title shows behind its floor and objects (#1512); the title's other colours are pre-coloured into its
    /// sheets by the import.
    /// </summary>
    public RgbColor TitleBackdrop { get; }
    /// <summary>Title-screen gradient presentation, separate from its static colours and pictures.</summary>
    public TitleGradientPresentation TitleGradient { get; }
    /// <summary>Mother Brain's rainbow beam backdrop colours.</summary>
    public MotherBrainRainbowBeamColours MotherBrainRainbowBeamColours { get; }
    /// <summary>The Ceres haze's installed tints.</summary>
    public CeresHazeTints CeresHaze { get; }
    /// <summary>The glow colours Power Bomb, Crystal Flash, and Ceres explosions add to the scene, separate from Samus's suit colours.</summary>
    public InstalledPowerBombGlowColors PowerBombGlowColors { get; }

    /// <summary>The Speed Booster delays the getaway's wing frames leave behind for the unauthored cadence.</summary>
    public CeresRidleyGetawayBoostDelays CeresRidleyGetawayBoostDelays { get; }

    /// <summary>Returns an area's map with its stock exploration and station-reveal rules (<see cref="Maps"/>).</summary>
    /// <param name="area">One of the seven areas, including Ceres.</param>
    /// <returns>The immutable view compiled when this content was loaded.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not an area.</exception>
    public IAreaMapView Get(AreaId area) => Maps.Get(area);
}
