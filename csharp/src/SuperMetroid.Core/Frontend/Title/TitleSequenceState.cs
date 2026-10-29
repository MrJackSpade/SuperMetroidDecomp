using SuperMetroid.Core.Rooms.Backgrounds;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

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
            BindTitleGraphics(artwork.MapPresentation?.TitleGraphics);
            BindTitlePalette(artwork.MapPresentation?.TitlePalette);
        }
        BindSpriteSheets(artwork.SpriteSheets);
    }

    /// <summary>Binds the installed sprite sheets the title's compositions draw from.</summary>
    [MemberNotNull(nameof(spriteSheets))]
    internal void BindSpriteSheets(InstalledSpriteSheets? sheets)
    {
        spriteSheets = sheets ?? throw new InvalidOperationException("Title sequence requires the installed sprite sheets.");
        floorTiles = null;
    }

    /// <summary>What the Mode 7 map shows with each Baby Metroid page loaded (#1392), once read; artwork, never saved.</summary>
    [NonSerialized] private Dictionary<int, FloorTilePictures>? floorTiles;

    /// <summary>What the Mode 7 map shows with Baby Metroid page <paramref name="page"/> loaded.</summary>
    private FloorTilePictures FloorTiles(int page)
    {
        floorTiles ??= [];
        if (!floorTiles.TryGetValue(page, out FloorTilePictures? tiles))
            floorTiles.Add(page, tiles = TitleScreenTiles.Of(spriteSheets, page));
        return tiles;
    }

    /// <summary>The installed sprite sheets the title's compositions draw from; artwork, never saved.</summary>
    [NonSerialized] private InstalledSpriteSheets spriteSheets;

    /// <summary>Queues the opening music and title transitions when audio is available.</summary>
    private readonly AudioQueues? audio;
    /// <summary>The title's colors: the installed palette and live console palette-FX changes (#1358).</summary>
    internal readonly ScreenPalette palette = new();
    /// <summary>The floor's cells as the bound artwork lays them out, until it is rebound.</summary>
    [NonSerialized] private byte[]? floorMap;
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
    [field: NonSerialized] internal TitleGraphicsPresentation? TitleGraphics { get; private set; }
    /// <summary>Countdown for the active title phase, including fade and idle stages.</summary>
    internal int PhaseTimer { get; private set; }
    /// <summary>The title card whose steps are running.</summary>
    private TitleSequencePhase textCard = TitleSequencePhase.YearText;
    /// <summary>The next step of the running title card.</summary>
    private int textCardStep;
    /// <summary>Frames left before the next title text list entry is interpreted.</summary>
    private int sequenceEntryTimer;
    /// <summary>Composition selected by the active title text or logo entry, or null while nothing is shown.</summary>
    internal TitleSprite? ActiveSprite { get; set; }
    /// <summary>Screen origin used when drawing the active title spritemap.</summary>
    internal Position16<ScreenPixels> ActiveOrigin { get; set; }
    /// <summary>Object attributes (the OBJ palette) applied to every part of the active composition.</summary>
    internal int ActiveObjectPalette { get; set; }
    /// <summary>The floor's pan position, whose whole words are interpreted as signed pixels.</summary>
    private FixedPosition<FloorPixels> panPosition;
    /// <summary>Native 8.8 Mode 7 matrix scale animated from the close-up scene to identity.</summary>
    internal int Zoom { get; private set; }
    /// <summary>Current SNES screen brightness applied after composing title layers.</summary>
    internal int Brightness { get; private set; }
    /// <summary>Index into the Baby Metroid character page cycle.</summary>
    internal int BabyFrame { get; private set; }
    /// <summary>Frames left before the next Baby Metroid character page transfer.</summary>
    private int babyFrameTimer;
    /// <summary>Whether the title shows its perspective floor behind the title objects.</summary>
    private bool floorBackgroundEnabled;
    /// <summary>Enables the installed scanline gradient during the final title stages.</summary>
    private bool gradientEnabled;
    /// <summary>Selects the demo destination after the shared title fade-out.</summary>
    private bool fadingToDemo;

    /// <summary>Creates the native initial title setup performed by <c>$8B:9B68</c>.</summary>
    /// <param name="audio">Receives the opening music and title transitions when audio is available.</param>
    /// <param name="titleGradientPresentation">The installed scanline gradient.</param>
    /// <param name="titlePalettePresentation">Required installed title colours.</param>
    /// <param name="titleGraphicsPresentation">Required installed title graphics.</param>
    /// <param name="spriteSheets">Required installed sprite sheets the title's compositions draw from.</param>
    public TitleSequenceState(
        AudioQueues? audio = null,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null,
        InstalledSpriteSheets? spriteSheets = null)
        : this(audio, openingMusic: TitleOpeningMusic.Queue, titleGradientPresentation, titlePalettePresentation, titleGraphicsPresentation,
            spriteSheets)
    {
    }

    /// <summary>Builds the initial title VRAM and palette state, optionally queuing opening music for a new session.</summary>
    private TitleSequenceState(
        AudioQueues? audio,
        TitleOpeningMusic openingMusic,
        TitleGradientPresentation? titleGradientPresentation,
        TitlePalettePresentation? titlePalettePresentation,
        TitleGraphicsPresentation? titleGraphicsPresentation,
        InstalledSpriteSheets? spriteSheets)
    {
        this.audio = audio;
        BindSpriteSheets(spriteSheets);
        this.titleGradientPresentation = titleGradientPresentation;
        this.titlePalettePresentation = titlePalettePresentation;
        TitleGraphics = titleGraphicsPresentation ?? throw new InvalidOperationException(
            "Title sequence requires installed graphics presentation assets.");
        if (titlePalettePresentation is null)
            throw new InvalidOperationException(
                "Title sequence requires installed palette presentation assets.");
        if (openingMusic == TitleOpeningMusic.Queue)
        {
            audio?.QueueMusicDelayed8(
                MusicRequest.LoadDataSet(MusicDataSet.TitleSequence));
            audio?.QueueMusicDelayed8(
                MusicRequest.SelectTrack(TitleSequenceDefinitions.Music.OpeningTrack));
        }

        // `$8B:9B87` expands the four artwork streams and uploads them; the title binds that
        // artwork and its capture places the Mode 7 planes, OBJ characters and Baby page.
        objects = new ScreenSprites();
        titlePalettePresentation.Apply(palette);
        ResetConsolePaletteFx();

        // The Year object's pre-instruction forces full brightness on the first
        // processing frame; its position and palette come from the native definition.
        Brightness = TitleSequenceDefinitions.Timing.MaximumBrightness;
        BeginTextSequence(TitleSequenceDefinitions.TextSequences.Year);
    }

    /// <summary>Current native title sub-state, exposed as a stable debugger label.</summary>
    public TitleSequencePhase Phase { get; internal set; }

    /// <summary>True after the title's slow fade has handed control to file select.</summary>
    public bool FileSelectRequested { get; private set; }
    /// <summary>True after the idle timeout's slow fade reaches native demo state $28.</summary>
    public bool DemoRequested { get; private set; }

    /// <summary>State $2C's player-cancelled return skips the introductory title cards.</summary>
    internal static TitleSequenceState ReturnFromDemo(
        AudioQueues audio,
        TitleGradientPresentation? titleGradientPresentation = null,
        TitlePalettePresentation? titlePalettePresentation = null,
        TitleGraphicsPresentation? titleGraphicsPresentation = null,
        InstalledSpriteSheets? spriteSheets = null)
    {
        var title = new TitleSequenceState(
            audio,
            openingMusic: TitleOpeningMusic.Skip,
            titleGradientPresentation,
            titlePalettePresentation,
            titleGraphicsPresentation,
            spriteSheets);
        title.EnterImmediateTitleObjects();
        title.Brightness = 0;
        title.Phase = TitleSequencePhase.TitleScreenFadeIn;
        return title;
    }

    /// <summary>Rebinds the host-selected presentation after debugger-state restoration.</summary>
    internal void BindTitleGradient(TitleGradientPresentation? presentation) =>
        titleGradientPresentation = presentation;

    /// <summary>Rebinds installed sprite compositions after debugger-state restoration.</summary>
    internal void BindTitleGraphics(TitleGraphicsPresentation? presentation)
    {
        TitleGraphics = presentation;
        floorMap = null;
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
            WriteCopyright(
                presentation?.SkipCopyrightWhite ?? TitleSequenceDefinitions.Palette.CopyrightWhite,
                presentation?.SkipCopyrightRed ?? TitleSequenceDefinitions.Palette.CopyrightRed);
        }
    }

    /// <summary>Runs one accepted title-sequence frame.</summary>
    public void Step(ControllerButton controllerInput)
    {
        controller.Latch(controllerInput);
        bool rebuildConsolePaletteFxAfterStep = false;

        // `$8B:9A48` admits B, Start, or A during every pre-title cinematic function. The
        // original performs a fast fade-out, reconstructs the final title objects, then a
        // fast fade-in. Do not jump straight to a host menu: those intermediate frames and
        // their brightness levels are observable in native traces.
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
                panPosition = panPosition.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(
                    panPosition.X, -TitleSequenceDefinitions.Scenes.PanVelocity16Point16));
                if (unchecked((short)panPosition.X.Whole) < TitleSequenceDefinitions.Scenes.SceneZeroEndX)
                    BeginTextSequence(TitleSequenceDefinitions.TextSequences.Nintendo);
                break;

            case TitleSequencePhase.SceneOnePan:
                panPosition = panPosition.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(
                    panPosition.X, -TitleSequenceDefinitions.Scenes.PanVelocity16Point16));
                if (unchecked((short)panPosition.X.Whole) < TitleSequenceDefinitions.Scenes.SceneOneEndX)
                    BeginTextSequence(TitleSequenceDefinitions.TextSequences.Presents);
                break;

            case TitleSequencePhase.SceneTwoPan:
                panPosition = panPosition.WithY(IntroCinematicMotion.AddSignedSixteenSixteen(
                    panPosition.Y, TitleSequenceDefinitions.Scenes.PanVelocity16Point16));
                if (unchecked((short)panPosition.Y.Whole) >= TitleSequenceDefinitions.Scenes.SceneTwoEndY)
                    BeginTextSequence(TitleSequenceDefinitions.TextSequences.MetroidThree);
                break;

            case TitleSequencePhase.SceneThreeZoom:
                // `$8B:9E8B` increments only on even NMI frame counters.
                if ((PhaseTimer++ & 1) == 0 && Zoom < TitleSequenceDefinitions.Scenes.IdentityScale)
                    Zoom++;
                if (Zoom >= TitleSequenceDefinitions.Scenes.IdentityScale)
                {
                    ActiveSprite = TitleSprite.SuperMetroidLogo;
                    ActiveOrigin = TitleSequenceDefinitions.Sprites.LogoOrigin;
                    ActiveObjectPalette = TitleSequenceDefinitions.Sprites.LogoPalette;
                    Phase = TitleSequencePhase.TitleLogoFade;
                    PhaseTimer = TitleSequenceDefinitions.Timing.LogoHoldFrames;
                }
                break;

            case TitleSequencePhase.TitleLogoFade:
                if (--PhaseTimer <= 0)
                {
                    // `$8B:A0E1` holds the copyright for 32 frames before arming the
                    // 900-frame demo countdown. The palette FX itself remains future work;
                    // its final cartridge palette is already the visible CGRAM source.
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
                    // HandleCinematicsTransitions_1 at $8B:9A83 queues track six before
                    // rebuilding the immediate title objects.
                    audio?.QueueMusicDelayed8(
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
                // The native timeout check wins over confirmation on its final frame.
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
        // The title calls the same bank-$8D interpreter as rooms. Its two console
        // programs own their colors and timers; the host must not synthesize a blink.
        consolePaletteFx.Step(new ScreenPaletteFxColours(palette), titlePalettePresentation ?? throw new InvalidOperationException(
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
        consolePaletteFx.SpawnDefinition(TitleSequenceDefinitions.ConsolePaletteFx.SlowLights, 0);
        consolePaletteFx.SpawnDefinition(TitleSequenceDefinitions.ConsolePaletteFx.FastLights, 0);
    }

    /// <summary>
    /// Builds the cartridge's OAM on the simulation owner and captures its display.
    /// No pixels are composed, and the returned value retains no live scene references.
    /// Call at the same display boundary as Render, before subsequent state changes.
    /// </summary>
    public FloorScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        return new(new FramePaint(palette.Shown(), objects.Draws),
            floorBackgroundEnabled
                ? new AffineTransform(unchecked((short)Zoom), TitleSequenceDefinitions.Scenes.Rotation.TableIndex,
                    0, unchecked((short)Zoom), new Offset16<FloorPixels>(128, 128), panPosition.Whole.AsSignedWord())
                : null,
            checked((byte)Brightness), gradientEnabled ? ResolveTitleGradient() : default,
            floorBackgroundEnabled ? AffineFloor.Of(FloorMap(), FloorTiles(TitleSequenceDefinitions.Graphics.BabyAnimationSourcePages[BabyFrame])) : null);
    }

    /// <summary>
    /// The floor's cells: the installed map over the initial tile every other cell keeps, as the title's setup fills the
    /// map and loads the installed map over its start.
    /// </summary>
    /// <exception cref="InvalidOperationException">No title artwork is installed.</exception>
    /// <exception cref="InvalidDataException">The installed map is shorter than the map the title loads.</exception>
    private byte[] FloorMap()
    {
        if (floorMap is not null)
            return floorMap;
        ReadOnlySpan<byte> map = (TitleGraphics ?? throw new InvalidOperationException("Title rendering requires installed title artwork.")).FloorMap;
        if (map.Length < TitleSequenceDefinitions.Graphics.FloorMapByteCount)
            throw new InvalidDataException("Title Mode 7 map stream is shorter than its $1000-byte DMA.");
        var cells = new byte[AffineFloor.CellCount];
        cells.AsSpan().Fill(TitleSequenceDefinitions.Graphics.FloorInitialMapByte);
        map[..TitleSequenceDefinitions.Graphics.FloorMapByteCount].CopyTo(cells);
        return floorMap = cells;
    }

    /// <summary>Resolves the installed title gradient at the current Mode 7 scale.</summary>
    private ReadOnlySpan<TitleGradientLine> ResolveTitleGradient()
    {
        if (titleGradientPresentation is null)
            throw new InvalidOperationException(
                "Title rendering requires the installed title-gradient asset.");
        return titleGradientPresentation.Resolve((ushort)Zoom);
    }

    /// <summary>Places the active title spritemap and stage-dependent copyright as the frame's sprite entries.</summary>
    internal void PrepareRenderSprites()
    {
        SpriteSheet sheet = spriteSheets.Get(SpriteSheetId.Title);
        objects.BeginFrame();
        if (ActiveSprite is { } sprite)
        {
            // Cinematic drawing calls `$81:879F`, whose `chr_r22` replaces palette bits
            // after masking the ROM attributes with `$F1FF`. It does not add a base tile;
            // confusing it with the enemy loader makes the title art uniformly blue.
            TitleGraphicsPresentation.DrawSprite(sheet, sprite, objects,
                ActiveOrigin.ToSpritePosition(), ActiveObjectPalette);
        }

        if (Phase is TitleSequencePhase.CopyrightFade or TitleSequencePhase.SkipFadeOut or TitleSequencePhase.TitleScreenFadeIn or
            TitleSequencePhase.TitleScreen or TitleSequencePhase.TitleScreenFadeOut)
        {
            TitleGraphicsPresentation.DrawSprite(sheet, TitleSprite.NintendoCopyright, objects,
                TitleSequenceDefinitions.Sprites.CopyrightOrigin.ToSpritePosition(),
                TitleSequenceDefinitions.Sprites.CopyrightPalette);
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
                ActiveSprite = step.Sprite;
                return;
            }

            switch (command)
            {
                case TitleSequenceInstruction.TriggerScene0:
                    Phase = TitleSequencePhase.SceneZeroPan;
                    floorBackgroundEnabled = true; // `$8B:9CE3-$9CE5` shows the floor beside the objects.
                    ApplyScene(TitleSequenceDefinitions.Scenes.SceneZero);
                    return;

                case TitleSequenceInstruction.TriggerScene1:
                    Phase = TitleSequencePhase.SceneOnePan;
                    floorBackgroundEnabled = true; // `$8B:9D5D-$9D61` shows the floor beside the objects.
                    ApplyScene(TitleSequenceDefinitions.Scenes.SceneOne);
                    return;

                case TitleSequenceInstruction.TriggerScene2:
                    Phase = TitleSequencePhase.SceneTwoPan;
                    floorBackgroundEnabled = true; // `$8B:9DD6-$9DDA` shows the floor beside the objects.
                    ApplyScene(TitleSequenceDefinitions.Scenes.SceneTwo);
                    return;

                case TitleSequenceInstruction.TriggerScene3:
                    Phase = TitleSequencePhase.SceneThreeZoom;
                    gradientEnabled = true;
                    floorBackgroundEnabled = true; // `$8B:9E58-$9E5C` shows the floor beside the objects.
                    PhaseTimer = 0;
                    ApplyScene(TitleSequenceDefinitions.Scenes.SceneThree);
                    return;

                case TitleSequenceInstruction.Delete:
                    ActiveSprite = null;
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
        ActiveSprite = null;
        ActiveOrigin = definition.Origin;
        ActiveObjectPalette = definition.Palette;
        panPosition = new(panPosition.X.WithFraction(0), panPosition.Y.WithFraction(0));

        // Initial setup and each of the first three completed pan functions hide the floor,
        // showing objects alone, before the next text actor becomes visible. Keeping the
        // visibility explicit also matters when Start skips during a pan: the fade-out keeps
        // whichever visibility was live rather than inferring it from the host phase name.
        floorBackgroundEnabled = false;
    }

    /// <summary>Writes the copyright glyph's white and red colours.</summary>
    private void WriteCopyright(RgbColor white, RgbColor red)
    {
        palette.Colours(ScreenColourGroup.TitleCopyrightWhite).Fill(white);
        palette.Colours(ScreenColourGroup.TitleCopyrightRed).Fill(red);
    }

    /// <summary>Installs the logo, gradient, copyright colors, and identity Mode 7 scene used after a skip.</summary>
    private void EnterImmediateTitleObjects()
    {
        gradientEnabled = true;
        // Skip transition `$8B:9A9C` explicitly overwrites the two glyph colors used by
        // the Nintendo copyright spritemap after restoring CGRAM's upper half.
        WriteCopyright(
            titlePalettePresentation?.SkipCopyrightWhite ?? TitleSequenceDefinitions.Palette.CopyrightWhite,
            titlePalettePresentation?.SkipCopyrightRed ?? TitleSequenceDefinitions.Palette.CopyrightRed);
        ActiveSprite = TitleSprite.SuperMetroidLogo;
        ActiveOrigin = TitleSequenceDefinitions.Sprites.LogoOrigin;
        ActiveObjectPalette = TitleSequenceDefinitions.Sprites.LogoPalette;
        floorBackgroundEnabled = true;
        panPosition = new(panPosition.X.WithWhole(0), panPosition.Y.WithWhole(0));
        Zoom = TitleSequenceDefinitions.Scenes.IdentityScale;
    }

    /// <summary>Opens the idle title screen at full brightness with its native demo countdown.</summary>
    private void EnterTitleScreen()
    {
        EnterImmediateTitleObjects();
        Phase = TitleSequencePhase.TitleScreen;
        PhaseTimer = TitleSequenceDefinitions.Timing.TitleScreenNtscFrames;
        Brightness = TitleSequenceDefinitions.Timing.MaximumBrightness;
    }

    /// <summary>Advances the Baby Metroid page cycle when its authored frame duration expires.</summary>
    private void StepBabyMetroidAnimation()
    {
        if (--babyFrameTimer > 0)
            return;

        BabyFrame = (BabyFrame + 1) % TitleSequenceDefinitions.Graphics.BabyAnimationSourcePages.Length;
        babyFrameTimer = TitleSequenceDefinitions.Timing.BabyFrameDuration;
    }

    /// <summary>Installs a compiled Mode 7 scene's position and scale while clearing the previous text sprite.</summary>
    private void ApplyScene(TitleFloorSceneDefinition definition)
    {
        Phase = definition.Phase;
        floorBackgroundEnabled = true;
        Zoom = definition.Scale;
        panPosition = new(panPosition.X.WithWhole(unchecked((ushort)definition.Position.X)),
            panPosition.Y.WithWhole(unchecked((ushort)definition.Position.Y)));
        ActiveSprite = null;
    }
}
