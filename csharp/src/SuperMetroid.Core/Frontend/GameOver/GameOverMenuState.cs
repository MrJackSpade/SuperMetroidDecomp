using SuperMetroid.Core.Frontend.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.GameOver.Artwork;

namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>The game-over prompt.</summary>
public sealed class GameOverMenuState : IMenuFrontendScreen
{
    /// <inheritdoc/>
    [MemberNotNull(nameof(spriteSheets))]
    public void BindSpriteSheets(InstalledSpriteSheets sheets)
    {
        spriteSheets = Ensure.NotNull(sheets);
        screen.BindScreenPictures(sheets);
    }

    /// <summary>The installed sprite sheets the Baby, egg and cursor draw from; artwork, never saved.</summary>
    private InstalledSpriteSheets spriteSheets;

    /// <summary>Shared music and sound queue advanced by the menu scene.</summary>
    private readonly AudioQueues audio;
    /// <summary>The menu screen game over draws: its artwork, pictures, colours and sprite entries.</summary>
    private readonly MenuScreen screen;
    /// <summary>Detects newly pressed answer-navigation and confirmation buttons.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>The screen's brightness, stepped one level on every fade update; the menu opens black.</summary>
    private readonly ScreenFade fade = new(0);
    /// <summary>The selection missile's frame cycle, shared by the Yes and No positions.</summary>
    private readonly MenuMissileAnimation missile;
    /// <summary>
    /// The Baby Metroid's animation (#2488), played by the shared player: it shows nothing of its own before the menu's
    /// set-up starts it, and holds its look once an answer is accepted.
    /// </summary>
    private readonly AnimationPlayer<GameOverBabySegment, GameOverBabyLook, GameOverBabySound> baby =
        AnimationPlayer<GameOverBabySegment, GameOverBabyLook, GameOverBabySound>.Starting(GameOverBabyAnimation.Segments, GameOverBabyAnimation.Playing);
    /// <summary>The pose and colours the Baby shows: its animation's look, or its resting look before the animation starts.</summary>
    private GameOverBabyLook BabyLook => baby.Shown ?? GameOverBabyAnimation.Resting;
    /// <summary>Routes Yes directly to the Ceres loader for the saved arrival state.</summary>
    private readonly bool continueLoadsCeresArrival;
    /// <summary>Installed artwork rebound whenever the game is bound to its installed content without changing menu timing.</summary>
    private InstalledScreenContent screenContent;

    /// <summary>Creates the game-over scene with installed artwork, leaving music, animation, and fade initialization for the first update.</summary>
    /// <param name="audio">Shared music and sound queue used by menu updates.</param>
    /// <param name="screenContent">Required installed screen content, whose game-over part the screen draws.</param>
    /// <param name="spriteSheets">Required installed sprite sheets, which the Baby, egg and cursor draw from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="audio"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No installed screen content or sprite sheets are supplied.</exception>
    /// <param name="continueLoadsCeresArrival">
    /// True when the save resumes at the Ceres elevator: the menu then answers Yes by
    /// publishing the Ceres loader immediately instead of fading into the area map.
    /// </param>
    public GameOverMenuState(
        AudioQueues audio,
        InstalledScreenContent screenContent,
        InstalledSpriteSheets spriteSheets,
        bool continueLoadsCeresArrival)
    {
        this.continueLoadsCeresArrival = continueLoadsCeresArrival;
        this.audio = Ensure.NotNull(audio);
        this.screenContent = Ensure.NotNull(screenContent);
        missile = new MenuMissileAnimation(screenContent.GameOver.CursorFrameDuration);
        screen = new MenuScreen(screenContent.Backdrops.Menus, Ensure.NotNull(spriteSheets));
        BindSpriteSheets(spriteSheets);
        Phase = GameOverMenuPhase.BlackHold;
    }

    /// <summary>The selected answer, Yes or No.</summary>
    private YesNoChoice SelectedItem { get; set; }

    /// <summary>Current menu step.</summary>
    private GameOverMenuPhase Phase { get; set; }

