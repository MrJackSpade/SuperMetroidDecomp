using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The staff credits and what follows them: the shooting stars, the reward's copyright and colours, the white flash and
/// the post-credits logo. It sets each scene up and returns the actors the scene owns; the ending's state shows them with
/// its phase (#2541).
/// </summary>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingCreditsRoll
{
    /// <summary>Collected-item snapshot displayed with the final result.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Chooses the localized post-credits text asset.</summary>
    private readonly bool japaneseText;
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    private readonly EndingScreen screen ;
    /// <summary>The ending's actors in their running order, in which they step and draw (<see cref="EndingCastOrderDefinitions"/>).</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>What every ending scene draws on: brightness, the turning-floor view, colours, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;
    /// <summary>The ending's installed artwork: its text, font, staff credits, turning floor, sprite and colour assets.</summary>
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

    /// <summary>
    /// The shooting stars, which fly through every post-credits scene (#2541). They are seeded as the ending starts and
    /// fly from the first post-credits update on.
    /// </summary>
    internal EndingShootingStars ShootingStars { get; } = new();

    /// <summary>Replaces planet actors with the staff scroll and no post-credits text.</summary>
    /// <returns>The staff credits' scroll.</returns>
    internal CreditsScroll SetupCredits()
    {
        // The credits' setup stops every colour effect before loading the credits' and rewards' colours.
        _stage.ResetColourEffects();
        _artwork.ShowCreditsColours();
        _artwork.PostCreditsText.Clear();
        sprites.Clear();
        _stage.Fade.Show(DisplayBrightness.Full);
        return new CreditsScroll();
    }

    /// <summary>Shows the post-credits scenes' own colours (#1544) on a blank screen.</summary>
    internal void SetupPostCreditsBlank()
    {
        // The credits' end starts the post-credits scenes in their own colours and blanks the screen.
        _stage.LoadColours(EndingColourLoad.PostCredits);
        _stage.Fade.Show(0);
    }

    /// <summary>Replaces the producer result rows with the copyright panel.</summary>
    internal void ShowRewardCopyright() => _artwork.PostCreditsText.ReplaceResultWithCopyright();

    /// <summary>Whether the reward copyright is showing, which it does from the reveal's first half's end on.</summary>
    internal bool RewardCopyrightShown => _artwork.PostCreditsText.Panel == EndingPostCreditsPanel.Copyright;

    /// <summary>Fades the post-credits waiting backdrop toward its authored colours.</summary>
    internal void FadeWaitingBackdrop(int step) => screen.Colours.Fade(EndingFade.WaitingBackdrop, step);

    /// <summary>Crossfades waiting colors into the selected non-armored reward colours by <paramref name="fade"/>.</summary>
    private void ShowRewardFade(PacedFadeSteps fade)
    {
        if (_stage.EndingReward == EndingReward.Armored) return;
        int step = fade.Taken;
        FadeWaitingBackdrop(EndingSequenceDefinitions.Timing.WaitingBackdropFadeSteps - step);
        screen.Colours.Fade(EndingFade.SuitlessSamus, step);
        if (_stage.EndingReward == EndingReward.Helmetless)
            screen.Colours.Fade(EndingFade.SuitedSamus, step);
        // The fade brightens toward the post-credits colours in its steps: each step shows each colour at that step's share
        // of its brightness, rounded down, the level the import pre-colours (#1512).
    }

    /// <summary>Starts the reward colour fade from its start and shows it there.</summary>
    /// <returns>The fade, which the reward's reveal owns.</returns>
    internal PacedFadeSteps StartRewardFade()
    {
        var fade = new PacedFadeSteps(EndingCadenceDefinitions.RewardColourFadeSteps, EndingCadenceDefinitions.RewardColourUpdatesPerStep,
            firstStepUpdate: EndingCadenceDefinitions.RewardColourUpdatesPerStep);
        ShowRewardFade(fade);
        return fade;
    }

    /// <summary>One update of the reward's colour fade <paramref name="fade"/>: on its due updates it takes its next step and shows it.</summary>
    internal void StepRewardFade(PacedFadeSteps fade)
    {
        if (Ensure.NotNull(fade).AdvanceTakingStep())
            ShowRewardFade(fade);
    }

    /// <summary>Restores the shooting colours, clears the copyright, puts the text back at the top and begins the white transition.</summary>
    /// <returns>The transition's white wash: full white as it begins, a step fewer each update.</returns>
    internal WhiteWash BeginPostCreditsWhiteFlash()
    {
        _stage.LoadColours(EndingColourLoad.RestoredShot);
        _artwork.PostCreditsText.Drop = 0;
        _artwork.PostCreditsText.ClearCopyright();
        return WhiteWash.FullWhite(EndingCadenceDefinitions.PostCreditsFlashUpdatesPerStep, EndingCadenceDefinitions.PostCreditsFlashFirstStepUpdate);
    }

    /// <summary>Clears the landed Samus and installs the final logo actor.</summary>
    /// <returns>The logo's actor.</returns>
    internal EndingLogo FinishPostCreditsWhiteFlash()
    {
        sprites.Clear();
        return new EndingLogo(_stage,
            () => _stage.ColourEffects.StartScreen(EndingLogoDefinitions.LandingColourEffect));
    }

    /// <summary>Starts the item-percentage text below the completed logo.</summary>
    /// <returns>The text, which types from the next update on.</returns>
    internal EndingTypewriter StartItemPercentage() => _artwork.PostCreditsText.StartItemPercentage(TypewriterSetup());

    /// <summary>Starts the final message above the moved-down item percentage.</summary>
    /// <returns>The text, which types from the next update on.</returns>
    internal EndingTypewriter StartFinalMessage() => _artwork.PostCreditsText.StartFinalMessage(TypewriterSetup());

    /// <summary>What the post-credits typed texts type from and count.</summary>
    private EndingTypewriterSetup TypewriterSetup() => new()
    {
        Inventory = inventory,
        JapaneseText = japaneseText,
        Text = _artwork.EndingText,
    };
}
