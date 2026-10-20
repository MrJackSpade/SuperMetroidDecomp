using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// Title-sequence state owned by game state one.
/// </summary>
/// <remarks>
/// This ports the visible state chain at <c>$8B:9A22-$8B:A35A</c>. Installed sessions
/// supply extracted graphics, palette, and gradient presentation. No cartridge
/// image is mapped during the title sequence.
/// </remarks>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
public sealed class TitleSequenceState
{
    /// <summary>Queues the opening music and title transitions when audio is available.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>The title's colors: the installed palette and live console palette-FX changes (#1358).</summary>
    internal readonly ScreenPalette palette = new();
    /// <summary>The composed memory image of the bound artwork and shown Baby Metroid page, until either changes.</summary>
    [NonSerialized] private byte[]? image;
    /// <summary>Runs the two console-light palette-FX programs during the title sequence.</summary>
    private RoomPaletteFxSystem consolePaletteFx = new(new PaletteFxSessionState());
    /// <summary>The title and copyright sprite entries in native drawing order, over the title OBJ page (#1358).</summary>
    internal readonly ScreenSprites objects;
    /// <summary>Preserves newly pressed input edges for title skip and file-select entry.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Host-selected scanline gradient rebound after debugger-state restoration.</summary>
    [NonSerialized] private TitleGradientPresentation? titleGradientPresentation;
    /// <summary>Host-selected title palette rebound after debugger-state restoration.</summary>
    [NonSerialized] private TitlePalettePresentation? titlePalettePresentation;
    /// <summary>Host-selected title graphics rebound after debugger-state restoration.</summary>
    [NonSerialized] private TitleGraphicsPresentation? titleGraphicsPresentation;
    /// <summary>Countdown for the active title phase, including fade and idle stages.</summary>
    internal int phaseTimer;
    /// <summary>The title card whose steps are running.</summary>
    private TitleSequencePhase textCard = TitleSequencePhase.YearText;
    /// <summary>The next step of the running title card.</summary>
    private int textCardStep;
    /// <summary>Frames left before the next title text list entry is interpreted.</summary>
    private int sequenceEntryTimer;
    /// <summary>Composition selected by the active title text or logo entry, or null while nothing is shown.</summary>
    internal TitleSprite? activeSprite;
    /// <summary>Screen origin used when drawing the active title spritemap.</summary>
    internal Position16<ScreenPixels> activeOrigin;
    /// <summary>Object attributes (the OBJ palette) applied to every part of the active composition.</summary>
    internal SnesObjAttributeWord activeObjectPalette;
    /// <summary>Mode 7 background position whose whole words are interpreted as signed pixels.</summary>
    private FixedPosition<Mode7Pixels> mode7;
    /// <summary>Native 8.8 Mode 7 matrix scale animated from the close-up scene to identity.</summary>
    internal int zoom;
    /// <summary>Current SNES screen brightness applied after composing title layers.</summary>
    internal int brightness;
    /// <summary>Index into the Baby Metroid character page cycle.</summary>
    private int babyFrame;
    /// <summary>Frames left before the next Baby Metroid character page transfer.</summary>
    private int babyFrameTimer;
    /// <summary>Tracks the TM bit that exposes Mode 7 BG1 behind title objects.</summary>
    private bool mode7BackgroundEnabled;
    /// <summary>Enables the installed scanline gradient during the final title stages.</summary>
    private bool gradientEnabled;
    /// <summary>Selects the demo destination after the shared title fade-out.</summary>
    private bool fadingToDemo;

    /// <summary>Creates the native initial title setup performed by <c>$8B:9B68</c>.</summary>
    public TitleSequenceState(
        CartridgeAudioState? audio = null,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null)
        : this(audio, queueOpeningMusic: true, titleGradientPresentation, titlePalettePresentation, titleGraphicsPresentation)
    {
    }

