using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// Title-sequence state owned by game state one.
/// </summary>
/// <remarks>
/// This ports the visible state chain at <c>$8B:9A22-$8B:A35A</c>. Installed sessions
/// supply extracted graphics, palette, and gradient presentation. No cartridge
/// image is mapped during the title sequence.
/// </remarks>
public sealed class TitleSequenceState
{
    /// <summary>Reusable OBJ raster cleared by each render; excluded from saved scene state.</summary>
    [NonSerialized] private Rgba32[]? objectLayerScratch;
    /// <summary>Reusable final frame, valid for callers until the next title render.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;
    /// <summary>Temporary OBJ priority plane rewritten before the title gradient is resolved.</summary>
    [NonSerialized] private byte[]? gradientPriorityScratch;
    /// <summary>Temporary OBJ palette plane used to choose the title gradient color per pixel.</summary>
    [NonSerialized] private byte[]? gradientPaletteScratch;

    /// <summary>Runtime address space used by native title state and palette-FX paths.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Queues the opening music and title transitions when audio is available.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Holds interleaved Mode 7 background and OBJ character data for title rendering.</summary>
    private readonly SnesVram vram = new();
    /// <summary>Holds installed title colors and live console palette-FX changes.</summary>
    internal readonly SnesCgram cgram = new();
    /// <summary>Runs the two console-light palette-FX programs during the title sequence.</summary>
    private RoomPaletteFxSystem consolePaletteFx = new();
    /// <summary>Receives title and copyright spritemaps in native drawing order.</summary>
    internal readonly OamBuffer oam = new();
    /// <summary>Preserves newly pressed input edges for title skip and file-select entry.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Extracted source pages copied into Mode 7 VRAM for Baby Metroid animation.</summary>
    private readonly byte[] babyMetroidCharacters;
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
    /// <summary>Spritemap selected by the active title text or logo entry.</summary>
    internal ushort activeSpritemap;
    /// <summary>Screen origin used when drawing the active title spritemap.</summary>
    internal Position16<ScreenPixels> activeOrigin;
    /// <summary>Character and palette offset supplied to the title sprite drawer.</summary>
    internal ushort activeCharacterOffset;
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
        ISnesAddressSpace bus,
        CartridgeAudioState? audio = null,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null)
        : this(bus, audio, queueOpeningMusic: true, titleGradientPresentation, titlePalettePresentation, titleGraphicsPresentation)
    {
    }

    /// <summary>Builds the initial title VRAM and palette state, optionally queuing opening music for a new session.</summary>
    private TitleSequenceState(
        ISnesAddressSpace bus,
        CartridgeAudioState? audio,
        bool queueOpeningMusic,
        TitleGradientPresentation? titleGradientPresentation,
        TitlePalettePresentation? titlePalettePresentation,
        TitleGraphicsPresentation? titleGraphicsPresentation)
    {
        this.bus = Ensure.NotNull(bus);
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

        // `$8B:9B87` expands these four independent streams to bank-$7F. Recreate the
        // subsequent DMA destinations rather than keeping an invented host texture format.
        byte[] mode7Characters = titleGraphicsPresentation.Mode7Characters.ToArray();
        byte[] mode7Map = titleGraphicsPresentation.Mode7Map.ToArray();
        byte[] objectCharacters = titleGraphicsPresentation.ObjectCharacters.ToArray();
        babyMetroidCharacters = titleGraphicsPresentation.BabyCharacters.ToArray();

        LoadMode7InterleavedVram(mode7Characters, mode7Map);
        new TitleVram(vram).ObjectCharacters.Load(
            objectCharacters.AsSpan(
                0,
                Math.Min(
                    TitleSequenceRomData.Vram.ObjectCharacterByteCount,
                    objectCharacters.Length)));
        titlePalettePresentation.Apply(cgram);
        ResetConsolePaletteFx();

        // The Year object's pre-instruction forces full brightness on the first
        // processing frame; its position and palette come from the native definition.
        brightness = TitleSequenceRomData.Timing.MaximumBrightness;
        BeginTextSequence(TitleSequenceRomData.TextSequences.Year);
        UpdateBabyMetroidCharacterFrame();
    }

    /// <summary>Current native title sub-state, exposed as a stable debugger label.</summary>
    public TitleSequencePhase Phase { get; internal set; }

    /// <summary>True after the title's slow fade has handed control to file select.</summary>
    public bool FileSelectRequested { get; private set; }
    /// <summary>True after the idle timeout's slow fade reaches native demo state $28.</summary>
    public bool DemoRequested { get; private set; }

    /// <summary>State $2C's player-cancelled return skips the introductory title cards.</summary>
    internal static TitleSequenceState ReturnFromDemo(
        ISnesAddressSpace bus,
        CartridgeAudioState audio,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null)
    {
        var title = new TitleSequenceState(
            bus,
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
    internal void BindTitleGraphics(TitleGraphicsPresentation? presentation) =>
        titleGraphicsPresentation = presentation;

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
            new FrontendPalettes(cgram).WriteTitleCopyright(
                presentation?.SkipCopyrightWhite ?? TitleSequenceRomData.Palette.CopyrightWhite,
                presentation?.SkipCopyrightRed ?? TitleSequenceRomData.Palette.CopyrightRed);
        }
    }

    /// <summary>Runs one accepted title-sequence frame.</summary>
    public void Step(ushort controllerInput)
    {
        controller.Latch(controllerInput);
        bool rebuildConsolePaletteFxAfterStep = false;

        // `$8B:9A48` admits B, Start, or A during every pre-title cinematic function. The
        // original performs a fast fade-out, reconstructs the final title objects, then a
        // fast fade-in. Do not jump straight to a host menu: those intermediate frames and
        // their INIDISP values are observable in native traces.
        bool confirmPressed =
            controller.NewlyPressedButtons.HasAny(TitleSequenceRomData.Timing.ConfirmButtons);
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
                    activeSpritemap = TitleSequenceRomData.Sprites.SuperMetroidLogo;
                    activeOrigin = TitleSequenceRomData.Sprites.LogoOrigin;
                    activeCharacterOffset = TitleSequenceRomData.Sprites.TitleCharacterOffset.Raw;
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
        consolePaletteFx.Step(bus, cgram, titlePalettePresentation ?? throw new InvalidOperationException(
                "Title palette FX requires installed title colors."), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        // Native skip reconstruction follows PaletteFxHandler for this frame. Restart
        // only here, not when the subsequent fade-in arms the title idle countdown.
        if (rebuildConsolePaletteFxAfterStep)
            ResetConsolePaletteFx();
    }

    /// <summary>Restarts the slow and fast console-light palette programs at the native title seam.</summary>
    private void ResetConsolePaletteFx()
    {
        consolePaletteFx = new RoomPaletteFxSystem();
        consolePaletteFx.SpawnDefinition(bus, TitleSequenceRomData.ConsolePaletteFx.SlowLights, 0);
        consolePaletteFx.SpawnDefinition(bus, TitleSequenceRomData.ConsolePaletteFx.FastLights, 0);
    }

    /// <summary>Renders the current Mode 7 background and bank-$8C title spritemaps.</summary>
    public Rgba32[] Render()
    {
        // `$8B:9E8B` reaches identity A=D=$0100. The value being animated is already the
        // Mode 7 matrix scalar, not a camera magnification that needs to be inverted. A
        // small matrix samples less source texture across the output screen, so the early
        // title image is enlarged; increasing $43 -> $100 therefore performs the retail
        // zoom *out*. Taking its reciprocal reversed that motion and also magnified the
        // Nintendo Presents pan offsets until much of their movement wrapped off-screen.
        short matrixScale = unchecked((short)zoom);
        Rgba32[] background = SnesLayerCompositor.CreateBackdrop(cgram, FrontendFrame.Width * FrontendFrame.Height, frameBuffer ??= new Rgba32[FrontendFrame.Width * FrontendFrame.Height]);

        // Setup_PPU_TitleSequence writes TM=$10 at `$8B:803F`: OBJ is visible but BG1 is
        // not. Each scrolling-text command leaves that state alone, so the 1994/NINTENDO/
        // PRESENTS/METROID 3 cards sit over the CGRAM-zero black backdrop. Their following
        // `$9CE1/$9D5D/$9DD6/$9E58` scene command writes TM=$11 and enables Mode 7 BG1;
        // each of the first three completed pans restores TM=$10 before spawning the next
        // text object. Rendering a zero-scale Mode 7 sample during YearText was therefore
        // not merely the wrong transform—it displayed a layer the SNES had disabled and
        // turned the whole screen into the sampled red texel.
        if (mode7BackgroundEnabled)
        {
            SnesMode7Renderer.CompositeViewport(background, vram, cgram, new Mode7RenderRegisters { MatrixA = matrixScale, MatrixB = TitleSequenceRomData.Scenes.Rotation.TableIndex, MatrixC = 0, MatrixD = matrixScale, Center = new Offset16<Mode7Pixels>(128, 128), Scroll = mode7.Whole.AsSignedRegister() });
        }

        PrepareRenderOam();
        Rgba32[] objects = objectLayerScratch ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
        SnesObjRenderer.Render(objects, oam,
            vram,
            cgram,
            obsel: TitleSequenceRomData.Sprites.ObjectSizeAndBaseSelector);
        SnesLayerCompositor.Composite(background, objects);
        if (gradientEnabled)
        {
            byte[] palettes = gradientPaletteScratch ??= new byte[background.Length];
            SnesObjRenderer.RenderResolved(oam, vram, cgram, TitleSequenceRomData.Sprites.ObjectSizeAndBaseSelector, new ObjRasterTargets(objects, gradientPriorityScratch ??= new byte[background.Length], palettes));
            ReadOnlySpan<TitleGradientLine> gradient = ResolveTitleGradient();
            for (int pixel = 0; pixel < background.Length; pixel++)
                background[pixel] = TitleGradientColorMath.Apply(background[pixel], gradient[pixel / 256],
                    palettes[pixel] == byte.MaxValue ? null : palettes[pixel]);
        }
        for (int pixel = 0; pixel < background.Length; pixel++)
            background[pixel] = ApplyBrightness(background[pixel], brightness);

        return background;
    }

    /// <summary>
    /// Builds the cartridge's OAM on the simulation owner and captures its display.
    /// No pixels are composed, and the returned value retains no live scene references.
    /// Call at the same display boundary as Render, before subsequent state changes.
    /// </summary>
    public Mode7ObjRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderOam();
        short scale = unchecked((short)zoom);
        return new(PpuMemorySnapshot.Capture(vram, cgram, oam),
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

    /// <summary>Draws the active title spritemap and stage-dependent copyright into finalized OAM.</summary>
    internal void PrepareRenderOam()
    {
        TitleGraphicsPresentation artwork = titleGraphicsPresentation
            ?? throw new InvalidOperationException("Title sprites require installed title artwork.");
        oam.BeginFrame();
        if (activeSpritemap != TitleSequenceRomData.Sprites.Blank)
        {
            // Cinematic drawing calls `$81:879F`, whose `chr_r22` replaces palette bits
            // after masking the ROM attributes with `$F1FF`. It does not add a base tile;
            // confusing it with the enemy loader makes the title art uniformly blue.
            artwork.DrawSprite(activeSpritemap, oam,
                activeOrigin.ToOam(), activeCharacterOffset);
        }

        if (Phase is >= TitleSequencePhase.CopyrightFade and <= TitleSequencePhase.TitleScreenFadeOut)
        {
            artwork.DrawSprite(TitleSequenceRomData.Sprites.NintendoCopyright, oam,
                TitleSequenceRomData.Sprites.CopyrightOrigin.ToOam(),
                TitleSequenceRomData.Sprites.CopyrightPalette.Raw);
        }

        oam.FinalizeFrame();
    }

    /// <summary>Advances the running title card's timed frames and runs its scene or delete commands.</summary>
    private void StepTextSequence()
    {
        if (--sequenceEntryTimer > 0)
            return;

        while (true)
        {
            TitleCardStep step = TitleCardDefinitions.Steps(textCard)[textCardStep++];
            if (step.Command is not { } command)
            {
                sequenceEntryTimer = step.Duration;
                activeSpritemap = step.Spritemap;
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
                    activeSpritemap = TitleSequenceRomData.Sprites.Blank;
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
        activeSpritemap = TitleSequenceRomData.Sprites.Blank;
        activeOrigin = definition.Origin;
        activeCharacterOffset = definition.CharacterOffset;
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
        new FrontendPalettes(cgram).WriteTitleCopyright(
            titlePalettePresentation?.SkipCopyrightWhite ?? TitleSequenceRomData.Palette.CopyrightWhite,
            titlePalettePresentation?.SkipCopyrightRed ?? TitleSequenceRomData.Palette.CopyrightRed);
        activeSpritemap = TitleSequenceRomData.Sprites.SuperMetroidLogo;
        activeOrigin = TitleSequenceRomData.Sprites.LogoOrigin;
        activeCharacterOffset = TitleSequenceRomData.Sprites.TitleCharacterOffset.Raw;
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

    /// <summary>Recreates the title's high-byte character DMA, low-byte fill, and map transfer in Mode 7 VRAM.</summary>
    private void LoadMode7InterleavedVram(byte[] characterBytes, byte[] mapBytes)
    {
        if (characterBytes.Length < TitleSequenceRomData.Vram.Mode7CharacterByteCount)
            throw new InvalidDataException("Title Mode 7 character stream is shorter than its $4000-byte DMA.");
        if (mapBytes.Length < TitleSequenceRomData.Vram.Mode7MapByteCount)
            throw new InvalidDataException("Title Mode 7 map stream is shorter than its $1000-byte DMA.");

        // DMA mode zero to $2119 writes only high bytes and increments VMADD afterward.
        // The following $2118 fill and map DMA populate the corresponding low bytes.
        var mode7 = new Mode7Vram(vram);
        mode7.FillMap(
            TitleSequenceRomData.Vram.Mode7InitialMapByte,
            TitleSequenceRomData.Vram.Mode7CharacterByteCount);
        mode7.LoadCharacters(
            characterBytes.AsSpan(0, TitleSequenceRomData.Vram.Mode7CharacterByteCount));
        mode7.LoadMap(
            mapBytes.AsSpan(0, TitleSequenceRomData.Vram.Mode7MapByteCount));
    }

    /// <summary>Advances the Baby Metroid page cycle when its authored frame duration expires.</summary>
    private void StepBabyMetroidAnimation()
    {
        if (--babyFrameTimer > 0)
            return;

        babyFrame = (babyFrame + 1) % TitleSequenceRomData.Vram.BabyAnimationSourcePages.Length;
        babyFrameTimer = TitleSequenceRomData.Timing.BabyFrameDuration;
        UpdateBabyMetroidCharacterFrame();
    }

    /// <summary>Uploads the selected Baby Metroid source page to Mode 7 character destination $3800.</summary>
    private void UpdateBabyMetroidCharacterFrame()
    {
        // The instruction list cycles source pages 0,1,2,1 into Mode 7 destination word
        // `$3800`, high byte only. Each page is exactly $100 bytes / four characters.
        int sourcePage = TitleSequenceRomData.Vram.BabyAnimationSourcePages[babyFrame];
        int source = sourcePage * TitleSequenceRomData.Vram.BabyCharacterPageByteCount;
        if (babyMetroidCharacters.Length <
            source + TitleSequenceRomData.Vram.BabyCharacterPageByteCount)
            throw new InvalidDataException("Title baby-Metroid stream is missing an animation page.");

        new TitleVram(vram).LoadBabyMetroidPage(
            babyMetroidCharacters.AsSpan(source, TitleSequenceRomData.Vram.BabyCharacterPageByteCount));
    }

    /// <summary>Installs a compiled Mode 7 scene's position and scale while clearing the previous text sprite.</summary>
    private void ApplyScene(TitleMode7SceneDefinition definition)
    {
        Phase = definition.Phase;
        mode7BackgroundEnabled = true;
        zoom = definition.Scale;
        mode7 = new(mode7.X.WithWhole(definition.Position.X), mode7.Y.WithWhole(definition.Position.Y));
        activeSpritemap = TitleSequenceRomData.Sprites.Blank;
    }

    /// <summary>Applies the SNES brightness level to a composed title pixel, retaining transparency.</summary>
    private static Rgba32 ApplyBrightness(Rgba32 color, int level)
    {
        if (color.A == 0 || level >= TitleSequenceRomData.Timing.MaximumBrightness)
            return color;
        if (level <= 0)
            return new Rgba32(0, 0, 0, color.A);

        return new Rgba32(
            (byte)(color.R * level / TitleSequenceRomData.Timing.MaximumBrightness),
            (byte)(color.G * level / TitleSequenceRomData.Timing.MaximumBrightness),
            (byte)(color.B * level / TitleSequenceRomData.Timing.MaximumBrightness),
            color.A);
    }
}
