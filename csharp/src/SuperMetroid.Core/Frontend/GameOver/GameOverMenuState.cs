using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>The retail game-over prompt.</summary>
public sealed class GameOverMenuState : IMenuFrontendScreen
{
    /// <inheritdoc/>
    [MemberNotNull(nameof(spriteSheets))]
    public void BindSpriteSheets(InstalledSpriteSheets? sheets)
    {
        spriteSheets = sheets ?? throw new InvalidOperationException("Game-over screen requires the installed sprite sheets.");
        screen.BindScreenPictures(sheets);
    }

    /// <summary>The installed sprite sheets the Baby, egg and cursor draw from; artwork, never saved.</summary>
    private InstalledSpriteSheets spriteSheets;

    /// <summary>Shared music and sound queue advanced by the menu scene.</summary>
    private readonly AudioQueues audio;
    /// <summary>The menu screen game over draws: its artwork, pictures, palette and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>Accumulates Baby, egg, and cursor sprites for each rendered frame.</summary>
    /// <summary>Detects newly pressed answer-navigation and confirmation buttons.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Screen brightness stepped once per fade update.</summary>
    private int brightness;
    /// <summary>The selection missile's frame cycle, shared by the Yes and No positions.</summary>
    internal readonly MenuMissileAnimation missile = new();
    /// <summary>Where the Baby's animation stands, or null before its first update of this visit.</summary>
    internal AnimationPlace? BabyPlace { get; private set; }
    /// <summary>The pose and colours the Baby shows: its resting look before its animation starts.</summary>
    internal GameOverBabyLook BabyLook { get; private set; } = GameOverBabyAnimation.Resting;
    /// <summary>Remaining duration of the current Baby pose or accepted-answer hold.</summary>
    internal int BabyFrameHold { get; private set; }
    /// <summary>The colours the Baby shows (#1512): those of its current look.</summary>
    internal GameOverBabyColouring BabyColours => BabyLook.Colours;
    /// <summary>Routes Yes directly to the Ceres loader for the saved arrival state.</summary>
    private readonly bool continueLoadsCeresArrival;
    /// <summary>Installed artwork rebound whenever the game is bound to its installed content without changing menu timing.</summary>
    private AreaMapPresentation? mapPresentation;

    /// <summary>Creates the game-over scene with installed artwork, leaving music, animation, and fade initialization for the first update.</summary>
    /// <param name="audio">Shared music and sound queue used by menu updates.</param>
    /// <param name="mapPresentation">Required installed presentation bundle.</param>
    /// <param name="spriteSheets">Required installed sprite sheets, which the Baby, egg and cursor draw from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="audio"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No installed presentation bundle or sprite sheets are supplied.</exception>
    /// <param name="continueLoadsCeresArrival">
    /// True when the save resumes at the Ceres elevator: the menu then answers Yes by
    /// publishing the Ceres loader immediately instead of fading into the area map.
    /// </param>
    public GameOverMenuState(
        AudioQueues audio,
        AreaMapPresentation? mapPresentation,
        InstalledSpriteSheets? spriteSheets,
        bool continueLoadsCeresArrival = false)
    {
        this.continueLoadsCeresArrival = continueLoadsCeresArrival;
        this.audio = Ensure.NotNull(audio);
        this.mapPresentation = mapPresentation ?? throw new InvalidOperationException(
            "Game-over screen requires installed presentation assets.");
        screen = new MenuScreen(mapPresentation.Backdrops.FileSelect, spriteSheets);
        BindSpriteSheets(spriteSheets);
        Phase = GameOverMenuPhase.ConfigureGraphics;
    }

    /// <summary>The selected answer, Yes or No.</summary>
    public YesNoChoice SelectedItem { get; private set; }

    /// <summary>Current menu step.</summary>
    public GameOverMenuPhase Phase { get; internal set; }

    /// <summary>
    /// True while the current menu step waits out a frame of the display (after blanking or unblanking): the next update
    /// finishes that step instead of starting a new update of the menu (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    public bool IsWaitingOutDisplayFrame { get; internal set; }

    /// <summary>Sticky request raised when the accepted Yes answer has faded fully to black; the outer game-state loop opens file-select map view rather than reloading the save inside this scene.</summary>
    public bool ContinueRequested { get; private set; }

    /// <summary>
    /// Sticky request raised on the frame Yes is accepted while the save resumes at the Ceres
    /// elevator: it publishes the Ceres game state and reloads the save without fading.
    /// </summary>
    public bool CeresArrivalRequested { get; private set; }

    /// <summary>Sticky request raised when the accepted No answer has faded fully to black; the outer game-state loop enters its soft-reset path toward the title.</summary>
    public bool TitleRequested { get; private set; }