    /// <summary>Builds the initial title VRAM and palette state, optionally queuing opening music for a new session.</summary>
    private TitleSequenceState(
        CartridgeAudioState? audio,
        bool queueOpeningMusic,
        TitleGradientPresentation? titleGradientPresentation,
        TitlePalettePresentation? titlePalettePresentation,
        TitleGraphicsPresentation? titleGraphicsPresentation)
    {
        this.audio = audio;
        this.titleGradientPresentation = titleGradientPresentation;
        this.titlePalettePresentation = titlePalettePresentation;
        this.titleGraphicsPresentation = titleGraphicsPresentation ?? throw new InvalidOperationException(
            "Title sequence requires installed graphics presentation assets.");
        if (titlePalettePresentation is null)
            throw new InvalidOperationException(
                "Title sequence requires installed palette presentation assets.");
        if (queueOpeningMusic)
        {
            audio?.QueueMusicDelayed8(
                MusicCommand.LoadData(TitleSequenceRomData.Music.DataIndex));
            audio?.QueueMusicDelayed8(
                MusicCommand.SelectTrack(TitleSequenceRomData.Music.OpeningTrack));
        }

        // `$8B:9B87` expands the four artwork streams and uploads them; the title binds that
        // artwork and its capture places the Mode 7 planes, OBJ characters and Baby page.
        objects = new ScreenSprites(this.titleGraphicsPresentation.ObjectPage, ObjectSelectionCodec.Sizes(TitleSequenceRomData.Sprites.ObjectSizeAndBaseSelector));
        titlePalettePresentation.Apply(palette);
        ResetConsolePaletteFx();

        // The Year object's pre-instruction forces full brightness on the first
        // processing frame; its position and palette come from the native definition.
        brightness = TitleSequenceRomData.Timing.MaximumBrightness;
        BeginTextSequence(TitleSequenceRomData.TextSequences.Year);
    }

    /// <summary>Current native title sub-state, exposed as a stable debugger label.</summary>
    public TitleSequencePhase Phase { get; internal set; }

    /// <summary>True after the title's slow fade has handed control to file select.</summary>
    public bool FileSelectRequested { get; private set; }
    /// <summary>True after the idle timeout's slow fade reaches native demo state $28.</summary>
    public bool DemoRequested { get; private set; }

    /// <summary>State $2C's player-cancelled return skips the introductory title cards.</summary>
    internal static TitleSequenceState ReturnFromDemo(
        CartridgeAudioState audio,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null)
    {
        var title = new TitleSequenceState(
            audio,
            queueOpeningMusic: false,
            titleGradientPresentation,
            titlePalettePresentation,
            titleGraphicsPresentation);
        title.EnterImmediateTitleObjects();
        title.brightness = 0;
        title.Phase = TitleSequencePhase.TitleScreenFadeIn;
        return title;
    }

    /// <summary>Rebinds the host-selected presentation after debugger-state restoration.</summary>
    internal void BindTitleGradient(TitleGradientPresentation? presentation) =>
        titleGradientPresentation = presentation;

    /// <summary>Rebinds installed sprite compositions after debugger-state restoration.</summary>
    internal void BindTitleGraphics(TitleGraphicsPresentation? presentation)
    {
        titleGraphicsPresentation = presentation;
        image = null;
        if (presentation is not null)
            objects.BindArtwork(presentation.ObjectPage);
    }

    /// <summary>Rebinds installed title colors after debugger-state restoration.</summary>
    internal void BindTitlePalette(TitlePalettePresentation? presentation)
    {
        titlePalettePresentation = presentation;
        // EnterImmediateTitleObjects already overlaid these two cells before a saved
        // title-screen state was captured. Reapply the currently installed colors so
        // loading that state cannot pin the previous installation's glyph colors.
        if (Phase is TitleSequencePhase.TitleScreenFadeIn or
            TitleSequencePhase.TitleScreen or TitleSequencePhase.TitleScreenFadeOut)
        {
            new FrontendPalettes(palette).WriteTitleCopyright(
                presentation?.SkipCopyrightWhite ?? TitleSequenceRomData.Palette.CopyrightWhite,
                presentation?.SkipCopyrightRed ?? TitleSequenceRomData.Palette.CopyrightRed);
        }
    }

    /// <summary>Runs one accepted title-sequence frame.</summary>
    public void Step(SnesButton controllerInput)
    {
        controller.Latch(controllerInput);
        bool rebuildConsolePaletteFxAfterStep = false;

        // `$8B:9A48` admits B, Start, or A during every pre-title cinematic function. The
        // original performs a fast fade-out, reconstructs the final title objects, then a
        // fast fade-in. Do not jump straight to a host menu: those intermediate frames and
        // their INIDISP values are observable in native traces.
        bool confirmPressed =
            controller.NewlyPressed.HasAny(TitleSequenceRomData.Timing.ConfirmButtons);
        if (confirmPressed && Phase < TitleSequencePhase.TitleScreenFadeIn)
        {
            Phase = TitleSequencePhase.SkipFadeOut;
            phaseTimer = 0;
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
                mode7 = mode7.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(
                    mode7.X, -TitleSequenceRomData.Scenes.PanVelocity16Point16));
                if (unchecked((short)mode7.X.Whole) < TitleSequenceRomData.Scenes.SceneZeroEndX)
                    BeginTextSequence(TitleSequenceRomData.TextSequences.Nintendo);
                break;

            case TitleSequencePhase.SceneOnePan:
                mode7 = mode7.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(
                    mode7.X, -TitleSequenceRomData.Scenes.PanVelocity16Point16));
                if (unchecked((short)mode7.X.Whole) < TitleSequenceRomData.Scenes.SceneOneEndX)
                    BeginTextSequence(TitleSequenceRomData.TextSequences.Presents);
                break;

