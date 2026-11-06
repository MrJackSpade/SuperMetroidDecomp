using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The staff credits and what follows them: the reward copyright and fades, the white flash and the post-credits logo.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingCreditsRoll
{
    /// <summary>Collected-item snapshot displayed with the final result.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Chooses the localized post-credits text asset.</summary>
    private readonly bool japaneseText;
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    private readonly EndingScreen screen ;
    /// <summary>Dynamic cinematic actors in slot traversal order.</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>The post-credits staging page.</summary>
    private readonly EndingTextPage postCreditsPage;
    /// <summary>What every ending scene draws on: brightness, the phase timer, the turning-floor view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;
    /// <summary>The ending's installed artwork: its text, font, staff credits, turning floor, object and palette assets.</summary>
    private readonly EndingArtwork _artwork;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingCreditsRoll(EndingInventorySnapshot inventory, bool japaneseText, EndingScreen screen, List<EndingSprite> sprites, EndingTextPage postCreditsPage, EndingStage stage, EndingArtwork artwork)
    {
        this.inventory = inventory;
        this.japaneseText = japaneseText;
        this.screen = screen;
        this.sprites = sprites;
        this.postCreditsPage = postCreditsPage;
        _stage = stage;
        _artwork = artwork;
    }

    /// <summary>Background star animation retained into the post-credits scene.</summary>
    internal EndingShootingStars? ShootingStars { get; set; }

    /// <summary>Post-credits reward jump and landing actor.</summary>
    internal EndingRewardJump? CurrentRewardJump { get; set; }

    /// <summary>Final shot-into-camera actor and its drawing state.</summary>
    internal EndingPostShot? CurrentPostShot { get; set; }

    /// <summary>Replaces planet actors with the staff scroll, shooting stars and blank reward staging page.</summary>
    internal void SetupCredits()
    {
        ShootingStars = new EndingShootingStars();
        // Func126 clears palette objects before installing credits/reward palettes.
        _stage.ResetPaletteFx();
        _artwork.LoadCreditsAndPostCreditsAssets();
        _artwork.CurrentCredits = new CreditsObjectState(CreditsRollDefinitions.ExpectedCompiledRows);
        postCreditsPage.Clear();
        sprites.Clear();
        _stage.Brightness = 15;
        _stage.Phase = EndingCreditsPhase.Credits;
    }

    /// <summary>Restores the post-credits palette except colors zero through three and holds a blank screen for sixty calls.</summary>
    internal void SetupPostCreditsBlank()
    {
        // The credits' end copies Intro4 colors 4-255, disables text glow, forces blank, and arms
        // function 129 for sixty calls. Preserve colors zero through three as the game does.
        _stage.LoadColours(EndingColourLoad.PostCredits);
        _stage.Brightness = 0;
        _stage.PhaseTimer = 60;
        _stage.Phase = EndingCreditsPhase.PostCreditsBlank;
    }

    /// <summary>Replaces the producer result rows with the copyright panel and starts its authored hold.</summary>
    internal void ShowRewardCopyright()
    {
        // Func135 removes the producer panel and shows the copyright panel in rows twelve and thirteen. The text page alone is
        // shown until Func137 ends.
        _ = _artwork.EndingText ?? throw new InvalidOperationException("Ending copyright requires installed text assets.");
        postCreditsPage.Erase(EndingPictureNames.ResultPanel);
        postCreditsPage.Write(EndingPictureNames.CopyrightPanel, EndingResultTextLayout.CopyrightPanelCell);
        RewardCopyrightShown = true;
        _stage.PhaseTimer = EndingSequenceDefinitions.Timing.CopyrightHoldFrames;
        _stage.Phase = EndingCreditsPhase.PostCreditsCopyright;
    }

    /// <summary>Fades the post-credits waiting backdrop toward its authored palette.</summary>
    internal void ApplyWaitingBackdropPalette(int step) => screen.Colours.Fade(EndingFade.WaitingBackdrop, step);

    /// <summary>Interpolation step for suitless or helmetless reward colors.</summary>
    private int rewardPaletteStep;

    /// <summary>Crossfades waiting colors into the selected non-armored reward palette.</summary>
    internal void ApplyRewardPalette()
    {
        if (_stage.EndingReward == EndingReward.Armored) return;
        ApplyWaitingBackdropPalette(EndingSequenceDefinitions.Timing.WaitingPaletteFadeFrames - rewardPaletteStep);
        screen.Colours.Fade(EndingFade.SuitlessSamus, rewardPaletteStep);
        if (_stage.EndingReward == EndingReward.Helmetless)
            screen.Colours.Fade(EndingFade.SuitedSamus, rewardPaletteStep);
        // E110 saves the post-credits colours as its target, then E158 runs the shared component accumulator 32 times.
        // Each increment is component<<3 in 8.8 precision and composing takes only its high byte, so step S of 32 shows
        // each target channel times S over 32, rounded down: the level the import pre-colours (#1512).
    }

    /// <summary>
    /// The text page the post-credits scenes show the staging page on (#1515): the result page, then the final page from
    /// the white flash on.
    /// </summary>
    internal EndingTextScreen PostCreditsTextPage { get; private set; } = EndingTextScreen.ResultPanel;

    /// <summary>The shown text page's height in cells, enlarged to two screens for the final page.</summary>
    internal int PostCreditsPageHeight { get; private set; } = 32;

    /// <summary>Fixed-color white component of the brief transition into the final logo, fading from 31 to zero, where it stays.</summary>
    internal int WhiteFlashColor { get; set; }

    /// <summary>The state of the ending logo, which exists while that phase runs.</summary>
    internal EndingLogo EndingLogo => _artwork.CurrentEndingLogo ??
        throw new InvalidOperationException("The ending logo is not running.");

    /// <summary>Restores the shooting palette, switches to the tall text map and begins the white transition.</summary>
    internal void BeginPostCreditsWhiteFlash()
    {
        CurrentPostShot = null;
        // E48A restores the shooting palette and shows the final text page: the final
        // text page is two screens tall, not the credits' single-screen page.
        _stage.LoadColours(EndingColourLoad.RestoredShot);
        PostCreditsTextPage = EndingTextScreen.FinalText;
        PostCreditsPageHeight = EndingPostShotDefinitions.FinalTextMapHeight;
        _stage.PostCreditsScroll = Position16<PlanePixels>.Zero;
        // E48A clears the copyright's two rows.
        postCreditsPage.Erase(EndingPictureNames.CopyrightPanel);
        WhiteFlashColor = EndingPostShotDefinitions.WhiteComponent;
        _stage.PhaseTimer = EndingPostShotDefinitions.FadeFrames;
        _stage.Phase = EndingCreditsPhase.PostCreditsWhiteFlash;
    }

    /// <summary>Shows the staging page on the final page's lower screen and installs the final logo actor.</summary>
    internal void FinishPostCreditsWhiteFlash()
    {
        // E504 copies the same staging page into the lower screen before the logo
        // actors enter. Subsequent percentage writes target the upper screen again.
        screen.ShowFinalLowerText(postCreditsPage.Written);
        CurrentRewardJump = null;
        sprites.Clear();
        _artwork.CurrentEndingLogo = new EndingLogo(_stage,
            () => _stage.PaletteFxSystem.SpawnDefinition(EndingLogoDefinitions.LandingPaletteFx, 0));
        _stage.Phase = EndingCreditsPhase.PostCreditsLogo;
    }

    /// <summary>Advances logo instructions and starts the item-percentage text once the logo completes.</summary>
    internal void StepPostCreditsLogo()
    {
        EndingLogo.Step(screen.Colours);
        if (!EndingLogo.Completed) return;
        _artwork.StartPostCreditsText(new EndingBackgroundTextState(new EndingBackgroundTextSetup { Page = postCreditsPage, Inventory = inventory, JapaneseText = japaneseText, Presentation = _artwork.EndingText, InstalledSequence = _artwork.EndingText is null ? null : SuperMetroid.Core.Frontend.Ending.Artwork.EndingTextSequence.ItemPercentage }));
        _stage.Phase = EndingCreditsPhase.ItemPercentage;
    }

    /// <summary>Whether the reward copyright is showing.</summary>
    internal bool RewardCopyrightShown { get; private set; }

    /// <summary>Starts the reward palette from its first step.</summary>
    internal void ResetRewardPalette() => rewardPaletteStep = 0;

    /// <summary>Moves the reward palette to its next step.</summary>
    internal void AdvanceRewardPalette() => rewardPaletteStep++;
}