    /// <summary>
    /// True while the current menu step waits out a frame of the display (after blanking or unblanking): the next update
    /// finishes that step instead of starting a new update of the menu (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    public bool IsWaitingOutDisplayFrame => phaseAfterDisplayFrame is not null;

    /// <summary>The step a display-frame wait leads to once it is waited out (<see cref="IsWaitingOutDisplayFrame"/>); none while no wait runs.</summary>
    private GameOverMenuPhase? phaseAfterDisplayFrame;

    /// <summary>How the menu hands off to the outer game-state loop (#2562), kept once it reaches one.</summary>
    public GameOverMenuOutcome Outcome { get; private set; }

    /// <summary>
    /// Binds the installed map artwork when the game is bound to its installed content and refreshes only
    /// presentation-owned display state. Animation phase, answer selection and fade remain live.
    /// </summary>
    public void BindScreenContent(InstalledScreenContent catalog)
    {
        string previousIdentity = screenContent.ContentIdentity;
        screenContent = Ensure.NotNull(catalog);
        missile.Rebind(catalog.GameOver.CursorFrameDuration);
        if (string.Equals(previousIdentity, catalog.ContentIdentity, StringComparison.Ordinal))
            return;
        screen.Backdrop = catalog.Backdrops.Menus;
    }

    /// <summary>What <c>Step</c> does for each <see cref="GameOverMenuPhase"/> member, each member's own arm.</summary>
    private IdentityTable<GameOverMenuPhase, Action<ControllerButton>> StepForGameOverMenuPhase => field ??= new(new Dictionary<GameOverMenuPhase, Action<ControllerButton>>
    {
        [GameOverMenuPhase.BlackHold] = pressed =>
        {
            // The death black-out hands over at brightness zero, so the screen is black; the menu
            // silences the room's sounds and holds the black screen while it waits out one update.
            if (!fade.IsBlack)
                throw new InvalidOperationException("Game-over menu must begin from a black screen.");
            audio.QueueSound(RoomSound.Silence, priority: GameOverMenuDefinitions.SoundPriority);
            phaseAfterDisplayFrame = GameOverMenuPhase.Initialize;
        },
        [GameOverMenuPhase.Initialize] = pressed =>
        {
            audio.QueueMusic(MusicRequest.Stop);
            audio.QueueMusic(MusicRequest.LoadDataSet(MusicDataSet.TitleSequence));
            // The set-up starts the Baby on its first look, and this update counts as the first of that look's updates.
            baby.Show(GameOverBabyAnimation.Playing);
            StepBabyMetroid();
            SelectedItem = YesNoChoice.Yes;
            fade.Show(0);
            // Unblank, then wait out one update before advancing the step.
            phaseAfterDisplayFrame = GameOverMenuPhase.WaitForInitialMusic;
        },
        [GameOverMenuPhase.WaitForInitialMusic] = pressed =>
        {
            // The music gate only polls the music queue; it neither animates the Baby nor draws the missile.
            if (!audio.HasQueuedMusic)
            {
                audio.QueueMusic(
                    MusicRequest.SelectTrack(GameOverMenuDefinitions.MenuSong));
                Phase = GameOverMenuPhase.FadeIn;
            }
        },
        [GameOverMenuPhase.FadeIn] = pressed =>
        {
            StepBabyMetroid();
            if (fade.Brighten())
                Phase = GameOverMenuPhase.Main;
        },
        [GameOverMenuPhase.Main] = pressed =>
        {
            StepBabyMetroid();
            if ((pressed & (ControllerButton.Select | ControllerButton.Up | ControllerButton.Down)) != ControllerButton.None)
            {
                audio.QueueSound(
                    SamusSound.MenuCursor,
                    priority: GameOverMenuDefinitions.SoundPriority);
                SelectedItem = SelectedItem == YesNoChoice.Yes ? YesNoChoice.No : YesNoChoice.Yes;
            }
            else if ((pressed & ControllerButton.A) != ControllerButton.None)
            {
                // The game holds the current Baby look once either answer is accepted, while the selected
                // fade owner continues drawing.
                baby.HoldShownFrameFor(GameOverMenuDefinitions.AcceptedAnswerHoldDuration);
                if (SelectedItem == YesNoChoice.Yes && continueLoadsCeresArrival)
                    Outcome = GameOverMenuOutcome.CeresArrival;
                else
                    Phase = SelectedItem == YesNoChoice.Yes
                        ? GameOverMenuPhase.FadeOutToContinue
                        : GameOverMenuPhase.FadeOutToTitle;
            }
        },
        [GameOverMenuPhase.FadeOutToContinue] = pressed =>
        {
            StepBabyMetroid();
            if (fade.Darken())
                Outcome = GameOverMenuOutcome.Continue;
        },
        [GameOverMenuPhase.FadeOutToTitle] = pressed =>
        {
            StepBabyMetroid();
            if (fade.Darken())
                Outcome = GameOverMenuOutcome.Title;
        },
    });