            case TitleSequencePhase.SceneTwoPan:
                mode7 = mode7.WithY(IntroCinematicMotion.AddSignedSixteenSixteen(
                    mode7.Y, TitleSequenceRomData.Scenes.PanVelocity16Point16));
                if (unchecked((short)mode7.Y.Whole) >= TitleSequenceRomData.Scenes.SceneTwoEndY)
                    BeginTextSequence(TitleSequenceRomData.TextSequences.MetroidThree);
                break;

            case TitleSequencePhase.SceneThreeZoom:
                // `$8B:9E8B` increments only on even NMI frame counters.
                if ((phaseTimer++ & 1) == 0 && zoom < TitleSequenceRomData.Scenes.IdentityScale)
                    zoom++;
                if (zoom >= TitleSequenceRomData.Scenes.IdentityScale)
                {
                    activeSprite = TitleSprite.SuperMetroidLogo;
                    activeOrigin = TitleSequenceRomData.Sprites.LogoOrigin;
                    activeObjectPalette = TitleSequenceRomData.Sprites.TitleCharacterOffset;
                    Phase = TitleSequencePhase.TitleLogoFade;
                    phaseTimer = TitleSequenceRomData.Timing.LogoHoldFrames;
                }
                break;

            case TitleSequencePhase.TitleLogoFade:
                if (--phaseTimer <= 0)
                {
                    // `$8B:A0E1` holds the copyright for 32 frames before arming the
                    // 900-frame demo countdown. The palette FX itself remains future work;
                    // its final cartridge palette is already the visible CGRAM source.
                    Phase = TitleSequencePhase.CopyrightFade;
                    phaseTimer = TitleSequenceRomData.Timing.CopyrightHoldFrames;
                }
                break;

            case TitleSequencePhase.CopyrightFade:
                if (--phaseTimer <= 0)
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.SkipFadeOut:
                brightness = Math.Max(
                    0,
                    brightness - TitleSequenceRomData.Timing.SkipFadeBrightnessStep);
                if (brightness == 0)
                {
                    // HandleCinematicsTransitions_1 at $8B:9A83 queues track six before
                    // rebuilding the immediate title objects.
                    audio?.QueueMusicDelayed8(
                        MusicCommand.SelectTrack(TitleSequenceRomData.Music.ImmediateTitleTrack));
                    EnterImmediateTitleObjects();
                    rebuildConsolePaletteFxAfterStep = true;
                    Phase = TitleSequencePhase.TitleScreenFadeIn;
                }
                break;

            case TitleSequencePhase.TitleScreenFadeIn:
                brightness = Math.Min(
                    TitleSequenceRomData.Timing.MaximumBrightness,
                    brightness + TitleSequenceRomData.Timing.SkipFadeBrightnessStep);
                if (brightness == TitleSequenceRomData.Timing.MaximumBrightness)
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.TitleScreen:
                // The native timeout check wins over confirmation on its final frame.
                if (--phaseTimer <= 0)
                {
                    fadingToDemo = true;
                    Phase = TitleSequencePhase.TitleScreenFadeOut;
                    phaseTimer = TitleSequenceRomData.Timing.TitleFadeCadenceFrames;
                }
                else if (confirmPressed)
                {
                    Phase = TitleSequencePhase.TitleScreenFadeOut;
                    phaseTimer = TitleSequenceRomData.Timing.TitleFadeCadenceFrames;
                }
                break;

