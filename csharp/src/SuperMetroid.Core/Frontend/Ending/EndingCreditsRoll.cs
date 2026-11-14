using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The staff credits and what follows them: the reward copyright and fades, the white flash and the post-credits logo.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
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
    /// <summary>What every ending scene draws on: brightness, the phase timer, the turning-floor view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;
    /// <summary>The ending's installed artwork: its text, font, staff credits, turning floor, object and palette assets.</summary>
    private readonly EndingArtwork _artwork;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingCreditsRoll(EndingInventorySnapshot inventory, bool japaneseText, EndingScreen screen, List<EndingSprite> sprites, EndingStage stage, EndingArtwork artwork)
    {
        this.inventory = inventory;
        this.japaneseText = japaneseText;
        this.screen = screen;
        this.sprites = sprites;
        _stage = stage;
        _artwork = artwork;
    }

    /// <summary>Background star animation retained into the post-credits scene.</summary>
    internal EndingShootingStars? ShootingStars { get; set; }

    /// <summary>Post-credits reward jump and landing actor.</summary>
    internal EndingRewardJump? CurrentRewardJump { get; set; }

    /// <summary>Final shot-into-camera actor and its drawing state.</summary>
    internal EndingPostShot? CurrentPostShot { get; set; }

    /// <summary>Replaces planet actors with the staff scroll, shooting stars and no post-credits text.</summary>
    internal void SetupCredits()
    {
        ShootingStars = new EndingShootingStars();
        // The credits' setup stops every colour effect before loading the credits' and rewards' colours.
        _stage.ResetColourEffects();
        _artwork.LoadCreditsAndPostCreditsAssets();
        _artwork.CurrentCredits = new CreditsObjectState();
        _artwork.PostCreditsText.Clear();
        sprites.Clear();
        _stage.Brightness = 15;
        _stage.Phase = EndingCreditsPhase.Credits;
    }

    /// <summary>Shows the post-credits scenes' own colours (#1544) and holds a blank screen for sixty calls.</summary>
    internal void SetupPostCreditsBlank()
    {
        // The credits' end starts the post-credits scenes in their own colours, forces blank and holds for sixty calls.
        _stage.LoadColours(EndingColourLoad.PostCredits);
        _stage.Brightness = 0;
        _stage.PhaseTimer = 60;
        _stage.Phase = EndingCreditsPhase.PostCreditsBlank;
    }

    /// <summary>Replaces the producer result rows with the copyright panel and starts its authored hold.</summary>
    internal void ShowRewardCopyright()
    {
        // The copyright panel replaces the producer panel. The text alone is shown until the copyright's hold ends.
        _ = _artwork.EndingText ?? throw new InvalidOperationException("Ending copyright requires installed text assets.");
        _artwork.PostCreditsText.ReplaceResultWithCopyright();
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
        // The fade saves the post-credits colours as its target, then runs the shared colour component step 32 times.
        // Each increment is eight times the component in 256ths and composing keeps only whole units, so step S of 32 shows
        // each target channel times S over 32, rounded down: the level the import pre-colours (#1512).
    }

    /// <summary>The steps left of the white wash of the brief transition into the final logo (<see cref="WhiteWash"/>), fading to none, where it stays.</summary>
    internal int WhiteFlashColor { get; set; }

    /// <summary>The state of the ending logo, which exists while that phase runs.</summary>
    internal EndingLogo EndingLogo => _artwork.CurrentEndingLogo ??
        throw new InvalidOperationException("The ending logo is not running.");

    /// <summary>Restores the shooting palette, clears the copyright, puts the text back at the top and begins the white transition.</summary>
    internal void BeginPostCreditsWhiteFlash()
    {
        CurrentPostShot = null;
        _stage.LoadColours(EndingColourLoad.RestoredShot);
        _artwork.PostCreditsText.Drop = 0;
        _artwork.PostCreditsText.ClearCopyright();
        WhiteFlashColor = EndingPostShotDefinitions.WhiteComponent;
        _stage.PhaseTimer = EndingPostShotDefinitions.FadeFrames;
        _stage.Phase = EndingCreditsPhase.PostCreditsWhiteFlash;
    }

    /// <summary>Installs the final logo actor.</summary>
    internal void FinishPostCreditsWhiteFlash()
    {
        CurrentRewardJump = null;
        sprites.Clear();
        _artwork.CurrentEndingLogo = new EndingLogo(_stage,
            () => _stage.ColourEffects.Start(EndingLogoDefinitions.LandingColourEffect, 0));
        _stage.Phase = EndingCreditsPhase.PostCreditsLogo;
    }

    /// <summary>Advances logo cues and starts the item-percentage text once the logo completes.</summary>
    internal void StepPostCreditsLogo()
    {
        EndingLogo.Step(screen.Colours);
        if (!EndingLogo.Completed) return;
        _artwork.PostCreditsText.StartItemPercentage(TypewriterSetup());
        _stage.Phase = EndingCreditsPhase.ItemPercentage;
    }

    /// <summary>What the post-credits typed texts type from and count.</summary>
    /// <exception cref="InvalidOperationException">No ending text is installed.</exception>
    internal EndingTypewriterSetup TypewriterSetup() => new()
    {
        Inventory = inventory,
        JapaneseText = japaneseText,
        Text = _artwork.EndingText ?? throw new InvalidOperationException("Post-credits text requires installed ending text."),
    };

    /// <summary>Whether the reward copyright is showing.</summary>
    internal bool RewardCopyrightShown { get; private set; }

    /// <summary>Starts the reward palette from its first step.</summary>
    internal void ResetRewardPalette() => rewardPaletteStep = 0;

    /// <summary>Moves the reward palette to its next step.</summary>
    internal void AdvanceRewardPalette() => rewardPaletteStep++;
}