    /// <summary>
    /// Binds the installed map artwork when the game is bound to its installed content and refreshes only
    /// presentation-owned display state. Animation phase, answer selection and fade remain live.
    /// </summary>
    public void BindMapPresentation(AreaMapPresentation? catalog)
    {
        string? previousIdentity = mapPresentation?.ContentIdentity;
        mapPresentation = catalog ?? throw new InvalidOperationException(
            "Game-over screen requires installed presentation assets.");
        if (string.Equals(previousIdentity, catalog.ContentIdentity, StringComparison.Ordinal))
            return;
        screen.Backdrop = catalog.Backdrops.FileSelect;
    }

    /// <summary>Advances one menu update, latching input edges, animating the cursor and Baby, and progressing the music wait and one-level-per-update brightness fades.</summary>
    /// <param name="controllerInput">Currently held buttons; only newly pressed buttons affect the interactive menu.</param>
    /// <remarks>Select, Up, or Down toggles the answer and takes priority over A confirmation in the same update. Accepting either answer holds the current Baby frame for 180 updates while fading out.</remarks>
    public void Step(ControllerButton controllerInput)
    {
        controller.Latch(controllerInput);
        if (IsWaitingOutDisplayFrame)
        {
            // Both waits are the last call of their index; the remainder only configures
            // presentation and advances the index.
            IsWaitingOutDisplayFrame = false;
            Phase = Phase switch
            {
                GameOverMenuPhase.ConfigureGraphics => GameOverMenuPhase.Initialize,
                GameOverMenuPhase.Initialize => GameOverMenuPhase.WaitForInitialMusic,
                GameOverMenuPhase.WaitForInitialMusic or
                    GameOverMenuPhase.FadeIn or
                    GameOverMenuPhase.Main or
                    GameOverMenuPhase.FadeOutToContinue or
                    GameOverMenuPhase.FadeOutToTitle => throw new InvalidOperationException($"Game-over phase {Phase} has no frame wait."),
                _ => throw new InvalidOperationException($"Game-over phase {Phase} has no frame wait."),
            };
            return;
        }

        ControllerButton pressed = controller.NewlyPressed;
        // Only the fade-in, the interaction and the two fade-outs draw the selection missile.
        if (Phase is GameOverMenuPhase.FadeIn or GameOverMenuPhase.Main or
            GameOverMenuPhase.FadeOutToContinue or GameOverMenuPhase.FadeOutToTitle)
            missile.Step((mapPresentation ?? throw new InvalidOperationException(
                "Game over requires installed menu presentation assets.")).GameOver.CursorFrameDuration);

        switch (Phase)
        {
            case GameOverMenuPhase.ConfigureGraphics:
                // The death black-out hands over at brightness zero, so the fade leaves the screen
                // black; the menu blanks it and waits out one update before configuring itself.
                if (brightness != 0)
                    throw new InvalidOperationException("Game-over menu must begin from a black screen.");
                audio.QueueSound(RoomSound.Silence, priority: GameOverMenuDefinitions.MaximumQueuedSounds);
                IsWaitingOutDisplayFrame = true;
                break;

            case GameOverMenuPhase.Initialize:
                audio.QueueMusic(MusicRequest.Stop);
                audio.QueueMusic(MusicRequest.LoadDataSet(MusicDataSet.TitleSequence));
                BabyPlace = null;
                BabyLook = GameOverBabyAnimation.Resting;
                BabyFrameHold = 0;
                StepBabyMetroid();
                SelectedItem = YesNoChoice.Yes;
                brightness = 0;
                // Unblank, then wait out one update before advancing the step.
                IsWaitingOutDisplayFrame = true;
                break;

            case GameOverMenuPhase.WaitForInitialMusic:
                // The music gate only polls the music queue; it neither animates the Baby nor draws the missile.
                if (!audio.HasQueuedMusic)
                {
                    audio.QueueMusic(
                        MusicRequest.SelectTrack(GameOverMenuDefinitions.MenuSong));
                    Phase = GameOverMenuPhase.FadeIn;
                }
                break;

            case GameOverMenuPhase.FadeIn:
                StepBabyMetroid();
                brightness = Math.Min(GameOverMenuDefinitions.MaximumBrightness, brightness + 1);
                if (brightness == GameOverMenuDefinitions.MaximumBrightness)
                    Phase = GameOverMenuPhase.Main;
                break;

            case GameOverMenuPhase.Main:
                StepBabyMetroid();
                if ((pressed & (ControllerButton.Select | ControllerButton.Up | ControllerButton.Down)) != ControllerButton.None)
                {
                    audio.QueueSound(
                        SamusSound.MenuCursor,
                        priority: GameOverMenuDefinitions.MaximumQueuedSounds);
                    SelectedItem = SelectedItem == YesNoChoice.Yes ? YesNoChoice.No : YesNoChoice.Yes;
                }
                else if ((pressed & ControllerButton.A) != ControllerButton.None)
                {
                    // The game holds the current Baby frame for 180 ticks once either
                    // answer is accepted, while the selected fade owner continues drawing.
                    BabyFrameHold = GameOverMenuDefinitions.AcceptedAnswerHoldDuration;
                    if (SelectedItem == YesNoChoice.Yes && continueLoadsCeresArrival)
                        CeresArrivalRequested = true;
                    else
                        Phase = SelectedItem == YesNoChoice.Yes
                            ? GameOverMenuPhase.FadeOutToContinue
                            : GameOverMenuPhase.FadeOutToTitle;
                }
                break;

            case GameOverMenuPhase.FadeOutToContinue:
                StepBabyMetroid();
                brightness = Math.Max(0, brightness - 1);
                if (brightness == 0)
                    ContinueRequested = true;
                break;

            case GameOverMenuPhase.FadeOutToTitle:
                StepBabyMetroid();
                brightness = Math.Max(0, brightness - 1);
                if (brightness == 0)
                    TitleRequested = true;
                break;

            default:
                throw new InvalidOperationException($"Unknown game-over phase {Phase}.");
        }
    }