            case TitleSequencePhase.TitleScreenFadeOut:
                if (--phaseTimer <= 0)
                {
                    phaseTimer = TitleSequenceRomData.Timing.TitleFadeCadenceFrames;
                    brightness = Math.Max(0, brightness - 1);
                    if (brightness == 0)
                    {
                        DemoRequested = fadingToDemo;
                        FileSelectRequested = !fadingToDemo;
                    }
                }
                break;
        }

        StepBabyMetroidAnimation();
        // The title calls the same bank-$8D interpreter as rooms. Its two console
        // programs own their colors and timers; the host must not synthesize a blink.
        consolePaletteFx.Step(palette, titlePalettePresentation ?? throw new InvalidOperationException(
                "Title palette FX requires installed title colors."), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        // Native skip reconstruction follows PaletteFxHandler for this frame. Restart
        // only here, not when the subsequent fade-in arms the title idle countdown.
        if (rebuildConsolePaletteFxAfterStep)
            ResetConsolePaletteFx();
    }

    /// <summary>Restarts the slow and fast console-light palette programs at the native title seam.</summary>
    private void ResetConsolePaletteFx()
    {
        consolePaletteFx = new RoomPaletteFxSystem(new PaletteFxSessionState());
        consolePaletteFx.SpawnDefinition(TitleSequenceRomData.ConsolePaletteFx.SlowLights, 0);
        consolePaletteFx.SpawnDefinition(TitleSequenceRomData.ConsolePaletteFx.FastLights, 0);
    }

    /// <summary>
    /// Builds the cartridge's OAM on the simulation owner and captures its display.
    /// No pixels are composed, and the returned value retains no live scene references.
    /// Call at the same display boundary as Render, before subsequent state changes.
    /// </summary>
    public Mode7ObjRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        short scale = unchecked((short)zoom);
        TitleGraphicsPresentation artwork = titleGraphicsPresentation
            ?? throw new InvalidOperationException("Title rendering requires installed title artwork.");
        image ??= TitleScreenImage.Compose(artwork, TitleSequenceRomData.Vram.BabyAnimationSourcePages[babyFrame]);
        return new(new PpuMemorySnapshot(image, palette.Colors, objects.Draws),
            mode7BackgroundEnabled
                ? new Mode7RenderRegisters(scale, TitleSequenceRomData.Scenes.Rotation.TableIndex,
                    0, scale, new Offset16<Mode7Pixels>(128, 128), mode7.Whole.AsSignedRegister())
                : null,
            TitleSequenceRomData.Sprites.ObjectSizeAndBaseSelector,
            checked((byte)brightness), gradientEnabled ? ResolveTitleGradient() : default);
    }

    /// <summary>Resolves the installed title gradient at the current Mode 7 scale.</summary>
    private ReadOnlySpan<TitleGradientLine> ResolveTitleGradient()
    {
        if (titleGradientPresentation is null)
            throw new InvalidOperationException(
                "Title rendering requires the installed title-gradient asset.");
        return titleGradientPresentation.Resolve((ushort)zoom);
    }

    /// <summary>Places the active title spritemap and stage-dependent copyright as the frame's sprite entries.</summary>
    internal void PrepareRenderSprites()
    {
        TitleGraphicsPresentation artwork = titleGraphicsPresentation
            ?? throw new InvalidOperationException("Title sprites require installed title artwork.");
        objects.BeginFrame();
        if (activeSprite is { } sprite)
        {
            // Cinematic drawing calls `$81:879F`, whose `chr_r22` replaces palette bits
            // after masking the ROM attributes with `$F1FF`. It does not add a base tile;
            // confusing it with the enemy loader makes the title art uniformly blue.
            artwork.DrawSprite(sprite, objects,
                activeOrigin.ToOam(), activeObjectPalette.Raw);
        }

        if (Phase is >= TitleSequencePhase.CopyrightFade and <= TitleSequencePhase.TitleScreenFadeOut)
        {
            artwork.DrawSprite(TitleSprite.NintendoCopyright, objects,
                TitleSequenceRomData.Sprites.CopyrightOrigin.ToOam(),
                TitleSequenceRomData.Sprites.CopyrightPalette.Raw);
        }

        objects.FinishFrame();
    }

    /// <summary>Advances the running title card's timed frames and runs its scene or delete commands.</summary>
    private void StepTextSequence()
    {
        if (--sequenceEntryTimer > 0)
            return;

        while (true)
        {
            TitleCardStep step = TitleCards.Steps(textCard)[textCardStep++];
            if (step.Command is not { } command)
            {
                sequenceEntryTimer = step.Duration;
                activeSprite = step.Sprite;
                return;
            }

            switch (command)
            {
                case TitleSequenceInstruction.TriggerScene0:
                    Phase = TitleSequencePhase.SceneZeroPan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9CE3-$9CE5`.
                    ApplyScene(TitleSequenceRomData.Scenes.SceneZero);
                    return;

                case TitleSequenceInstruction.TriggerScene1:
                    Phase = TitleSequencePhase.SceneOnePan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9D5D-$9D61`.
                    ApplyScene(TitleSequenceRomData.Scenes.SceneOne);
                    return;

                case TitleSequenceInstruction.TriggerScene2:
                    Phase = TitleSequencePhase.SceneTwoPan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9DD6-$9DDA`.
                    ApplyScene(TitleSequenceRomData.Scenes.SceneTwo);
                    return;

                case TitleSequenceInstruction.TriggerScene3:
                    Phase = TitleSequencePhase.SceneThreeZoom;
                    gradientEnabled = true;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9E58-$9E5C`.
                    phaseTimer = 0;
                    ApplyScene(TitleSequenceRomData.Scenes.SceneThree);
                    return;

                case TitleSequenceInstruction.Delete:
                    activeSprite = null;
                    return;

                default:
                    throw new InvalidOperationException($"Undefined TitleSequenceInstruction {command}.");
            }
        }
    }

    /// <summary>Starts a title card's instruction list with OBJ visible and Mode 7 BG1 hidden.</summary>
    private void BeginTextSequence(TitleTextSequenceDefinition definition)
    {
        Phase = definition.Phase;
        textCard = definition.Phase;
        textCardStep = 0;
        sequenceEntryTimer = 1;
        activeSprite = null;
        activeOrigin = definition.Origin;
        activeObjectPalette = definition.CharacterOffset;
        mode7 = new(mode7.X.WithFraction(0), mode7.Y.WithFraction(0));

        // Initial PPU setup and each of the first three completed pan functions select
        // TM=$10 before the next text actor becomes visible. Keeping an explicit register
        // bit also matters when Start skips during a pan: the fade-out preserves whichever
        // TM value was live rather than inferring visibility from the host phase name.
        mode7BackgroundEnabled = false;
    }

    /// <summary>Installs the logo, gradient, copyright colors, and identity Mode 7 scene used after a skip.</summary>
    private void EnterImmediateTitleObjects()
    {
        gradientEnabled = true;
        // Skip transition `$8B:9A9C` explicitly overwrites the two glyph colors used by
        // the Nintendo copyright spritemap after restoring CGRAM's upper half.
        new FrontendPalettes(palette).WriteTitleCopyright(
            titlePalettePresentation?.SkipCopyrightWhite ?? TitleSequenceRomData.Palette.CopyrightWhite,
            titlePalettePresentation?.SkipCopyrightRed ?? TitleSequenceRomData.Palette.CopyrightRed);
        activeSprite = TitleSprite.SuperMetroidLogo;
        activeOrigin = TitleSequenceRomData.Sprites.LogoOrigin;
        activeObjectPalette = TitleSequenceRomData.Sprites.TitleCharacterOffset;
        mode7BackgroundEnabled = true;
        mode7 = new(mode7.X.WithWhole(0), mode7.Y.WithWhole(0));
        zoom = TitleSequenceRomData.Scenes.IdentityScale;
    }

    /// <summary>Opens the idle title screen at full brightness with its native demo countdown.</summary>
    private void EnterTitleScreen()
    {
        EnterImmediateTitleObjects();
        Phase = TitleSequencePhase.TitleScreen;
        phaseTimer = TitleSequenceRomData.Timing.TitleScreenNtscFrames;
        brightness = TitleSequenceRomData.Timing.MaximumBrightness;
    }

    /// <summary>Advances the Baby Metroid page cycle when its authored frame duration expires.</summary>
    private void StepBabyMetroidAnimation()
    {
        if (--babyFrameTimer > 0)
            return;

        babyFrame = (babyFrame + 1) % TitleSequenceRomData.Vram.BabyAnimationSourcePages.Length;
        babyFrameTimer = TitleSequenceRomData.Timing.BabyFrameDuration;
        // The instruction list uploads the next source page; the capture shows it.
        image = null;
    }

    /// <summary>Installs a compiled Mode 7 scene's position and scale while clearing the previous text sprite.</summary>
    private void ApplyScene(TitleMode7SceneDefinition definition)
    {
        Phase = definition.Phase;
        mode7BackgroundEnabled = true;
        zoom = definition.Scale;
        mode7 = new(mode7.X.WithWhole(definition.Position.X), mode7.Y.WithWhole(definition.Position.Y));
        activeSprite = null;
    }
}
