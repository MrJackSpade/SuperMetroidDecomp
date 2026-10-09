using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

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
    // OBJ layer reused across renders; the span overload clears it first. Never saved state.
    /// <summary>Reusable OBJ pixel plane overwritten when the current title frame is rendered.</summary>
    [NonSerialized] private Rgba32[]? objectLayerScratch;
    // Final frame, reused by every render: a returned frame is valid until this scene renders again.
    /// <summary>Cached output framebuffer; callers must consume it before the next render reuses it.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;
    // Gradient OBJ priority/palette planes; fully rewritten by every resolve.
    /// <summary>Per-pixel OBJ priority data used to resolve title gradient application.</summary>
    [NonSerialized] private byte[]? gradientPriorityScratch;
    /// <summary>Per-pixel palette selection produced alongside the resolved OBJ priorities.</summary>
    [NonSerialized] private byte[]? gradientPaletteScratch;

    /// <summary>Address space used by cartridge title instructions and console palette effects.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Optional output for opening music and title palette/menu audio commands.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>VRAM image populated from installed title graphics and updated by scene DMA operations.</summary>
    private readonly SnesVram vram = new();
    /// <summary>Palette image used by the title renderer and console lighting effects.</summary>
    private readonly SnesCgram cgram = new();
    /// <summary>Native console-lighting effects active over the title palette.</summary>
    private RoomPaletteFxSystem consolePaletteFx = new();
    /// <summary>OAM assembled for the current title objects.</summary>
    private readonly OamBuffer oam = new();
    /// <summary>Controller state consumed by title input dispatch.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Extracted character stream used to animate the baby Metroid in Mode 7 VRAM.</summary>
    private readonly byte[] babyMetroidCharacters;
    /// <summary>Installed line-by-line title gradient, when supplied by the session.</summary>
    [NonSerialized] private TitleGradientPresentation? titleGradientPresentation;
    /// <summary>Installed title palette and skip-transition colors.</summary>
    [NonSerialized] private TitlePalettePresentation? titlePalettePresentation;
    /// <summary>Installed title sprites, Mode 7 graphics, and animation source assets.</summary>
    [NonSerialized] private TitleGraphicsPresentation? titleGraphicsPresentation;

    /// <summary>Current stage of the opening sequence, title display, or outgoing transition.</summary>
    private TitleSequencePhase phase;
    /// <summary>Phase-specific countdown or cadence counter measured in gameplay updates.</summary>
    private int phaseTimer;
    /// <summary>Address of the next timed entry or command in the active text sequence.</summary>
    private int sequenceEntry;
    /// <summary>Remaining updates before the active text-sequence entry is consumed.</summary>
    private int sequenceEntryTimer;
    /// <summary>Installed sprite identity currently drawn by the title sequence.</summary>
    private ushort activeSpritemap;
    /// <summary>Screen-space horizontal origin of the active title sprite.</summary>
    private ushort activeOriginX;
    /// <summary>Screen-space vertical origin of the active title sprite.</summary>
    private ushort activeOriginY;
    /// <summary>Character-data offset applied while drawing the active title sprite.</summary>
    private ushort activeCharacterOffset;
    /// <summary>Integer part of the Mode 7 horizontal scene offset.</summary>
    private int mode7X;
    /// <summary>Integer part of the Mode 7 vertical scene offset.</summary>
    private int mode7Y;
    /// <summary>Fractional part of the Mode 7 horizontal pan accumulated by scene updates.</summary>
    private int mode7XSubposition;
    /// <summary>Fractional part of the Mode 7 vertical pan accumulated by scene updates.</summary>
    private int mode7YSubposition;
    /// <summary>Native Mode 7 matrix scale, stored in the cartridge's 8.8 representation.</summary>
    private int zoom;
    /// <summary>Current master brightness level applied to title pixels.</summary>
    private int brightness;
    /// <summary>Source-page index for the baby Metroid's repeating character animation.</summary>
    private int babyFrame;
    /// <summary>Remaining updates before the baby Metroid selects its next source page.</summary>
    private int babyFrameTimer;
    /// <summary>Whether PPU setup has enabled the Mode 7 background for the active title stage.</summary>
    private bool mode7BackgroundEnabled;
    /// <summary>Whether the title's line gradient is applied during rendering.</summary>
    private bool gradientEnabled;
    /// <summary>Whether the title-screen fade should request the demo destination at black.</summary>
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

    /// <summary>Initializes title state from installed assets with explicit control of opening music.</summary>
    /// <param name="bus">Address space used by title instructions and palette effects.</param>
    /// <param name="audio">Optional sound output for music and palette effects.</param>
    /// <param name="queueOpeningMusic">Whether construction queues the opening music commands.</param>
    /// <param name="titleGradientPresentation">Installed gradient rows used by the title compositor.</param>
    /// <param name="titlePalettePresentation">Installed title colors required for palette setup.</param>
    /// <param name="titleGraphicsPresentation">Installed graphics streams required to populate title VRAM.</param>
    /// <exception cref="ArgumentNullException">The address space is null.</exception>
    /// <exception cref="InvalidOperationException">Required title graphics or palette assets are missing.</exception>
    private TitleSequenceState(
        ISnesAddressSpace bus,
        CartridgeAudioState? audio,
        bool queueOpeningMusic,
        TitleGradientPresentation? titleGradientPresentation,
        TitlePalettePresentation? titlePalettePresentation,
        TitleGraphicsPresentation? titleGraphicsPresentation)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
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
        vram.LoadBytes(
            TitleSequenceRomData.Vram.ObjectCharacterDestinationByte,
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
    public TitleSequencePhase Phase => phase;

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
        title.phase = TitleSequencePhase.TitleScreenFadeIn;
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
        if (phase is TitleSequencePhase.TitleScreenFadeIn or
            TitleSequencePhase.TitleScreen or TitleSequencePhase.TitleScreenFadeOut)
        {
            cgram.SetColor(TitleSequenceRomData.Palette.CopyrightWhiteIndex,
                presentation?.SkipCopyrightWhite ?? TitleSequenceRomData.Palette.CopyrightWhite);
            cgram.SetColor(TitleSequenceRomData.Palette.CopyrightRedIndex,
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
        if (confirmPressed && phase < TitleSequencePhase.TitleScreenFadeIn)
        {
            phase = TitleSequencePhase.SkipFadeOut;
            phaseTimer = 0;
        }

        switch (phase)
        {
            case TitleSequencePhase.YearText:
            case TitleSequencePhase.NintendoText:
            case TitleSequencePhase.PresentsText:
            case TitleSequencePhase.MetroidThreeText:
                StepTextSequence();
                break;

            case TitleSequencePhase.SceneZeroPan:
                AddFixedPoint(
                    ref mode7X,
                    ref mode7XSubposition,
                    -TitleSequenceRomData.Scenes.PanVelocity16Point16);
                if (mode7X < TitleSequenceRomData.Scenes.SceneZeroEndX)
                    BeginTextSequence(TitleSequenceRomData.TextSequences.Nintendo);
                break;

            case TitleSequencePhase.SceneOnePan:
                AddFixedPoint(
                    ref mode7X,
                    ref mode7XSubposition,
                    -TitleSequenceRomData.Scenes.PanVelocity16Point16);
                if (mode7X < TitleSequenceRomData.Scenes.SceneOneEndX)
                    BeginTextSequence(TitleSequenceRomData.TextSequences.Presents);
                break;

            case TitleSequencePhase.SceneTwoPan:
                AddFixedPoint(
                    ref mode7Y,
                    ref mode7YSubposition,
                    TitleSequenceRomData.Scenes.PanVelocity16Point16);
                if (mode7Y >= TitleSequenceRomData.Scenes.SceneTwoEndY)
                    BeginTextSequence(TitleSequenceRomData.TextSequences.MetroidThree);
                break;

            case TitleSequencePhase.SceneThreeZoom:
                // `$8B:9E8B` increments only on even NMI frame counters.
                if ((phaseTimer++ & 1) == 0 && zoom < TitleSequenceRomData.Scenes.IdentityScale)
                    zoom++;
                if (zoom >= TitleSequenceRomData.Scenes.IdentityScale)
                {
                    activeSpritemap = ReadWord(TitleSequenceRomData.Sprites.LogoPointerAddress);
                    activeOriginX = TitleSequenceRomData.Sprites.LogoX;
                    activeOriginY = TitleSequenceRomData.Sprites.LogoY;
                    activeCharacterOffset = TitleSequenceRomData.Sprites.TitleCharacterOffset.Raw;
                    phase = TitleSequencePhase.TitleLogoFade;
                    phaseTimer = TitleSequenceRomData.Timing.LogoHoldFrames;
                }
                break;

            case TitleSequencePhase.TitleLogoFade:
                if (--phaseTimer <= 0)
                {
                    // `$8B:A0E1` holds the copyright for 32 frames before arming the
                    // 900-frame demo countdown. The palette FX itself remains future work;
                    // its final cartridge palette is already the visible CGRAM source.
                    phase = TitleSequencePhase.CopyrightFade;
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
                    phase = TitleSequencePhase.TitleScreenFadeIn;
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
                    phase = TitleSequencePhase.TitleScreenFadeOut;
                    phaseTimer = TitleSequenceRomData.Timing.TitleFadeCadenceFrames;
                }
                else if (confirmPressed)
                {
                    phase = TitleSequencePhase.TitleScreenFadeOut;
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
        consolePaletteFx.Step(bus, cgram,
            titlePalettePresentation ?? throw new InvalidOperationException(
                "Title palette FX requires installed title colors."), 0, 0, false, false);
        // Native skip reconstruction follows PaletteFxHandler for this frame. Restart
        // only here, not when the subsequent fade-in arms the title idle countdown.
        if (rebuildConsolePaletteFxAfterStep)
            ResetConsolePaletteFx();
    }

    /// <summary>Recreates both native console-lighting programs after a title skip reset.</summary>
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
            SnesMode7Renderer.CompositeViewport(
                background,
                vram,
                cgram,
                matrixScale,
                TitleSequenceRomData.Scenes.Rotation.TableIndex,
                0,
                matrixScale,
                128,
                128,
                unchecked((short)mode7X),
                unchecked((short)mode7Y));
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
            SnesObjRenderer.RenderResolved(oam, vram, cgram,
                TitleSequenceRomData.Sprites.ObjectSizeAndBaseSelector, objects,
                gradientPriorityScratch ??= new byte[background.Length], palettes: palettes);
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
                    0, scale, 128, 128, unchecked((short)mode7X), unchecked((short)mode7Y))
                : null,
            TitleSequenceRomData.Sprites.ObjectSizeAndBaseSelector,
            checked((byte)brightness), gradientEnabled ? ResolveTitleGradient() : default);
    }

    /// <summary>Resolves the installed gradient rows for the current Mode 7 scale.</summary>
    /// <returns>Per-scanline gradient data matching the current title zoom.</returns>
    /// <exception cref="InvalidOperationException">No title-gradient presentation was installed.</exception>
    private ReadOnlySpan<TitleGradientLine> ResolveTitleGradient()
    {
        if (titleGradientPresentation is null)
            throw new InvalidOperationException(
                "Title rendering requires the installed title-gradient asset.");
        return titleGradientPresentation.Resolve((ushort)zoom);
    }

    /// <summary>Builds OAM for the active title sprite and the persistent copyright mark.</summary>
    private void PrepareRenderOam()
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
                activeOriginX, activeOriginY, activeCharacterOffset);
        }

        if (phase is >= TitleSequencePhase.CopyrightFade and <= TitleSequencePhase.TitleScreenFadeOut)
        {
            artwork.DrawSprite(TitleSequenceRomData.Sprites.NintendoCopyright, oam,
                TitleSequenceRomData.Sprites.CopyrightX, TitleSequenceRomData.Sprites.CopyrightY,
                TitleSequenceRomData.Sprites.CopyrightPalette.Raw);
        }

        oam.FinalizeFrame();
    }

    /// <summary>Consumes due entries and commands from the current bank-$8B text instruction list.</summary>
    private void StepTextSequence()
    {
        if (--sequenceEntryTimer > 0)
            return;

        while (true)
        {
            int entryAddress = sequenceEntry;
            ushort durationOrCommand = ReadWord(entryAddress);
            if ((durationOrCommand & TitleSequenceRomData.TextSequences.CommandBit) == 0)
            {
                sequenceEntryTimer = durationOrCommand;
                activeSpritemap = ReadWord(entryAddress + 2);
                sequenceEntry = AddWithinBank(
                    entryAddress,
                    TitleSequenceRomData.TextSequences.TimedEntryByteCount);
                return;
            }

            sequenceEntry = AddWithinBank(
                entryAddress,
                TitleSequenceRomData.TextSequences.InstructionWordByteCount);
            switch (durationOrCommand)
            {
                case CinematicCodePointers.Instruction_TriggerTitleSequenceScene0:
                    phase = TitleSequencePhase.SceneZeroPan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9CE3-$9CE5`.
                    ApplyScene(TitleSequenceRomData.Scenes.SceneZero);
                    return;

                case CinematicCodePointers.Instruction_TriggerTitleSequenceScene1:
                    phase = TitleSequencePhase.SceneOnePan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9D5D-$9D61`.
                    ApplyScene(TitleSequenceRomData.Scenes.SceneOne);
                    return;

                case CinematicCodePointers.Instruction_TriggerTitleSequenceScene2:
                    phase = TitleSequencePhase.SceneTwoPan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9DD6-$9DDA`.
                    ApplyScene(TitleSequenceRomData.Scenes.SceneTwo);
                    return;

                case CinematicCodePointers.Instruction_TriggerTitleSequenceScene3:
                    phase = TitleSequencePhase.SceneThreeZoom;
                    gradientEnabled = true;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9E58-$9E5C`.
                    phaseTimer = 0;
                    ApplyScene(TitleSequenceRomData.Scenes.SceneThree);
                    return;

                case CinematicCodePointers.CinematicSpriteObject_Instruction_Delete:
                    activeSpritemap = TitleSequenceRomData.Sprites.Blank;
                    return;

                default:
                    // The three title-scene lists use the complete command set above.
                    throw new InvalidDataException(
                        $"Title sequence instruction $8B:{durationOrCommand:X4} is invalid for the active retail list.");
            }
        }
    }

    /// <summary>Installs a scrolling text actor and resets its scene-local Mode 7 fractions.</summary>
    /// <param name="definition">Instruction list, initial sprite placement, and phase for the text actor.</param>
    private void BeginTextSequence(TitleTextSequenceDefinition definition)
    {
        phase = definition.Phase;
        sequenceEntry = definition.InstructionAddress;
        sequenceEntryTimer = 1;
        activeSpritemap = TitleSequenceRomData.Sprites.Blank;
        activeOriginX = definition.OriginX;
        activeOriginY = definition.OriginY;
        activeCharacterOffset = definition.CharacterOffset;
        mode7XSubposition = 0;
        mode7YSubposition = 0;

        // Initial PPU setup and each of the first three completed pan functions select
        // TM=$10 before the next text actor becomes visible. Keeping an explicit register
        // bit also matters when Start skips during a pan: the fade-out preserves whichever
        // TM value was live rather than inferring visibility from the host phase name.
        mode7BackgroundEnabled = false;
    }

    /// <summary>Installs the logo and copyright objects used when the opening is skipped.</summary>
    private void EnterImmediateTitleObjects()
    {
        gradientEnabled = true;
        // Skip transition `$8B:9A9C` explicitly overwrites the two glyph colors used by
        // the Nintendo copyright spritemap after restoring CGRAM's upper half.
        cgram.SetColor(
            TitleSequenceRomData.Palette.CopyrightWhiteIndex,
            titlePalettePresentation?.SkipCopyrightWhite ?? TitleSequenceRomData.Palette.CopyrightWhite);
        cgram.SetColor(
            TitleSequenceRomData.Palette.CopyrightRedIndex,
            titlePalettePresentation?.SkipCopyrightRed ?? TitleSequenceRomData.Palette.CopyrightRed);
        activeSpritemap = TitleSequenceRomData.Sprites.SuperMetroidLogo;
        activeOriginX = TitleSequenceRomData.Sprites.LogoX;
        activeOriginY = TitleSequenceRomData.Sprites.LogoY;
        activeCharacterOffset = TitleSequenceRomData.Sprites.TitleCharacterOffset.Raw;
        mode7BackgroundEnabled = true;
        mode7X = 0;
        mode7Y = 0;
        zoom = TitleSequenceRomData.Scenes.IdentityScale;
    }

    /// <summary>Enters the stable title display and starts its native idle countdown.</summary>
    private void EnterTitleScreen()
    {
        EnterImmediateTitleObjects();
        phase = TitleSequencePhase.TitleScreen;
        phaseTimer = TitleSequenceRomData.Timing.TitleScreenNtscFrames;
        brightness = TitleSequenceRomData.Timing.MaximumBrightness;
    }

    /// <summary>Loads extracted Mode 7 character and map streams using the cartridge's interleaved layout.</summary>
    /// <param name="characterBytes">Character source bytes, including the full native DMA length.</param>
    /// <param name="mapBytes">Map source bytes, including the full native DMA length.</param>
    /// <exception cref="InvalidDataException">Either source is shorter than its native transfer.</exception>
    private void LoadMode7InterleavedVram(byte[] characterBytes, byte[] mapBytes)
    {
        if (characterBytes.Length < TitleSequenceRomData.Vram.Mode7CharacterByteCount)
            throw new InvalidDataException("Title Mode 7 character stream is shorter than its $4000-byte DMA.");
        if (mapBytes.Length < TitleSequenceRomData.Vram.Mode7MapByteCount)
            throw new InvalidDataException("Title Mode 7 map stream is shorter than its $1000-byte DMA.");

        // DMA mode zero to $2119 writes only high bytes and increments VMADD afterward.
        // The following $2118 fill and map DMA populate the corresponding low bytes.
        vram.FillMode7MapBytes(
            TitleSequenceRomData.Vram.Mode7InitialMapByte,
            TitleSequenceRomData.Vram.Mode7CharacterByteCount);
        vram.LoadMode7CharacterBytes(
            characterBytes.AsSpan(0, TitleSequenceRomData.Vram.Mode7CharacterByteCount));
        vram.LoadMode7MapBytes(
            mapBytes.AsSpan(0, TitleSequenceRomData.Vram.Mode7MapByteCount));
    }

    /// <summary>Advances the baby Metroid source-page animation when its timer expires.</summary>
    private void StepBabyMetroidAnimation()
    {
        if (--babyFrameTimer > 0)
            return;

        babyFrame = (babyFrame + 1) % TitleSequenceRomData.Vram.BabyAnimationSourcePages.Length;
        babyFrameTimer = TitleSequenceRomData.Timing.BabyFrameDuration;
        UpdateBabyMetroidCharacterFrame();
    }

    /// <summary>Copies the current baby Metroid character page into its Mode 7 VRAM destination.</summary>
    /// <exception cref="InvalidDataException">The installed character stream lacks the selected page.</exception>
    private void UpdateBabyMetroidCharacterFrame()
    {
        // The instruction list cycles source pages 0,1,2,1 into Mode 7 destination word
        // `$3800`, high byte only. Each page is exactly $100 bytes / four characters.
        int sourcePage = TitleSequenceRomData.Vram.BabyAnimationSourcePages[babyFrame];
        int source = sourcePage * TitleSequenceRomData.Vram.BabyCharacterPageByteCount;
        if (babyMetroidCharacters.Length <
            source + TitleSequenceRomData.Vram.BabyCharacterPageByteCount)
            throw new InvalidDataException("Title baby-Metroid stream is missing an animation page.");

        for (int byteIndex = 0;
             byteIndex < TitleSequenceRomData.Vram.BabyCharacterPageByteCount;
             byteIndex++)
        {
            int word = TitleSequenceRomData.Vram.BabyCharacterDestinationWord + byteIndex;
            ushort value = (ushort)(
                (vram.ReadWord(word) & TitleSequenceRomData.Vram.Mode7MapLowByteMask) |
                (babyMetroidCharacters[source + byteIndex] <<
                    TitleSequenceRomData.Vram.Mode7CharacterByteShift));
            vram.ExecuteWordTransfer([value], (ushort)word, 1);
        }
    }

    /// <summary>Reads one little-endian instruction word from the compiled title sequence.</summary>
    /// <param name="address">Address of the word in the title sequence's bank.</param>
    /// <returns>The decoded 16-bit word.</returns>
    private static ushort ReadWord(int address) => TitleSequenceInstructionDefinitions.ReadWord(address);

    /// <summary>Advances a compiled instruction address without carrying into another ROM bank.</summary>
    /// <param name="address">Current bus address.</param>
    /// <param name="bytes">Byte distance to advance within the bank.</param>
    /// <returns>The wrapped address after the advance.</returns>
    private static int AddWithinBank(int address, int bytes) =>
        (int)SnesAddress.FromBusAddress(address).AddWithinBank(bytes);

    /// <summary>Adds a signed 16.16 delta to a signed integer and unsigned fractional pair.</summary>
    /// <param name="integer">Integer component, updated with 16-bit signed wrap semantics.</param>
    /// <param name="fraction">Fractional low word, updated with 16-bit unsigned wrap semantics.</param>
    /// <param name="delta16Point16">Signed 16.16 increment applied to the pair.</param>
    private static void AddFixedPoint(ref int integer, ref int fraction, int delta16Point16)
    {
        long combined = ((long)integer << 16) | (ushort)fraction;
        combined += delta16Point16;
        integer = unchecked((short)(combined >> 16));
        fraction = (ushort)combined;
    }

    /// <summary>Loads a scene's Mode 7 transform and hides any scrolling text sprite.</summary>
    /// <param name="definition">Compiled scene phase, scale, and starting offsets.</param>
    private void ApplyScene(TitleMode7SceneDefinition definition)
    {
        phase = definition.Phase;
        mode7BackgroundEnabled = true;
        zoom = definition.Scale;
        mode7X = definition.HorizontalOffset;
        mode7Y = definition.VerticalOffset;
        activeSpritemap = TitleSequenceRomData.Sprites.Blank;
    }

    /// <summary>Scales RGB channels by the title master-brightness level while preserving alpha.</summary>
    /// <param name="color">Pixel to shade.</param>
    /// <param name="level">Brightness from zero through the native maximum.</param>
    /// <returns>The shaded pixel, with transparent pixels left untouched.</returns>
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