    /// <summary>Captures the page and the sprites over it; the game-over screen shows no backdrop page.</summary>
    public LayeredScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        // The page is one installed picture of the whole game-over page (#1467).
        DisplayLayer[] layers = [new PicturedPlaneLayer(screen.PagePictures(GameOverPresentationDefinitions.PageFrame), Offset16<PlanePixels>.Zero), screen.Sprites()];
        return new(screen.Paint(), layers, brightness);
    }

    /// <summary>Builds the scene's Baby, egg, and selected-answer cursor sprite entries from installed art.</summary>
    private void PrepareRenderSprites()
    {
        ScreenSprites sprites = screen.Objects;
        sprites.BeginFrame();
        var content = mapPresentation ?? throw new InvalidOperationException(
            "Game-over screen requires installed presentation assets.");
        SpriteSheet sheet = spriteSheets.Get(SpriteSheetId.GameOver);
        content.GameOver.DrawBaby(sprites, sheet, BabyLook.Frame, BabyLook.Colours);
        content.GameOver.DrawEgg(sprites, sheet);
        content.GameOver.DrawCursor(sprites, sheet, missile.Frame, SelectedItem != YesNoChoice.Yes);
        sprites.FinishFrame();
    }

    /// <summary>
    /// Runs the game-over Baby Metroid's animation one update: its first update starts it, and when a look's updates run
    /// out it plays on to the next, queueing the cries it passes.
    /// </summary>
    private void StepBabyMetroid()
    {
        if (BabyFrameHold == 0)
        {
            BabyPlace = null;
            ShowNextBabyLook();
        }

        BabyFrameHold--;
        if (BabyFrameHold == 0)
            ShowNextBabyLook();
    }

    /// <summary>Plays the Baby's animation on to its next look, queueing the cries it passes, and holds that look.</summary>
    /// <exception cref="InvalidOperationException">The animation showed no look, which a looping animation of looks never does.</exception>
    private void ShowNextBabyLook()
    {
        AnimationPlace place = BabyPlace ?? AnimationPlace.StartOfAnimation;
        WrappingCountdown repeats = default;
        AnimationShown<GameOverBabySegment, GameOverBabyLook> shown = GameOverBabyAnimation.Segments.PlayToNextFrame(
            GameOverBabyAnimation.Playing, ref place, ref repeats, QueueCry);
        if (shown is not AnimationShown<GameOverBabySegment, GameOverBabyLook>.Frame look)
            throw new InvalidOperationException($"The game-over Baby's animation showed {shown} instead of its next look.");
        BabyPlace = place;
        BabyLook = look.Shown;
        BabyFrameHold = look.Duration;
    }

    /// <summary>Queues one of the Baby's cries on the retail sound queue.</summary>
    private void QueueCry(GameOverBabySound sound) =>
        audio.QueueSound(CrySound(sound), priority: GameOverMenuDefinitions.MaximumQueuedSounds);

    /// <summary>The sound of one of the Baby's cries.</summary>
    /// <exception cref="InvalidDataException">The cry is not one of the Baby's.</exception>
    internal static SoundEffectId CrySound(GameOverBabySound sound) => sound switch
    {
        GameOverBabySound.FirstCry => GameOverMenuDefinitions.FirstCry,
        GameOverBabySound.SecondCry => GameOverMenuDefinitions.SecondCry,
        GameOverBabySound.ThirdCry => GameOverMenuDefinitions.ThirdCry,
        _ => throw new InvalidDataException($"Unknown compiled game-over Baby sound {sound}."),
    };

}
