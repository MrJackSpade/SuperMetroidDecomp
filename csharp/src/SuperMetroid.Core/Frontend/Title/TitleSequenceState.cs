using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// Title-sequence state owned by the opening-cinematic game state.
/// </summary>
/// <remarks>
/// This runs the visible title state chain. Installed sessions
/// supply extracted graphics, palette, and gradient presentation. No original
/// image is mapped during the title sequence.
/// </remarks>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
public sealed class TitleSequenceState : IFrontendScreen
{
    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork, SuperMetroidGameState state)
    {
        Ensure.NotNull(previous);
        Ensure.NotNull(artwork);
        if (!ReferenceEquals(previous.MapPresentation, artwork.MapPresentation))
        {
            BindTitleGradient(artwork.MapPresentation?.TitleGradient);
            BindTitleBackdrop(artwork.MapPresentation?.TitleBackdrop);
        }
        BindSpriteSheets(artwork.SpriteSheets);
    }

    /// <summary>Binds the installed sprite sheets the title's compositions draw from.</summary>
    [MemberNotNull(nameof(spriteSheets))]
    internal void BindSpriteSheets(InstalledSpriteSheets? sheets)
    {
        spriteSheets = sheets ?? throw new InvalidOperationException("Title sequence requires the installed sprite sheets.");
    }

    /// <summary>The installed sprite sheets the title's compositions draw from; artwork, never saved.</summary>
    private InstalledSpriteSheets spriteSheets;

    /// <summary>Queues the opening music and title transitions when audio is available.</summary>
    private readonly AudioQueues? audio;
    /// <summary>Runs the two console-light palette-FX animations during the title sequence.</summary>
    private RoomPaletteFxSystem consolePaletteFx = new(new PaletteFxSessionState());
    /// <summary>The title and copyright sprite entries in drawing order, over the title sprite page (#1358).</summary>
    internal readonly ScreenSprites objects;
    /// <summary>Preserves newly pressed input edges for title skip and file-select entry.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Installed row gradient, bound when the game is bound to its installed content.</summary>
    private TitleGradientPresentation? titleGradientPresentation;
    /// <summary>
    /// The installed title backdrop (#1512), bound when the game is bound to its installed content; the title's other colours
    /// are pre-coloured into its sheets.
    /// </summary>
    private RgbColor? titleBackdrop;
    /// <summary>Countdown for the active title phase, including fade and idle stages.</summary>
    internal int PhaseTimer { get; private set; }
    /// <summary>The title card whose steps are running.</summary>
    private TitleSequencePhase textCard = TitleSequencePhase.YearText;
    /// <summary>The next step of the running title card.</summary>
    private int textCardStep;
    /// <summary>Frames left before the next title sequence entry is read.</summary>
    private int sequenceEntryTimer;
    /// <summary>Composition selected by the active title text or logo entry, or null while nothing is shown.</summary>
    internal TitleSprite? ActiveSprite { get; set; }
    /// <summary>Screen origin used when drawing the active title frame.</summary>
    internal Position16<ScreenPixels> ActiveOrigin { get; set; }
    /// <summary>What the title shows every part of the active composition as (#1512).</summary>
    internal TitleSpriteColouring ActiveObjectColouring { get; set; }
    /// <summary>
    /// Whether the title objects' rebuild after a skip, or on the return from the demo, has restored the copyright's
    /// glyph colours (#1512), which the copyright then shows over its drawn colours.
    /// </summary>
    internal bool CopyrightRestored { get; private set; }
    /// <summary>The floor's pan position, whose whole pixels are read as signed.</summary>
    private FixedPosition<FloorPixels> panPosition;
    /// <summary>The floor's zoom in 256ths of a floor pixel per screen pixel, animated from the close-up scene to the floor's own size.</summary>
    internal FloorZoom Zoom { get; private set; }
    /// <summary>Current screen brightness applied after composing title layers.</summary>
    internal int Brightness { get; private set; }
    /// <summary>The frame of the Baby Metroid's pulse animation the floor shows.</summary>
    internal int BabyFrame { get; private set; }
    /// <summary>Updates left before the Baby Metroid's animation shows its next frame.</summary>
    private int babyFrameTimer;
    /// <summary>Whether the title shows its perspective floor behind the title objects.</summary>
    private bool floorBackgroundEnabled;
    /// <summary>Enables the installed gradient during the final title stages.</summary>
    private bool gradientEnabled;
    /// <summary>Selects the demo destination after the shared title fade-out.</summary>
    private bool fadingToDemo;

    /// <summary>Creates the initial title setup.</summary>
    /// <param name="audio">Receives the opening music and title transitions when audio is available.</param>
    /// <param name="titleGradientPresentation">The installed gradients, one per zoom bucket.</param>
    /// <param name="titleBackdrop">Required installed title backdrop.</param>
    /// <param name="spriteSheets">Required installed sprite sheets the title's compositions draw from.</param>
    public TitleSequenceState(
        AudioQueues? audio = null,
        TitleGradientPresentation? titleGradientPresentation = null,
        RgbColor? titleBackdrop = null,
        InstalledSpriteSheets? spriteSheets = null)
        : this(audio, openingMusic: TitleOpeningMusic.Queue, titleGradientPresentation, titleBackdrop, spriteSheets)
    {
    }

    /// <summary>Builds the initial title artwork and palette state, optionally queuing opening music for a new session.</summary>
    private TitleSequenceState(
        AudioQueues? audio,
        TitleOpeningMusic openingMusic,
        TitleGradientPresentation? titleGradientPresentation,
        RgbColor? titleBackdrop,
        InstalledSpriteSheets? spriteSheets)
    {
        this.audio = audio;
        BindSpriteSheets(spriteSheets);
        this.titleGradientPresentation = titleGradientPresentation;
        this.titleBackdrop = titleBackdrop;
        if (titleBackdrop is null)
            throw new InvalidOperationException(
                "Title sequence requires the installed title backdrop.");
        if (openingMusic == TitleOpeningMusic.Queue)
        {
            audio?.QueueMusic(
                MusicRequest.LoadDataSet(MusicDataSet.TitleSequence));
            audio?.QueueMusic(
                MusicRequest.SelectTrack(TitleSequenceDefinitions.Music.OpeningTrack));
        }

        // Title setup expands the four artwork streams; the title binds that
        // artwork and its capture places the turning floor, the sprites and the Baby's pose.
        objects = new ScreenSprites();
        ResetConsolePaletteFx();

        // The Year object's frame behaviour forces full brightness on the first
        // processing frame; its position and palette come from its definition.
        Brightness = TitleSequenceDefinitions.Timing.MaximumBrightness;
        BeginTextSequence(TitleSequenceDefinitions.TextSequences.Year);
    }

    /// <summary>Current title sub-state.</summary>
    public TitleSequencePhase Phase { get; internal set; }

    /// <summary>True after the title's slow fade has handed control to file select.</summary>
    public bool FileSelectRequested { get; private set; }
    /// <summary>True after the idle timeout's slow fade reaches the demo state.</summary>
    public bool DemoRequested { get; private set; }

    /// <summary>The player-cancelled return from the demo skips the introductory title cards.</summary>
    internal static TitleSequenceState ReturnFromDemo(
        AudioQueues audio,
        TitleGradientPresentation? titleGradientPresentation = null,
        RgbColor? titleBackdrop = null,
        InstalledSpriteSheets? spriteSheets = null)
    {
        var title = new TitleSequenceState(
            audio,
            openingMusic: TitleOpeningMusic.Skip,
            titleGradientPresentation,
            titleBackdrop,
            spriteSheets);
        title.EnterImmediateTitleObjects();
        title.Brightness = 0;
        title.Phase = TitleSequencePhase.TitleScreenFadeIn;
        return title;
    }

    /// <summary>Binds the installed row gradient when the game is bound to its installed content.</summary>
    internal void BindTitleGradient(TitleGradientPresentation? presentation) =>
        titleGradientPresentation = presentation;

    /// <summary>Binds the installed title backdrop when the game is bound to its installed content.</summary>
    internal void BindTitleBackdrop(RgbColor? backdrop)
    {
        // The restored copyright is a shade of the installed title sheet (#1512), so a new installation's glyph colours
        // show with its sheet; only the backdrop is bound here.
        titleBackdrop = backdrop;
    }

    /// <summary>Runs one accepted title-sequence frame.</summary>
    public void Step(ControllerButton controllerInput)
    {
        controller.Latch(controllerInput);
        bool rebuildConsolePaletteFxAfterStep = false;

        // The title admits B, Start, or A during every pre-title cinematic function. The
        // game performs a fast fade-out, reconstructs the final title objects, then a
        // fast fade-in. Do not jump straight to a host menu: those intermediate frames and
        // their brightness levels are observable in recorded traces.
        bool confirmPressed =
            controller.NewlyPressed.HasAny(TitleSequenceDefinitions.Timing.ConfirmButtons);
        if (confirmPressed && Phase is not (TitleSequencePhase.TitleScreenFadeIn or TitleSequencePhase.TitleScreen or
            TitleSequencePhase.TitleScreenFadeOut))
        {
            Phase = TitleSequencePhase.SkipFadeOut;
            PhaseTimer = 0;
        }

        switch (Phase)
        {
            case TitleSequencePhase.YearText:
            case TitleSequencePhase.NintendoText:
            case TitleSequencePhase.PresentsText:
            case TitleSequencePhase.MetroidThreeText:
                StepTextSequence();
                break;

            case TitleSequencePhase.SceneZeroPan:
                panPosition = panPosition.WithX(panPosition.X.MovedBy(-TitleSequenceDefinitions.Scenes.PanVelocity));
                if (CoordinateAxis.AsSigned(panPosition.X.Whole) < TitleSequenceDefinitions.Scenes.SceneZeroEndX)
                    BeginTextSequence(TitleSequenceDefinitions.TextSequences.Nintendo);
                break;

            case TitleSequencePhase.SceneOnePan:
                panPosition = panPosition.WithX(panPosition.X.MovedBy(-TitleSequenceDefinitions.Scenes.PanVelocity));
                if (CoordinateAxis.AsSigned(panPosition.X.Whole) < TitleSequenceDefinitions.Scenes.SceneOneEndX)
                    BeginTextSequence(TitleSequenceDefinitions.TextSequences.Presents);
                break;

            case TitleSequencePhase.SceneTwoPan:
                panPosition = panPosition.WithY(panPosition.Y.MovedBy(TitleSequenceDefinitions.Scenes.PanVelocity));
                if (CoordinateAxis.AsSigned(panPosition.Y.Whole) >= TitleSequenceDefinitions.Scenes.SceneTwoEndY)
                    BeginTextSequence(TitleSequenceDefinitions.TextSequences.MetroidThree);
                break;

            case TitleSequencePhase.SceneThreeZoom:
                // The zoom grows only on every second update; reaching the floor's own size completes it.
                if (TitleSequenceDefinitions.Scenes.FinalZoomStepEverySecondUpdate.IsDue(PhaseTimer++) && Zoom < FloorZoom.OwnSize)
                    Zoom = Zoom.Widened(TitleSequenceDefinitions.Scenes.FinalZoomStep);
                if (Zoom >= FloorZoom.OwnSize)
                {
                    ActiveSprite = TitleSprite.SuperMetroidLogo;
                    ActiveOrigin = TitleSequenceDefinitions.Sprites.LogoOrigin;
                    ActiveObjectColouring = TitleSequenceDefinitions.Sprites.LogoColouring;
                    Phase = TitleSequencePhase.TitleLogoFade;
                    PhaseTimer = TitleSequenceDefinitions.Timing.LogoHoldFrames;
                }
                break;

            case TitleSequencePhase.TitleLogoFade:
                if (--PhaseTimer <= 0)
                {
                    // The copyright holds for 32 frames before arming the
                    // 900-frame demo countdown. The palette FX itself remains future work;
                    // its final palette is already the visible colour source.
                    Phase = TitleSequencePhase.CopyrightFade;
                    PhaseTimer = TitleSequenceDefinitions.Timing.CopyrightHoldFrames;
                }
                break;

            case TitleSequencePhase.CopyrightFade:
                if (--PhaseTimer <= 0)
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.SkipFadeOut:
                Brightness = Math.Max(
                    0,
                    Brightness - TitleSequenceDefinitions.Timing.SkipFadeBrightnessStep);
                if (Brightness == 0)
                {
                    // The cinematic transition queues the immediate title song before
                    // rebuilding the immediate title objects.
                    audio?.QueueMusic(
                        MusicRequest.SelectTrack(TitleSequenceDefinitions.Music.ImmediateTitleTrack));
                    EnterImmediateTitleObjects();
                    rebuildConsolePaletteFxAfterStep = true;
                    Phase = TitleSequencePhase.TitleScreenFadeIn;
                }
                break;

            case TitleSequencePhase.TitleScreenFadeIn:
                Brightness = Math.Min(
                    TitleSequenceDefinitions.Timing.MaximumBrightness,
                    Brightness + TitleSequenceDefinitions.Timing.SkipFadeBrightnessStep);
                if (Brightness == TitleSequenceDefinitions.Timing.MaximumBrightness)
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.TitleScreen:
                // The timeout check wins over confirmation on its final frame.
                if (--PhaseTimer <= 0)
                {
                    fadingToDemo = true;
                    Phase = TitleSequencePhase.TitleScreenFadeOut;
                    PhaseTimer = TitleSequenceDefinitions.Timing.TitleFadeCadenceFrames;
                }
                else if (confirmPressed)
                {
                    Phase = TitleSequencePhase.TitleScreenFadeOut;
                    PhaseTimer = TitleSequenceDefinitions.Timing.TitleFadeCadenceFrames;
                }
                break;

            case TitleSequencePhase.TitleScreenFadeOut:
                if (--PhaseTimer <= 0)
                {
                    PhaseTimer = TitleSequenceDefinitions.Timing.TitleFadeCadenceFrames;
                    Brightness = Math.Max(0, Brightness - 1);
                    if (Brightness == 0)
                    {
                        DemoRequested = fadingToDemo;
                        FileSelectRequested = !fadingToDemo;
                    }
                }
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TitleSequencePhase)} {Phase}.");
        }

        StepBabyMetroidAnimation();
        // The title runs the same colour effects as rooms. Its two console
        // animations own their colors and timers; the host must not synthesize a blink.
        consolePaletteFx.StepScreen(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, TorizoIsDead = false, AreaMiniBossDefeated = false });
        // The skip reconstruction follows the palette effects for this frame. Restart
        // only here, not when the subsequent fade-in arms the title idle countdown.
        if (rebuildConsolePaletteFxAfterStep)
            ResetConsolePaletteFx();
    }

    /// <summary>Restarts the slow and fast console-light palette animations at the title seam.</summary>
    private void ResetConsolePaletteFx()
    {
        consolePaletteFx = new RoomPaletteFxSystem(new PaletteFxSessionState());
        consolePaletteFx.Start(TitleSequenceDefinitions.ConsolePaletteFx.SlowLights, 0);
        consolePaletteFx.Start(TitleSequenceDefinitions.ConsolePaletteFx.FastLights, 0);
    }

    /// <summary>
    /// Builds the title's sprite list on the simulation owner and captures its display.
    /// No pixels are composed, and the returned value retains no live scene references.
    /// Call at the same display boundary as Render, before subsequent state changes.
    /// </summary>
    public FloorScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        return new(new FramePaint(Backdrop, objects.Draws),
            floorBackgroundEnabled
                ? FloorView.Turned(TitleSequenceDefinitions.Scenes.Rotation, Zoom, TitleSequenceDefinitions.Scenes.FloorCentre,
                    panPosition.Whole.AsSignedOffset())
                : null,
            Brightness, gradientEnabled ? new ScreenGradientLayer(ResolveTitleGradient()) : null,
            floorBackgroundEnabled
                ? TitleFloors.Of(spriteSheets, TitleSequenceDefinitions.Graphics.BabyAnimation[BabyFrame]).Shaded(FloorShading)
                : null);
    }

    /// <summary>The colour the title shows behind its floor and objects: the installed title palette's backdrop (#1512).</summary>
    private RgbColor Backdrop => titleBackdrop ?? throw new InvalidOperationException(
        "Title rendering requires the installed title backdrop.");

    /// <summary>
    /// The floor's shading (#1512, #1597): the shade of the record each running console light shows, the tube light's then
    /// the displays', which recolour different colours of the floor. Lights the skip has just restarted show the floor's
    /// own colours until they show their first record.
    /// </summary>
    internal PictureShading FloorShading
    {
        get
        {
            PictureShading shading = PictureShading.Standard;
            if (consolePaletteFx.FrameOf(PaletteFxColours.BabyMetroidTubeLight) is { } tube)
                shading = shading.With(PaletteFxShades.Of(PaletteFxColours.BabyMetroidTubeLight, tube));
            if (consolePaletteFx.FrameOf(PaletteFxColours.TitleScreenDisplays) is { } displays)
                shading = shading.With(PaletteFxShades.Of(PaletteFxColours.TitleScreenDisplays, displays));
            return shading;
        }
    }

    /// <summary>The copyright's shading: its drawn colours, with its glyph colours restored once the skip has rebuilt the title objects (#1512).</summary>
    internal PictureShading CopyrightShading => CopyrightRestored
        ? PictureShading.Shaded(TitleShades.Drawn(TitleSequenceDefinitions.Sprites.CopyrightColouring)).With(TitleShades.RestoredCopyright)
        : PictureShading.Shaded(TitleShades.Drawn(TitleSequenceDefinitions.Sprites.CopyrightColouring));

    /// <summary>Resolves the installed title gradient at the floor's current zoom.</summary>
    private ScreenGradient ResolveTitleGradient()
    {
        if (titleGradientPresentation is null)
            throw new InvalidOperationException(
                "Title rendering requires the installed title-gradient asset.");
        return titleGradientPresentation.Resolve(Zoom);
    }

    /// <summary>Places the active title frame and stage-dependent copyright as the frame's sprite entries.</summary>
    internal void PrepareRenderSprites()
    {
        SpriteSheet sheet = spriteSheets.Get(SpriteSheetId.Title);
        objects.BeginFrame();
        if (ActiveSprite is { } sprite)
        {
            // Cinematic drawing replaces each part's palette bits
            // after masking the stored attributes. It does not add a base tile;
            // confusing it with the enemy loader makes the title art uniformly blue.
            TitleSpriteDrawing.DrawSprite(sheet, sprite, objects,
                ActiveOrigin.ToSpritePosition(), ActiveObjectColouring);
        }

        if (Phase is TitleSequencePhase.CopyrightFade or TitleSequencePhase.SkipFadeOut or TitleSequencePhase.TitleScreenFadeIn or
            TitleSequencePhase.TitleScreen or TitleSequencePhase.TitleScreenFadeOut)
        {
            TitleSpriteDrawing.DrawSprite(sheet, TitleSprite.NintendoCopyright, objects,
                TitleSequenceDefinitions.Sprites.CopyrightOrigin.ToSpritePosition(),
                CopyrightShading);
        }

        objects.FinishFrame();
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
                ActiveSprite = step.Sprite;
                return;
            }

            switch (cue)
            {
                case TitleSequenceScriptStep.TriggerScene0:
                    Phase = TitleSequencePhase.SceneZeroPan;
                    floorBackgroundEnabled = true; // The pan shows the floor beside the objects.
                    ApplyScene(TitleSequenceDefinitions.Scenes.SceneZero);
                    return;

                case TitleSequenceScriptStep.TriggerScene1:
                    Phase = TitleSequencePhase.SceneOnePan;
                    floorBackgroundEnabled = true; // The pan shows the floor beside the objects.
                    ApplyScene(TitleSequenceDefinitions.Scenes.SceneOne);
                    return;

                case TitleSequenceScriptStep.TriggerScene2:
                    Phase = TitleSequencePhase.SceneTwoPan;
                    floorBackgroundEnabled = true; // The pan shows the floor beside the objects.
                    ApplyScene(TitleSequenceDefinitions.Scenes.SceneTwo);
                    return;

                case TitleSequenceScriptStep.TriggerScene3:
                    Phase = TitleSequencePhase.SceneThreeZoom;
                    gradientEnabled = true;
                    floorBackgroundEnabled = true; // The pan shows the floor beside the objects.
                    PhaseTimer = 0;
                    ApplyScene(TitleSequenceDefinitions.Scenes.SceneThree);
                    return;

                case TitleSequenceScriptStep.Delete:
                    ActiveSprite = null;
                    return;

                default:
                    throw new InvalidOperationException($"Undefined TitleSequenceScriptStep {cue}.");
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
        ActiveOrigin = definition.Origin;
        ActiveObjectColouring = definition.Colouring;
        panPosition = new(panPosition.X.WithFraction(0), panPosition.Y.WithFraction(0));

        // Initial setup and each of the first three completed pan functions hide the floor,
        // showing objects alone, before the next text actor becomes visible. Keeping the
        // visibility explicit also matters when Start skips during a pan: the fade-out keeps
        // whichever visibility was live rather than inferring it from the host phase name.
        floorBackgroundEnabled = false;
    }

    /// <summary>Installs the logo, gradient, copyright colors, and identity turning-floor scene used after a skip.</summary>
    private void EnterImmediateTitleObjects()
    {
        gradientEnabled = true;
        // The skip transition explicitly overwrites the two glyph colours the Nintendo copyright draws in after
        // restoring the sprite colours; the copyright shows them as a shade from now on (#1512).
        CopyrightRestored = true;
        ActiveSprite = TitleSprite.SuperMetroidLogo;
        ActiveOrigin = TitleSequenceDefinitions.Sprites.LogoOrigin;
        ActiveObjectColouring = TitleSequenceDefinitions.Sprites.LogoColouring;
        floorBackgroundEnabled = true;
        panPosition = new(panPosition.X.WithWhole(0), panPosition.Y.WithWhole(0));
        Zoom = FloorZoom.OwnSize;
    }

    /// <summary>Opens the idle title screen at full brightness with its demo countdown.</summary>
    private void EnterTitleScreen()
    {
        EnterImmediateTitleObjects();
        Phase = TitleSequencePhase.TitleScreen;
        PhaseTimer = TitleSequenceDefinitions.Timing.TitleScreenNtscFrames;
        Brightness = TitleSequenceDefinitions.Timing.MaximumBrightness;
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
        panPosition = new(panPosition.X.WithWhole(CoordinateAxis.Wrap(definition.Position.X)),
            panPosition.Y.WithWhole(CoordinateAxis.Wrap(definition.Position.Y)));
        ActiveSprite = null;
    }
}
