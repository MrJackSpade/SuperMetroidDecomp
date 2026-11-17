using System.Diagnostics.CodeAnalysis;
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
/// This runs the visible title state chain. Installed sessions
/// supply extracted graphics, colours, and gradient presentation. No original
/// image is mapped during the title sequence.
/// </remarks>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
public sealed class TitleSequenceState : IFrontendScreen
{
    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork, SuperMetroidGameState state)
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
    private ColourEffectSystem colourEffects = new(new ColourEffectSessionState());
    /// <summary>The title's sprites this frame, the active composition and the copyright, in drawing order (#1358).</summary>
    internal readonly ScreenSprites screenSprites;
    /// <summary>Preserves newly pressed input edges for title skip and file-select entry.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Installed row gradient, bound when the game is bound to its installed content.</summary>
    private TitleGradientPresentation titleGradientPresentation;

    /// <summary>Countdown for the active title phase, including fade and idle stages.</summary>
    internal int PhaseTimer { get; private set; }
    /// <summary>The title card whose steps are running.</summary>
    private TitleSequencePhase textCard = TitleSequencePhase.YearText;
    /// <summary>The next step of the running title card.</summary>
    private int textCardStep;
    /// <summary>Frames left before the next title sequence entry is read.</summary>
    private int sequenceEntryTimer;
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
    /// <summary>The floor's zoom in 256ths of a floor pixel per screen pixel, animated from the close-up scene to the floor's own size.</summary>
    internal FloorZoom Zoom { get; private set; }
    /// <summary>The title's screen brightness and the cadence of its fades; the skip's and the fade-in's step every update.</summary>
    private readonly ScreenFade fade = new(0);
    /// <summary>Current screen brightness applied after composing title layers.</summary>
    internal int Brightness => fade.Level;
    /// <summary>The frame of the Baby Metroid's pulse animation the floor shows.</summary>
    internal int BabyFrame { get; private set; }
    /// <summary>Updates left before the Baby Metroid's animation shows its next frame.</summary>
    private int babyFrameTimer;
    /// <summary>Whether the title shows its perspective floor behind its sprites.</summary>
    private bool floorBackgroundEnabled;
    /// <summary>Enables the installed gradient during the final title stages.</summary>
    private bool gradientEnabled;
    /// <summary>Selects the demo destination after the shared title fade-out.</summary>
    private bool fadingToDemo;

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

        // Title setup expands the four artwork sets; the title binds that
        // artwork and its capture places the turning floor, the sprites and the Baby's pose.
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

        StepBabyMetroidAnimation();
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
                // holds the floor's own size for two updates before the logo shows.
                if (!TitleSequenceDefinitions.Scenes.FinalZoomStepEverySecondUpdate.IsDue(PhaseTimer++))
                    break;
                if (Zoom < FloorZoom.OwnSize)
                    Zoom = Zoom.Widened(TitleSequenceDefinitions.Scenes.FinalZoomStep);
                else
                {
                    ActiveSprite = TitleSprite.SuperMetroidLogo;
                    ActiveOrigin = TitleSequenceDefinitions.Sprites.LogoOrigin;
                    ActiveColouring = TitleSequenceDefinitions.Sprites.LogoColouring;
                    Phase = TitleSequencePhase.TitleLogoFade;
                    PhaseTimer = TitleSequenceDefinitions.Timing.LogoHoldFrames;
                    // The logo starts its fade as it first shows; the effect shows its first shade when the colour
                    // effects step at the end of this update, so the logo first shows black.
                    colourEffects.StartScreen(TitleSequenceDefinitions.Fades.Logo);
                }
                break;

            case TitleSequencePhase.TitleLogoFade:
                if (--PhaseTimer <= 0)
                {
                    // The copyright first shows, and starts its fade, as the logo's hold ends; it holds for 32 updates
                    // before arming the 900-update demo countdown.
                    Phase = TitleSequencePhase.CopyrightFade;
                    PhaseTimer = TitleSequenceDefinitions.Timing.CopyrightHoldFrames;
                    colourEffects.StartScreen(TitleSequenceDefinitions.Fades.Copyright);
                }
                break;

            case TitleSequencePhase.CopyrightFade:
                if (--PhaseTimer <= 0)
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.TitleScreenFadeIn:
                if (fade.Brighten(TitleSequenceDefinitions.Timing.SkipFadeBrightnessStep))
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.TitleScreen:
                // The timeout check wins over confirmation on its final frame.
                if (--PhaseTimer <= 0)
                {
                    fadingToDemo = true;
                    Phase = TitleSequencePhase.TitleScreenFadeOut;
                    fade.StepEvery(TitleSequenceDefinitions.Timing.TitleFadeUpdatesPerStep);
                }
                else if (confirmPressed)
                {
                    Phase = TitleSequencePhase.TitleScreenFadeOut;
                    fade.StepEvery(TitleSequenceDefinitions.Timing.TitleFadeUpdatesPerStep);
                }
                break;

            case TitleSequencePhase.TitleScreenFadeOut:
                if (fade.Darken())
                {
                    DemoRequested = fadingToDemo;
                    FileSelectRequested = !fadingToDemo;
                }
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
            TitleSequencePhase.TitleScreenFadeOut => false,
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
        colourEffects = new ColourEffectSystem(new ColourEffectSessionState());
        colourEffects.StartScreen(TitleSequenceDefinitions.ConsoleColourEffects.SlowLights);
        colourEffects.StartScreen(TitleSequenceDefinitions.ConsoleColourEffects.FastLights);
    }

    /// <summary>
    /// Builds the title's sprite list on the simulation owner and captures its display.
    /// No pixels are composed, and the returned value retains no live scene references.
    /// Call at the same display boundary as Render, before subsequent state changes.
    /// </summary>
    public FloorScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        return new(new FramePaint(Backdrop, screenSprites.Draws),
            floorBackgroundEnabled
                ? FloorView.Turned(TitleSequenceDefinitions.Scenes.Rotation, Zoom, TitleSequenceDefinitions.Scenes.FloorCentre,
                    panPosition.Whole)
                : null,
            Brightness, gradientEnabled ? new ScreenGradientLayer(ResolveTitleGradient()) : null,
            floorBackgroundEnabled
                ? TitleFloors.Of(spriteSheets, TitleSequenceDefinitions.Graphics.BabyAnimation[BabyFrame]).Shaded(FloorShading)
                : null);
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
        // Every part of the composition draws in the active colouring's shading, whatever colouring the part was stored
        // with; the parts' pictures draw as the title sheet holds them.
        if (ActiveSprite is { } sprite)
            TitleSpriteDrawing.DrawSprite(sheet, sprite, screenSprites, ActiveOrigin, ActiveShading);
        if (ActiveCard is { } typed)
            TitleSpriteDrawing.DrawCard(sheet, typed, screenSprites, ActiveOrigin, ActiveShading);

        // The skip's fade-out draws what its running step draws, which never includes the copyright: the copyright first
        // shows in the copyright's hold, which refuses the skip (#1719).
        if (Phase is TitleSequencePhase.CopyrightFade or TitleSequencePhase.TitleScreenFadeIn or
            TitleSequencePhase.TitleScreen or TitleSequencePhase.TitleScreenFadeOut)
        {
            TitleSpriteDrawing.DrawSprite(sheet, TitleSprite.NintendoCopyright, screenSprites,
                TitleSequenceDefinitions.Sprites.CopyrightOrigin,
                CopyrightShading);
        }

        screenSprites.FinishFrame();
    }

    /// <summary>Advances the running title card's timed frames and runs its scene or delete cues.</summary>
    private void StepTextSequence()
    {
        if (--sequenceEntryTimer > 0)
            return;

        while (true)
        {
            TitleCardStep step = TitleCards.Steps(textCard)[textCardStep++];
            if (step.Cue is not { } cue)
            {
                sequenceEntryTimer = step.Duration;
                ActiveCard = step.Typed;
                return;
            }

            switch (cue)
            {
                case TitleCardCue.StartLowerPan:
                    Phase = TitleSequencePhase.LowerPan;
                    floorBackgroundEnabled = true; // The pan shows the floor beside the sprites.
                    ApplyScene(TitleSequenceDefinitions.Scenes.LowerPan);
                    return;

                case TitleCardCue.StartUpperPan:
                    Phase = TitleSequencePhase.UpperPan;
                    floorBackgroundEnabled = true; // The pan shows the floor beside the sprites.
                    ApplyScene(TitleSequenceDefinitions.Scenes.UpperPan);
                    return;

                case TitleCardCue.StartDownwardPan:
                    Phase = TitleSequencePhase.DownwardPan;
                    floorBackgroundEnabled = true; // The pan shows the floor beside the sprites.
                    ApplyScene(TitleSequenceDefinitions.Scenes.DownwardPan);
                    return;

                case TitleCardCue.StartFinalZoom:
                    Phase = TitleSequencePhase.FinalZoom;
                    gradientEnabled = true;
                    floorBackgroundEnabled = true; // The pan shows the floor beside the sprites.
                    PhaseTimer = 0;
                    ApplyScene(TitleSequenceDefinitions.Scenes.FinalZoom);
                    return;

                case TitleCardCue.RemoveCard:
                    ActiveCard = null;
                    return;

                default:
                    throw new InvalidOperationException($"Undefined TitleCardCue {cue}.");
            }
        }
    }

    /// <summary>Starts a title card's animation with its sprites shown and the turning floor hidden.</summary>
    private void BeginTextSequence(TitleTextSequenceDefinition definition)
    {
        Phase = definition.Phase;
        textCard = definition.Phase;
        textCardStep = 0;
        sequenceEntryTimer = 1;
        ActiveSprite = null;
        ActiveCard = null;
        ActiveOrigin = definition.Origin;
        ActiveColouring = definition.Colouring;
        panPosition = new(panPosition.X.WithFraction(0), panPosition.Y.WithFraction(0));

        // Initial setup and each of the first three completed pan phases hide the floor,
        // showing the sprites alone, before the next text actor becomes visible, also while the
        // skip's fade-out runs alongside the step (#1719).
        floorBackgroundEnabled = false;
    }

    /// <summary>Installs the logo, gradient, copyright colors, and identity turning-floor scene used after a skip.</summary>
    private void EnterImmediateTitleScreen()
    {
        gradientEnabled = true;
        // Skipping to the title screen shows the Nintendo copyright in its restored colours, a shade from now on (#1512).
        CopyrightRestored = true;
        ActiveCard = null;
        ActiveSprite = TitleSprite.SuperMetroidLogo;
        ActiveOrigin = TitleSequenceDefinitions.Sprites.LogoOrigin;
        ActiveColouring = TitleSequenceDefinitions.Sprites.LogoColouring;
        floorBackgroundEnabled = true;
        panPosition = new(panPosition.X.WithWholePixels(0), panPosition.Y.WithWholePixels(0));
        Zoom = FloorZoom.OwnSize;
    }

    /// <summary>Opens the idle title screen at full brightness with its demo countdown.</summary>
    private void EnterTitleScreen()
    {
        EnterImmediateTitleScreen();
        Phase = TitleSequencePhase.TitleScreen;
        PhaseTimer = TitleSequenceDefinitions.Timing.TitleScreenIdleTimeoutFrames;
        fade.Show(DisplayBrightness.Full);
    }

    /// <summary>Advances the Baby Metroid's pulse animation when its authored frame duration expires.</summary>
    private void StepBabyMetroidAnimation()
    {
        if (--babyFrameTimer > 0)
            return;

        BabyFrame = (BabyFrame + 1) % TitleSequenceDefinitions.Graphics.BabyAnimation.Count;
        babyFrameTimer = TitleSequenceDefinitions.Timing.BabyFrameDuration;
    }

    /// <summary>Installs a compiled turning-floor scene's position and zoom while clearing the previous text sprite.</summary>
    private void ApplyScene(TitleFloorSceneDefinition definition)
    {
        Phase = definition.Phase;
        floorBackgroundEnabled = true;
        Zoom = definition.Zoom;
        panPosition = panPosition.WithWhole(definition.Position);
        ActiveSprite = null;
        ActiveCard = null;
    }
}