    /// <summary>Advances one menu update, latching input edges, animating the cursor and Baby, and progressing the music wait and one-level-per-update brightness fades.</summary>
    /// <param name="controllerInput">Currently held buttons; only newly pressed buttons affect the interactive menu.</param>
    /// <remarks>Select, Up, or Down toggles the answer and takes priority over A confirmation in the same update. Accepting either answer holds the current Baby frame (<see cref="GameOverMenuDefinitions.AcceptedAnswerHoldDuration"/>) while fading out.</remarks>
    public void Step(ControllerButton controllerInput)
    {
        controller.Latch(controllerInput);
        if (phaseAfterDisplayFrame is { } next)
        {
            // Each wait ends its step: what follows it only finishes setting up the screen and
            // moves the menu on to the step the wait leads to.
            phaseAfterDisplayFrame = null;
            Phase = next;
            return;
        }

        ControllerButton pressed = controller.NewlyPressed;
        // Only the fade-in, the interaction and the two fade-outs draw the selection missile.
        if (Phase is GameOverMenuPhase.FadeIn or GameOverMenuPhase.Main or
            GameOverMenuPhase.FadeOutToContinue or GameOverMenuPhase.FadeOutToTitle)
            missile.Step();

        StepForGameOverMenuPhase[Phase](pressed);
    }

    /// <summary>Captures the page and the sprites over it; the game-over screen shows no backdrop page.</summary>
    public LayeredScene ComposeScene()
    {
        PrepareRenderSprites();
        // The page is one installed picture of the whole game-over page (#1467).
        DisplayLayer[] layers = [new PicturedPlaneLayer(screen.PagePictures(GameOverPresentationDefinitions.PageFrame), Offset16<PlanePixels>.Zero), screen.SpriteLayer()];
        return new(screen.Paint(), layers, fade.Level);
    }

    /// <summary>Builds the scene's Baby, egg, and selected-answer cursor sprite entries from installed art.</summary>
    private void PrepareRenderSprites()
    {
        ScreenSprites sprites = screen.Sprites;
        sprites.BeginFrame();
        var content = screenContent;
        SpriteSheet sheet = spriteSheets.Get(SpriteSheetId.GameOver);
        content.GameOver.DrawBaby(sprites, sheet, BabyLook.Frame, BabyLook.Colours);
        content.GameOver.DrawEgg(sprites, sheet);
        content.GameOver.DrawCursor(sprites, sheet, missile.Frame, SelectedItem);
        sprites.FinishFrame();
    }

    /// <summary>
    /// Runs the game-over Baby Metroid's animation one update: when a look's updates run out it plays on to the next,
    /// queueing the cries it passes.
    /// </summary>
    private void StepBabyMetroid() => baby.Step(QueueCry);

    /// <summary>Queues one of the Baby's cries at the menu's sound priority.</summary>
    private void QueueCry(GameOverBabySound sound) =>
        audio.QueueSound(CrySound(sound), priority: GameOverMenuDefinitions.SoundPriority);

    /// <summary>The sound of one of the Baby's cries.</summary>
    private static SoundEffectId CrySound(GameOverBabySound sound) =>
        sound == GameOverBabySound.FirstCry ? GameOverMenuDefinitions.FirstCry
        : sound == GameOverBabySound.SecondCry ? GameOverMenuDefinitions.SecondCry
        // The remaining member, GameOverBabySound.ThirdCry.
        : GameOverMenuDefinitions.ThirdCry;

}
