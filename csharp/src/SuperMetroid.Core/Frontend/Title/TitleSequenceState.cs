using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// Title-sequence state owned by the opening-cinematic game state.
/// </summary>
/// <remarks>
/// Runs the title's scenes in order, showing the installed pictures, colours and gradient.
/// </remarks>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
public sealed class TitleSequenceState : IFrontendScreen
{
    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork)
    {
        Ensure.NotNull(previous);
        Ensure.NotNull(artwork);
        if (!ReferenceEquals(previous.ScreenContent, artwork.ScreenContent))
        {
            BindTitleGradient(artwork.ScreenContent.TitleGradient);
            BindTitleBackdrop(artwork.ScreenContent.TitleBackdrop);
        }
        BindSpriteSheets(artwork.SpriteSheets);
    }

    /// <summary>Binds the installed sprite sheets the title's compositions draw from.</summary>
    [MemberNotNull(nameof(spriteSheets))]
    internal void BindSpriteSheets(InstalledSpriteSheets sheets)
    {
        spriteSheets = Ensure.NotNull(sheets);
    }

    /// <summary>The installed sprite sheets the title's compositions draw from; artwork, never saved.</summary>
    private InstalledSpriteSheets spriteSheets;

    /// <summary>Queues the opening music and title transitions.</summary>
    private readonly AudioQueues audio;
    /// <summary>
    /// Runs the title's colour effects (#1714): the two console lights for the whole sequence, and the logo's and the
    /// copyright's fades from the updates they first show.
    /// </summary>
    private ColourEffectSystem colourEffects = ColourEffectSystem.ForScreen();
    /// <summary>The title's sprites this frame, the active composition and the copyright, in drawing order (#1358).</summary>
    internal readonly ScreenSprites screenSprites;
    /// <summary>Preserves newly pressed input edges for title skip and file-select entry.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Installed row gradient, bound when the game is bound to its installed content.</summary>
    private TitleGradientPresentation titleGradientPresentation;

    /// <summary>The final zoom's updates, whose cadence paces the zoom's steps.</summary>
    private readonly ElapsedUpdates zoomAge = new();
    /// <summary>The logo's hold before the copyright shows.</summary>
    private UpdateCountdown logoHold = UpdateCountdown.Ended();
    /// <summary>The copyright's hold before the title screen opens.</summary>
    private UpdateCountdown copyrightHold = UpdateCountdown.Ended();
    /// <summary>The idle title screen's wait before the attract demo starts.</summary>
    internal UpdateCountdown IdleTimeout { get; private set; } = UpdateCountdown.Ended();
    /// <summary>The running title card's typing (#2359): it shows nothing until its first update reads its first beat.</summary>
    private readonly AnimationPlayer<TitleSequencePhase, TitleCardTyping, TitleCardCue> textCard =
        AnimationPlayer<TitleSequencePhase, TitleCardTyping, TitleCardCue>.Starting(TitleCards.Segments, TitleCards.Typing(TitleSequencePhase.YearText));
    /// <summary>The whole composition the title shows, the logo, or null while it shows none.</summary>
    internal TitleSprite? ActiveSprite { get; private set; }
    /// <summary>The running title card's text as far as it is typed, or null while no card shows any (#1979).</summary>
    internal TitleCardTyping? ActiveCard { get; private set; }
    /// <summary>Screen origin used when drawing the active title frame.</summary>
    internal Offset16<ScreenPixels> ActiveOrigin { get; private set; }
    /// <summary>What the title shows every part of the active composition as (#1512).</summary>
    internal TitleSpriteColouring ActiveColouring { get; private set; }
    /// <summary>
    /// Whether the title screen's rebuild after a skip, or on the return from the demo, has restored the copyright's
    /// glyph colours (#1512), which the copyright then shows over its drawn colours.
    /// </summary>
    internal bool CopyrightRestored { get; private set; }
    /// <summary>The floor's pan position, whose whole pixels may lie left of zero.</summary>
    private FixedVector<FloorPixels> panPosition;
    /// <summary>The floor's zoom, animated from the close-up scene to the floor's own size.</summary>
    internal FloorZoom Zoom { get; private set; }
    /// <summary>The title's screen brightness and the cadence of its fades; the skip's and the fade-in's step every update.</summary>
    private readonly ScreenFade fade = new(0);
    /// <summary>Current screen brightness applied after composing title layers.</summary>
    internal int Brightness => fade.Level;
    /// <summary>The Baby Metroid's pulse on the floor (#2359).</summary>
    private readonly AnimationPlayer<TitleBabySegment, TitleBabyPose, NoAnimationCue> baby = TitleBabyAnimation.Start();
    /// <summary>The pose of the Baby Metroid's pulse the floor shows.</summary>
    internal TitleBabyPose BabyPose => baby.Showing;
    /// <summary>
    /// Whether the title shows its perspective floor behind its sprites in <paramref name="phase"/>: every step but the
    /// title cards, whose sprites show alone, also while the skip's fade-out runs alongside a card (#1719).
    /// </summary>
    private static bool ShowsFloor(TitleSequencePhase phase) => phase switch
    {
        TitleSequencePhase.YearText or
            TitleSequencePhase.NintendoText or
            TitleSequencePhase.PresentsText or
            TitleSequencePhase.MetroidThreeText => false,
        TitleSequencePhase.LowerPan or
            TitleSequencePhase.UpperPan or
            TitleSequencePhase.DownwardPan or
            TitleSequencePhase.FinalZoom or
            TitleSequencePhase.TitleLogoFade or
            TitleSequencePhase.CopyrightFade or
            TitleSequencePhase.TitleScreenFadeIn or
            TitleSequencePhase.TitleScreen or
            TitleSequencePhase.TitleScreenFadeOutToFileSelect or
            TitleSequencePhase.TitleScreenFadeOutToDemo => true,
        _ => throw new InvalidOperationException($"Undefined {nameof(TitleSequencePhase)} {phase}."),
    };

    /// <summary>
    /// Whether the title shows its installed gradient in <paramref name="phase"/>: from the final zoom on, and on the
    /// title screen a skip or a demo's return rebuilds.
    /// </summary>
    private static bool ShowsGradient(TitleSequencePhase phase) => phase switch
    {
        TitleSequencePhase.YearText or
            TitleSequencePhase.LowerPan or
            TitleSequencePhase.NintendoText or
            TitleSequencePhase.UpperPan or
            TitleSequencePhase.PresentsText or
            TitleSequencePhase.DownwardPan or
            TitleSequencePhase.MetroidThreeText => false,
        TitleSequencePhase.FinalZoom or
            TitleSequencePhase.TitleLogoFade or
            TitleSequencePhase.CopyrightFade or
            TitleSequencePhase.TitleScreenFadeIn or
            TitleSequencePhase.TitleScreen or
            TitleSequencePhase.TitleScreenFadeOutToFileSelect or
            TitleSequencePhase.TitleScreenFadeOutToDemo => true,
        _ => throw new InvalidOperationException($"Undefined {nameof(TitleSequencePhase)} {phase}."),
    };

    /// <summary>Creates the initial title setup.</summary>
    /// <param name="audio">Receives the opening music and title transitions.</param>
    /// <param name="titleGradientPresentation">The installed gradients, one per zoom range.</param>
    /// <param name="titleBackdrop">The installed title backdrop.</param>
    /// <param name="spriteSheets">The installed sprite sheets the title's compositions draw from.</param>
    public TitleSequenceState(
        AudioQueues audio,
        TitleGradientPresentation titleGradientPresentation,
        RgbColor titleBackdrop,
        InstalledSpriteSheets spriteSheets)
        : this(audio, openingMusic: TitleOpeningMusic.Queue, titleGradientPresentation, titleBackdrop, spriteSheets)
    {
    }

    /// <summary>Builds the initial title artwork and colour state, optionally queuing opening music for a new session.</summary>
    private TitleSequenceState(
        AudioQueues audio,
        TitleOpeningMusic openingMusic,
        TitleGradientPresentation titleGradientPresentation,
        RgbColor titleBackdrop,
        InstalledSpriteSheets spriteSheets)
    {
        this.audio = Ensure.NotNull(audio);
        BindSpriteSheets(Ensure.NotNull(spriteSheets));
        this.titleGradientPresentation = Ensure.NotNull(titleGradientPresentation);
        Backdrop = titleBackdrop;
        if (openingMusic == TitleOpeningMusic.Queue)
        {
            audio.QueueMusic(
                MusicRequest.LoadDataSet(MusicDataSet.TitleSequence));
            audio.QueueMusic(
                MusicRequest.SelectTrack(TitleSequenceDefinitions.Music.OpeningTrack));
        }

        // The title shows its installed pictures: its capture places the turning floor, the sprites and the Baby's pose.
        screenSprites = new ScreenSprites();
        RestartColourEffects();

        // The year card forces full brightness on the first
        // processing frame; its position and colouring come from its definition.
        fade.Show(DisplayBrightness.Full);
        BeginTextSequence(TitleSequenceDefinitions.TextSequences.Year);
    }

    /// <summary>The title's running step; the skip's fade-out runs alongside it (#1719).</summary>
    public TitleSequencePhase Phase { get; private set; }

    /// <summary>How far the skip to the title screen has run alongside the running step (#1719).</summary>
    internal TitleSkipStage SkipStage { get; private set; }

    /// <summary>True after the title's slow fade has handed control to file select.</summary>
    public bool FileSelectRequested { get; private set; }
    /// <summary>True after the idle timeout's slow fade reaches the demo state.</summary>
    public bool DemoRequested { get; private set; }

    /// <summary>The player-cancelled return from the demo skips the introductory title cards.</summary>
    internal static TitleSequenceState ReturnFromDemo(
        AudioQueues audio,
        TitleGradientPresentation titleGradientPresentation,
        RgbColor titleBackdrop,
        InstalledSpriteSheets spriteSheets)
    {
        var title = new TitleSequenceState(
            audio,
            openingMusic: TitleOpeningMusic.Skip,
            titleGradientPresentation,
            titleBackdrop,
            spriteSheets);
        title.EnterImmediateTitleScreen();
        title.fade.Restart(0, 0);
        title.Phase = TitleSequencePhase.TitleScreenFadeIn;
        return title;
    }

    /// <summary>Binds the installed row gradient when the game is bound to its installed content.</summary>
    internal void BindTitleGradient(TitleGradientPresentation presentation) =>
        titleGradientPresentation = Ensure.NotNull(presentation);

    /// <summary>Binds the installed title backdrop when the game is bound to its installed content.</summary>
    internal void BindTitleBackdrop(RgbColor backdrop)
    {
        // The restored copyright is a shade of the installed title sheet (#1512), so a new installation's glyph colours
        // show with its sheet; only the backdrop is bound here.
        Backdrop = backdrop;
    }

    /// <summary>
    /// Runs one accepted title-sequence frame: the running step, then the skip's check and fade-out alongside it (#1719),
    /// or, on the update after the skip's fade-out reaches black, the rebuild of the title screen in place of the step.
    /// </summary>
    public void Step(ControllerButton controllerInput)
    {
        controller.Latch(controllerInput);
        bool confirmPressed =
            controller.NewlyPressed.HasAny(TitleSequenceDefinitions.Timing.ConfirmButtons);
        bool rebuiltTitleScreen = SkipStage == TitleSkipStage.Rebuilding;
        if (rebuiltTitleScreen)
        {
            // The fade-out's black stopped the running step on the previous update; this update rebuilds the title
            // screen in its place, and the fade-in starts on the next update.
            EnterImmediateTitleScreen();
            fade.StepEvery(1);
            Phase = TitleSequencePhase.TitleScreenFadeIn;
            SkipStage = TitleSkipStage.NotSkipping;
        }
        else
        {
            StepRunningStep(confirmPressed);
            // A new B, Start or A press skips to the title screen, read after this update's step has run (#1715): the
            // step this update ends in decides, so the update the zoom completes on already refuses it, and a running
            // skip does not restart. The skip's fade-out then runs alongside the running step (#1719), which keeps
            // running until the screen is black; its first fade step falls on the update of the press.
            if (confirmPressed && SkipStage == TitleSkipStage.NotSkipping && AcceptsSkip(Phase))
            {
                fade.StepEvery(1);
                SkipStage = TitleSkipStage.FadingOut;
            }
            if (SkipStage == TitleSkipStage.FadingOut)
                StepSkipFadeOut();
        }

        baby.Step();
        // The title runs the same colour effects as rooms. The console lights and the
        // logo's and copyright's fades own their colours and timers; nothing else synthesizes a blink or a fade.
        colourEffects.StepScreen();
        // The skip's rebuild follows the colour effects for this frame. Restart
        // only here, not when the subsequent fade-in arms the title idle countdown.
        if (rebuiltTitleScreen)
            RestartColourEffects();
    }

    /// <summary>Runs one update of the title's running step.</summary>
    /// <param name="confirmPressed">Whether B, Start or A is newly pressed, which the idle title screen reads as its confirmation.</param>
    private void StepRunningStep(bool confirmPressed)
    {
        switch (Phase)
        {
            case TitleSequencePhase.YearText:
            case TitleSequencePhase.NintendoText:
            case TitleSequencePhase.PresentsText:
            case TitleSequencePhase.MetroidThreeText:
                StepTextSequence();
                break;

            case TitleSequencePhase.LowerPan:
                panPosition = panPosition.WithX(panPosition.X - TitleSequenceDefinitions.Scenes.PanVelocity);
                if (panPosition.X.WholePixels < TitleSequenceDefinitions.Scenes.LowerPanEndX)
                    BeginTextSequence(TitleSequenceDefinitions.TextSequences.Nintendo);
                break;

            case TitleSequencePhase.UpperPan:
                panPosition = panPosition.WithX(panPosition.X - TitleSequenceDefinitions.Scenes.PanVelocity);
                if (panPosition.X.WholePixels < TitleSequenceDefinitions.Scenes.UpperPanEndX)
                    BeginTextSequence(TitleSequenceDefinitions.TextSequences.Presents);
                break;

            case TitleSequencePhase.DownwardPan:
                panPosition = panPosition.WithY(panPosition.Y + TitleSequenceDefinitions.Scenes.PanVelocity);
                if (panPosition.Y.WholePixels >= TitleSequenceDefinitions.Scenes.DownwardPanEndY)
                    BeginTextSequence(TitleSequenceDefinitions.TextSequences.MetroidThree);
                break;

            case TitleSequencePhase.FinalZoom:
                // The zoom acts only on every second update: it grows one step while the floor is smaller than its own
                // size, and the first such update that finds it at its own size completes the zoom (#1715), so the zoom
                // holds the floor's own size for one of its steps before the logo shows.
                bool zoomSteps = TitleSequenceDefinitions.Scenes.FinalZoomStepEverySecondUpdate.IsDue(zoomAge.Age);
                zoomAge.Count();
                if (!zoomSteps)
                    break;
                if (Zoom < FloorZoom.OwnSize)
                    Zoom = Zoom.Widened(TitleSequenceDefinitions.Scenes.FinalZoomStep);
                else
                {
                    ActiveSprite = TitleSprite.SuperMetroidLogo;
                    ActiveOrigin = TitleSequenceDefinitions.Sprites.LogoOrigin;
                    ActiveColouring = TitleSequenceDefinitions.Sprites.LogoColouring;
                    Phase = TitleSequencePhase.TitleLogoFade;
                    logoHold = UpdateCountdown.For(TitleSequenceDefinitions.Timing.LogoHoldFrames);
                    // The logo starts its fade as it first shows; the effect shows its first shade when the colour
                    // effects step at the end of this update, so the logo first shows black.
                    colourEffects.StartScreen(TitleSequenceDefinitions.Fades.Logo);
                }
                break;

            case TitleSequencePhase.TitleLogoFade:
                if (logoHold.Step())
                {
                    // The copyright first shows, and starts its fade, as the logo's hold ends; it holds for its own hold
                    // (TitleSequenceDefinitions.Timing.CopyrightHoldFrames) before arming the demo countdown
                    // (TitleSequenceDefinitions.Timing.TitleScreenIdleTimeoutFrames).
                    Phase = TitleSequencePhase.CopyrightFade;
                    copyrightHold = UpdateCountdown.For(TitleSequenceDefinitions.Timing.CopyrightHoldFrames);
                    colourEffects.StartScreen(TitleSequenceDefinitions.Fades.Copyright);
                }
                break;

            case TitleSequencePhase.CopyrightFade:
                if (copyrightHold.Step())
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.TitleScreenFadeIn:
                if (fade.Brighten(TitleSequenceDefinitions.Timing.SkipFadeBrightnessStep))
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.TitleScreen:
                // The timeout check wins over confirmation on its final frame.
                if (IdleTimeout.Step())
                {
                    Phase = TitleSequencePhase.TitleScreenFadeOutToDemo;
                    fade.StepEvery(TitleSequenceDefinitions.Timing.TitleFadeUpdatesPerStep);
                }
                else if (confirmPressed)
                {
                    Phase = TitleSequencePhase.TitleScreenFadeOutToFileSelect;
                    fade.StepEvery(TitleSequenceDefinitions.Timing.TitleFadeUpdatesPerStep);
                }
                break;

            case TitleSequencePhase.TitleScreenFadeOutToFileSelect:
                if (fade.Darken())
                    FileSelectRequested = true;
                break;

            case TitleSequencePhase.TitleScreenFadeOutToDemo:
                if (fade.Darken())
                    DemoRequested = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TitleSequencePhase)} {Phase}.");
        }
    }

    /// <summary>
    /// Whether a new skip press read at the end of an update in <paramref name="phase"/> skips to the title screen (#1715),
    /// while no skip is running: only while the opening runs, from the first title card until the update the final zoom
    /// completes on. The logo's and the copyright's holds refuse it, the skip's fade-in does not restart, and the idle title
    /// screen reads its own confirmation instead.
    /// </summary>
    private static bool AcceptsSkip(TitleSequencePhase phase) => phase switch
    {
        TitleSequencePhase.YearText or
            TitleSequencePhase.LowerPan or
            TitleSequencePhase.NintendoText or
            TitleSequencePhase.UpperPan or
            TitleSequencePhase.PresentsText or
            TitleSequencePhase.DownwardPan or
            TitleSequencePhase.MetroidThreeText or
            TitleSequencePhase.FinalZoom => true,
        TitleSequencePhase.TitleLogoFade or
            TitleSequencePhase.CopyrightFade or
            TitleSequencePhase.TitleScreenFadeIn or
            TitleSequencePhase.TitleScreen or
            TitleSequencePhase.TitleScreenFadeOutToFileSelect or
            TitleSequencePhase.TitleScreenFadeOutToDemo => false,
        _ => throw new InvalidOperationException($"Undefined {nameof(TitleSequencePhase)} {phase}."),
    };

    /// <summary>
    /// Runs one update of the skip's fast fade-out, after the running step; at black it stops the running step and queues
    /// the immediate title song, and the title screen rebuilds on the next update.
    /// </summary>
    private void StepSkipFadeOut()
    {
        if (!fade.Darken(TitleSequenceDefinitions.Timing.SkipFadeBrightnessStep))
            return;
        // The transition queues the immediate title song an update before rebuilding the immediate title screen.
        audio.QueueMusic(
            MusicRequest.SelectTrack(TitleSequenceDefinitions.Music.ImmediateTitleTrack));
        SkipStage = TitleSkipStage.Rebuilding;
    }

    /// <summary>
    /// Restarts the slow and fast console-light animations at the title seam, ending every other colour effect: a logo or
    /// copyright fade the skip interrupts stops, and the rebuilt logo and copyright show their drawn colours (#1714).
    /// </summary>
    private void RestartColourEffects()
    {
        colourEffects = ColourEffectSystem.ForScreen();
        colourEffects.StartScreen(TitleSequenceDefinitions.ConsoleColourEffects.SlowLights);
        colourEffects.StartScreen(TitleSequenceDefinitions.ConsoleColourEffects.FastLights);
    }

    /// <summary>
    /// Builds the title's sprite list on the simulation owner and captures its display.
    /// No pixels are composed, and the returned value retains no live scene references.
    /// Captured once per shown frame, before the next update changes the state.
    /// </summary>
    public FloorScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        bool floorShown = ShowsFloor(Phase);
        return new(new FramePaint(Backdrop, screenSprites.Draws),
            floorShown
                ? new PlacedFloor(FloorView.Turned(TitleSequenceDefinitions.Scenes.Rotation, Zoom, TitleSequenceDefinitions.Scenes.FloorCentre,
                    panPosition.Whole), TitleFloors.Of(spriteSheets, BabyPose).Shaded(FloorShading))
                : null,
            Brightness, ShowsGradient(Phase) ? new ScreenGradientLayer(ResolveTitleGradient()) : null);
    }

    /// <summary>The colour the title shows behind its floor and sprites: the installed title colours' backdrop (#1512).</summary>
    private RgbColor Backdrop { get; set; }

    /// <summary>
    /// The floor's shading (#1512, #1597): the shade each running console light shows, the tube light's then
    /// the displays', which recolour different colours of the floor. Lights the skip has just restarted show the floor's
    /// own colours until they show their first shade.
    /// </summary>
    internal PictureShading FloorShading
    {
        get
        {
            PictureShading shading = PictureShading.Standard;
            if (colourEffects.FrameOf(ColourEffectColours.BabyMetroidTubeLight) is { } tube)
                shading = shading.With(ColourEffectShades.Of(ColourEffectColours.BabyMetroidTubeLight, tube));
            if (colourEffects.FrameOf(ColourEffectColours.TitleScreenDisplays) is { } displays)
                shading = shading.With(ColourEffectShades.Of(ColourEffectColours.TitleScreenDisplays, displays));
            return shading;
        }
    }

    /// <summary>
    /// The copyright's shading: its drawn colours, with its glyph colours restored once the skip has rebuilt the title
    /// screen (#1512), and the shade its fade last showed laid over them (#1714).
    /// </summary>
    internal PictureShading CopyrightShading => Faded(TitleSequenceDefinitions.Sprites.CopyrightColouring, CopyrightRestored
        ? PictureShading.Shaded(TitleShades.Drawn(TitleSequenceDefinitions.Sprites.CopyrightColouring)).With(TitleShades.RestoredCopyright)
        : PictureShading.Shaded(TitleShades.Drawn(TitleSequenceDefinitions.Sprites.CopyrightColouring)));

    /// <summary>The active composition's shading: its colouring's drawn colours, with the shade a fade of them last showed (#1714).</summary>
    internal PictureShading ActiveShading =>
        Faded(ActiveColouring, PictureShading.Shaded(TitleShades.Drawn(ActiveColouring)));

    /// <summary>
    /// <paramref name="drawn"/> with the shade the fade of <paramref name="colouring"/>'s colours last showed laid over
    /// it (#1714): a fade that has ended keeps showing its last shade, and a colouring no fade animates, or one whose
    /// fade has not started, shows <paramref name="drawn"/>.
    /// </summary>
    private PictureShading Faded(TitleSpriteColouring colouring, PictureShading drawn) =>
        TitleSequenceDefinitions.Sprites.FadedColours.TryGetValue(colouring, out ColourEffectColours colours)
        && colourEffects.FrameOf(colours) is { } frame
            ? drawn.With(ColourEffectShades.Of(colours, frame))
            : drawn;

    /// <summary>Resolves the installed title gradient at the floor's current zoom.</summary>
    private ScreenGradient ResolveTitleGradient()
    {
        return titleGradientPresentation.Resolve(Zoom);
    }

    /// <summary>Places the active title frame and stage-dependent copyright as the frame's sprite entries.</summary>
    internal void PrepareRenderSprites()
    {
        SpriteSheet sheet = spriteSheets.Get(SpriteSheetId.Title);
        screenSprites.BeginFrame();
        // Every part of the composition draws in the active colouring's shading; the parts' pictures draw as the title
        // sheet holds them.
        if (ActiveSprite is { } sprite)
            TitleSpriteDrawing.DrawSprite(sheet, sprite, screenSprites, ActiveOrigin, ActiveShading);
        if (ActiveCard is { } typed)
            TitleSpriteDrawing.DrawCard(sheet, typed, screenSprites, ActiveOrigin, ActiveShading);

        // The skip's fade-out draws what its running step draws, which never includes the copyright: the copyright first
        // shows in the copyright's hold, which refuses the skip (#1719).
        if (Phase is TitleSequencePhase.CopyrightFade or TitleSequencePhase.TitleScreenFadeIn or
            TitleSequencePhase.TitleScreen or TitleSequencePhase.TitleScreenFadeOutToFileSelect or
            TitleSequencePhase.TitleScreenFadeOutToDemo)
        {
            TitleSpriteDrawing.DrawSprite(sheet, TitleSprite.NintendoCopyright, screenSprites,
                TitleSequenceDefinitions.Sprites.CopyrightOrigin,
                CopyrightShading);
        }

        screenSprites.FinishFrame();
    }

    /// <summary>
    /// Plays the running title card on one update: the card shows each typing as its turn comes, and once its whole text has
    /// held it runs its scene cue and removes itself, which the scene's start has already done.
    /// </summary>
    private void StepTextSequence()
    {
        bool cued = false;
        textCard.Step(cue =>
        {
            cued = true;
            RunCardCue(cue);
        });
        if (!cued)
            ActiveCard = textCard.Shown;
    }

    /// <summary>Runs a title card's scene or removal cue.</summary>
    /// <exception cref="InvalidOperationException">The cue is undefined.</exception>
    private void RunCardCue(TitleCardCue cue)
    {
        switch (cue)
        {
            case TitleCardCue.StartLowerPan:
                ApplyScene(TitleSequenceDefinitions.Scenes.LowerPan);
                return;

            case TitleCardCue.StartUpperPan:
                ApplyScene(TitleSequenceDefinitions.Scenes.UpperPan);
                return;

            case TitleCardCue.StartDownwardPan:
                ApplyScene(TitleSequenceDefinitions.Scenes.DownwardPan);
                return;

            case TitleCardCue.StartFinalZoom:
                zoomAge.Restart();
                ApplyScene(TitleSequenceDefinitions.Scenes.FinalZoom);
                return;

            case TitleCardCue.RemoveCard:
                ActiveCard = null;
                return;

            default:
                throw new InvalidOperationException($"Undefined TitleCardCue {cue}.");
        }
    }

    /// <summary>Starts a title card's animation with its sprites shown and the turning floor hidden.</summary>
    private void BeginTextSequence(TitleTextSequenceDefinition definition)
    {
        Phase = definition.Phase;
        textCard.Start(TitleCards.Typing(definition.Phase));
        ActiveSprite = null;
        ActiveCard = null;
        ActiveOrigin = definition.Origin;
        ActiveColouring = definition.Colouring;
        panPosition = FixedVector<FloorPixels>.FromWholePixels(panPosition.Whole);
    }

    /// <summary>Installs the logo, gradient, copyright colors, and identity turning-floor scene used after a skip.</summary>
    private void EnterImmediateTitleScreen()
    {
        // Skipping to the title screen shows the Nintendo copyright in its restored colours, a shade from now on (#1512).
        CopyrightRestored = true;
        ActiveCard = null;
        ActiveSprite = TitleSprite.SuperMetroidLogo;
        ActiveOrigin = TitleSequenceDefinitions.Sprites.LogoOrigin;
        ActiveColouring = TitleSequenceDefinitions.Sprites.LogoColouring;
        // The pan returns to the floor's origin in whole pixels alone: the fractions an earlier pan left stay, as the game
        // resets only the pan's whole pixels.
        panPosition = panPosition.WithWhole(Offset16<FloorPixels>.Zero);
        Zoom = FloorZoom.OwnSize;
    }

    /// <summary>Opens the idle title screen at full brightness with its demo countdown.</summary>
    private void EnterTitleScreen()
    {
        EnterImmediateTitleScreen();
        Phase = TitleSequencePhase.TitleScreen;
        IdleTimeout = UpdateCountdown.For(TitleSequenceDefinitions.Timing.TitleScreenIdleTimeoutFrames);
        fade.Show(DisplayBrightness.Full);
    }

    /// <summary>Enters a turning-floor scene's step at its position and zoom while clearing the previous text sprite.</summary>
    private void ApplyScene(TitleFloorSceneDefinition definition)
    {
        Phase = definition.Phase;
        Zoom = definition.Zoom;
        panPosition = panPosition.WithWhole(definition.Position);
        ActiveSprite = null;
        ActiveCard = null;
    }
}