/// <summary>Debugger-facing names for the visible title-sequence functions.</summary>
public enum TitleSequencePhase
{
    /// <summary>Runs the $8B:A03D progressive 1994 title card over black, including its initial blank hold, before triggering the lower-background pan.</summary>
    YearText,
    /// <summary>Ports $8B:9D17: pans the lower Mode 7 scene left by 1.5 pixels per update until X falls below -7 pixels, then starts the Nintendo card.</summary>
    SceneZeroPan,
    /// <summary>Runs the $8B:A055 progressive Nintendo title card with the Mode 7 background hidden, then triggers the upper-background pan.</summary>
    NintendoText,
    /// <summary>Ports $8B:9D90: pans the upper Mode 7 scene left by 1.5 pixels per update until X falls below -176 pixels, then starts the Presents card.</summary>
    SceneOnePan,
    /// <summary>Runs the $8B:A079 progressive Presents title card over black before triggering the downward-background pan.</summary>
    PresentsText,
    /// <summary>Ports $8B:9E12: pans the Mode 7 scene down by 1.5 pixels per update until Y reaches 163 pixels, then starts the Metroid 3 card.</summary>
    SceneTwoPan,
    /// <summary>Runs the $8B:A09D progressive Metroid 3 title card, including its 120-update final hold, before triggering the final zoom.</summary>
    MetroidThreeText,
    /// <summary>Ports $8B:9E8B: enables the title gradient and zooms out by increasing the native 8.8 Mode 7 scale from $0043 to unity $0100 on alternating updates.</summary>
    SceneThreeZoom,
    /// <summary>Displays the title logo at the completed Mode 7 transform for the 32-update $8B:A0C5 logo-list stage before showing the copyright.</summary>
    TitleLogoFade,
    /// <summary>Displays the logo and Nintendo copyright for the 32-update $8B:A0E1 copyright-list stage before starting the idle-title demo countdown.</summary>
    CopyrightFade,
    /// <summary>Ports the $8B:9A83 skip fade: lowers brightness by two levels per update, then installs the immediate title objects and queues track six at black.</summary>
    SkipFadeOut,
    /// <summary>Ports the $8B:9B53 immediate-title fade-in after skipping the opening or returning from a demo; raises brightness by two levels per update before starting the idle countdown.</summary>
    TitleScreenFadeIn,
    /// <summary>Ports $8B:9F29: waits up to 900 NTSC title updates for a new B, Start, or A press; countdown expiration takes precedence and selects a demo.</summary>
    TitleScreen,
    /// <summary>Combines the $8B:9F52 file-select and $8B:9FAE demo transitions, reducing brightness by one level every two updates and requesting the selected destination at black.</summary>
    TitleScreenFadeOut,
}
