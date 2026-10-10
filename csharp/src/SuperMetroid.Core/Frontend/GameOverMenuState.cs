using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>Retail game-over prompt at <c>$81:90AE-$81:93F7</c>.</summary>
public sealed class GameOverMenuState
{
    // OBJ layer reused across renders; the span overload clears it first. Never saved state.
    [NonSerialized] private Rgba32[]? objectLayerScratch;
    // Final frame, reused by every render: a returned frame is valid until this scene renders again.
    [NonSerialized] private Rgba32[]? frameBuffer;

    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState audio;
    private readonly MenuPpuState ppu;
    private readonly OamBuffer oam = new();
    private readonly ControllerInputState controller = new();
    private int brightness;
    private int missileTimer = 1;
    private int missileFrame;
    private ushort babyInstructionPointer;
    private ushort babyInstructionTimer;
    private ushort babySpritemap = GameOverRomData.BabyAnimation.InitialSpritemap;
    private readonly bool continueLoadsCeresArrival;
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;

    /// <summary>Creates the game-over scene with installed artwork and its BG1 tilemap, leaving music, animation, and fade initialization for the first update.</summary>
    /// <param name="bus">Address-space context passed to the menu PPU and later presentation rebinding.</param>
    /// <param name="audio">Shared cartridge-style music and sound queue used by menu updates.</param>
    /// <param name="mapPresentation">Required installed presentation bundle, despite the compatibility default of null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> or <paramref name="audio"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No installed presentation bundle is supplied.</exception>
    /// <param name="continueLoadsCeresArrival">
    /// True when <c>SRAMMirror_LoadingGameState</c> is <c>$1F</c>: <c>$81:915D</c> then answers Yes by
    /// publishing the Ceres loader immediately instead of fading into the area map.
    /// </param>
    public GameOverMenuState(
        ISnesAddressSpace bus,
        CartridgeAudioState audio,
        AreaMapPresentationCatalog? mapPresentation = null,
        bool continueLoadsCeresArrival = false)
    {
        this.continueLoadsCeresArrival = continueLoadsCeresArrival;
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
        this.mapPresentation = mapPresentation ?? throw new InvalidOperationException(
            "Game-over screen requires installed presentation assets.");
        ppu = new MenuPpuState(bus, mapPresentation.Tiles, mapPresentation.Palettes,
            mapPresentation.WorldArtwork, mapPresentation.Sprites,
            loadInitialBackground: false);
        mapPresentation.GameOver.LoadTilemapTo(ppu.Vram, MenuPpuState.Bg1TilemapWord);
        Phase = GameOverMenuPhase.ConfigureGraphics;
    }

    /// <summary>Zero selects Yes; one selects No, matching <c>file_select_map_area_index</c>.</summary>
    public int SelectedItem { get; private set; }

    /// <summary>Current native menu index; these enum ordinals are not native menu-index words.</summary>
    public GameOverMenuPhase Phase { get; private set; }

    /// <summary>
    /// True when the current menu index stopped at its own NMI wait (<c>$81:8D1D</c> or
    /// <c>$81:9260</c>); the next update finishes that index instead of entering MainGameLoop.
    /// </summary>
    public bool ResumesAfterNmiWait { get; private set; }

    /// <summary>Sticky request raised when the accepted Yes answer has faded fully to black; the outer dispatcher opens file-select map view rather than reloading the save inside this scene.</summary>
    public bool ContinueRequested { get; private set; }

    /// <summary>
    /// Sticky request raised on the frame Yes is accepted while the save resumes at the Ceres
    /// elevator: <c>$81:9171</c> stores game state <c>$1F</c> and reloads SRAM without fading.
    /// </summary>
    public bool CeresArrivalRequested { get; private set; }

    /// <summary>Sticky request raised when the accepted No answer has faded fully to black; the outer dispatcher enters its soft-reset path toward the title.</summary>
    public bool TitleRequested { get; private set; }

    /// <summary>
    /// Reattaches host-owned presentation after debugger restoration and refreshes only
    /// presentation-owned PPU state. Animation phase, answer selection and fade remain live.
    /// </summary>
    public void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        string? previousIdentity = mapPresentation?.ContentIdentity;
        mapPresentation = catalog ?? throw new InvalidOperationException(
            "Game-over screen requires installed presentation assets.");
        if (string.Equals(previousIdentity, catalog.ContentIdentity, StringComparison.Ordinal))
            return;

        ppu.BindMapTiles(catalog.Tiles);
        ppu.BindMapSprites(catalog.Sprites);
        ppu.BindMapPalettes(catalog.Palettes);
        catalog.GameOver.LoadTilemapTo(ppu.Vram, MenuPpuState.Bg1TilemapWord);
        if (babyInstructionPointer != 0)
        {
            GameOverBabyInstruction instruction =
                GameOverBabyAnimationDefinitions.Get(babyInstructionPointer);
            catalog.GameOver.ApplyBabyPalette(ppu.Cgram, instruction.Palette);
        }
    }

    /// <summary>Advances one menu update, latching input edges, animating the cursor and Baby, and progressing the music wait and one-level-per-update brightness fades.</summary>
    /// <param name="controllerInput">Current held-button word in native <see cref="SnesButton"/> bit layout; only newly pressed buttons affect the interactive menu.</param>
    /// <remarks>Select, Up, or Down toggles the answer and takes priority over A confirmation in the same update. Accepting either answer holds the current Baby frame for 180 updates while fading out.</remarks>
    public void Step(ushort controllerInput)
    {
        controller.Latch(controllerInput);
        if (ResumesAfterNmiWait)
        {
            // Both waits are the last call of their index; the remainder only configures
            // presentation and advances the index.
            ResumesAfterNmiWait = false;
            Phase = Phase switch
            {
                GameOverMenuPhase.ConfigureGraphics => GameOverMenuPhase.Initialize,
                GameOverMenuPhase.Initialize => GameOverMenuPhase.WaitForInitialMusic,
                _ => throw new InvalidOperationException($"Game-over phase {Phase} has no NMI wait."),
            };
            return;
        }

        SnesButton pressed = controller.NewlyPressedButtons;
        // Only indexes three, four, five and seven draw the selection missile.
        if (Phase is GameOverMenuPhase.FadeIn or GameOverMenuPhase.Main or
            GameOverMenuPhase.FadeOutToContinue or GameOverMenuPhase.FadeOutToTitle)
            StepMissileAnimation();

        switch (Phase)
        {
            case GameOverMenuPhase.ConfigureGraphics:
                // State $19 hands over at brightness zero, so HandleFadingOut leaves the screen
                // black and $81:8D1D blanks it and waits for NMI before configuring the menu.
                if (brightness != 0)
                    throw new InvalidOperationException("Game-over menu must begin from a black screen.");
                audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: GameOverRomData.MaximumQueuedSounds);
                ResumesAfterNmiWait = true;
                break;

            case GameOverMenuPhase.Initialize:
                audio.QueueMusicDelayed8(MusicCommand.Stop);
                audio.QueueMusicDelayed8(MusicCommand.LoadData(GameOverRomData.Music.DataIndex));
                babyInstructionPointer = 0;
                babyInstructionTimer = 0;
                StepBabyMetroid();
                SelectedItem = 0;
                brightness = 0;
                // $81:9260 unblanks and waits for NMI before advancing the index.
                ResumesAfterNmiWait = true;
                break;

            case GameOverMenuPhase.WaitForInitialMusic:
                // $81:93E8 only polls the music queue; it neither animates the Baby nor draws the missile.
                if (!audio.HasQueuedMusic)
                {
                    audio.QueueMusicDelayed8(
                        MusicCommand.SelectTrack(GameOverRomData.Music.TrackIndex));
                    Phase = GameOverMenuPhase.FadeIn;
                }
                break;

            case GameOverMenuPhase.FadeIn:
                StepBabyMetroid();
                brightness = Math.Min(GameOverRomData.MaximumBrightness, brightness + 1);
                if (brightness == GameOverRomData.MaximumBrightness)
                    Phase = GameOverMenuPhase.Main;
                break;

            case GameOverMenuPhase.Main:
                StepBabyMetroid();
                if ((pressed & (SnesButton.Select | SnesButton.Up | SnesButton.Down)) != 0)
                {
                    audio.QueueSound(
                        SoundEffectLibrary1Sounds.MenuCursor,
                        maximumQueued: GameOverRomData.MaximumQueuedSounds);
                    SelectedItem ^= 1;
                }
                else if ((pressed & SnesButton.A) != 0)
                {
                    // `$81:914B` holds the current Baby frame for 180 ticks once either
                    // answer is accepted, while the selected fade owner continues drawing.
                    babyInstructionTimer =
                        GameOverRomData.BabyAnimation.AcceptedAnswerHoldDuration;
                    if (SelectedItem == 0 && continueLoadsCeresArrival)
                        CeresArrivalRequested = true;
                    else
                        Phase = SelectedItem == 0
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

    /// <summary>Composites the current BG1, Baby, egg, and selection cursor into a brightness-filtered 256x224 frame without advancing menu timing.</summary>
    /// <returns>The scene-owned row-major RGBA buffer, valid only until this scene renders again; copy it before retaining a frame across later renders.</returns>
    public Rgba32[] Render()
    {
        // `TM=$11` enables BG1 and OBJ only. Game-over does not retain the menu starfield.
        Rgba32[] output = SnesLayerCompositor.CreateBackdrop(ppu.Cgram, FrontendFrame.Width * FrontendFrame.Height, frameBuffer ??= new Rgba32[FrontendFrame.Width * FrontendFrame.Height]);
        SnesBgTilemapRenderer.Composite4BppViewport(
            output,
            ppu.Vram, ppu.Cgram, MenuPpuState.Bg1TilemapWord, 0, 0, 0,
            FrontendFrame.Width, FrontendFrame.Height,
            GameOverRomData.TilemapWidth, GameOverRomData.TilemapHeight);

        PrepareRenderOam();
        Rgba32[] objectLayer = objectLayerScratch ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
        SnesObjRenderer.Render(objectLayer, oam, ppu.Vram, ppu.Cgram, obsel: MenuRenderDefinitions.ObjectSelection);
        SnesLayerCompositor.Composite(output,
            objectLayer);
        MasterBrightnessFilter.Apply(output, (byte)brightness);
        return output;
    }

    /// <summary>Captures BG1/OBJ only; the game-over PPU does not enable BG2.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderOam();
        RenderLayer[] layers = [new Bg4BppRenderLayer(MenuPpuState.Bg1TilemapWord, 0,
            0, 0, GameOverRomData.TilemapWidth, GameOverRomData.TilemapHeight, null), new ObjRenderLayer()];
        return new(PpuMemorySnapshot.Capture(ppu.Vram, ppu.Cgram, oam), layers,
            MenuRenderDefinitions.ObjectSelection, checked((byte)brightness));
    }

    private void PrepareRenderOam()
    {
        oam.BeginFrame();
        var content = mapPresentation ?? throw new InvalidOperationException(
            "Game-over screen requires installed presentation assets.");
        GameOverBabyInstruction instruction = babyInstructionPointer == 0
            ? GameOverBabyAnimationDefinitions.Get(GameOverBabyAnimationDefinitions.FirstPointer)
            : GameOverBabyAnimationDefinitions.Get(babyInstructionPointer);
        content.GameOver.DrawBaby(oam, instruction.Frame);
        content.GameOver.DrawEgg(oam);
        content.GameOver.DrawCursor(oam, missileFrame, SelectedItem != 0);
        oam.FinalizeFrame();
    }

    /// <summary>Ports <c>HandleGameOverBabyMetroid</c>'s six/eight-byte instruction stream.</summary>
    private void StepBabyMetroid()
    {
        StepCompiledBabyMetroid();
    }

    private void StepCompiledBabyMetroid()
    {
        if (babyInstructionTimer == 0)
        {
            babyInstructionPointer = GameOverBabyAnimationDefinitions.FirstPointer;
            babyInstructionTimer =
                GameOverBabyAnimationDefinitions.Get(babyInstructionPointer).Duration;
        }

        babyInstructionTimer = unchecked((ushort)(babyInstructionTimer - 1));
        if (babyInstructionTimer == 0)
        {
            GameOverBabyInstruction completed =
                GameOverBabyAnimationDefinitions.Get(babyInstructionPointer);
            if (completed.SoundAfter != GameOverBabySound.None)
                QueueCompiledBabySound(completed.SoundAfter);
            babyInstructionPointer = completed.NextPointer;
            babyInstructionTimer =
                GameOverBabyAnimationDefinitions.Get(completed.NextPointer).Duration;
        }

        GameOverBabyInstruction current =
            GameOverBabyAnimationDefinitions.Get(babyInstructionPointer);
        babySpritemap = GameOverBabyAnimationDefinitions.NativeSpritemap(current.Frame);
        mapPresentation!.GameOver.ApplyBabyPalette(ppu.Cgram, current.Palette);
    }

    private void QueueCompiledBabySound(GameOverBabySound sound)
    {
        SoundEffectId effect = sound switch
        {
            GameOverBabySound.Cry23 => GameOverRomData.BabyAnimation.Cry23,
            GameOverBabySound.Cry26 => GameOverRomData.BabyAnimation.Cry26,
            GameOverBabySound.Cry27 => GameOverRomData.BabyAnimation.Cry27,
            _ => throw new InvalidDataException($"Unknown compiled game-over Baby sound {sound}."),
        };
        audio.QueueSound(effect, maximumQueued: GameOverRomData.MaximumQueuedSounds);
    }

    private void StepMissileAnimation()
    {
        if (--missileTimer != 0)
            return;
        missileFrame = (missileFrame + 1) % MenuMissileAnimationDefinitions.FrameCount;
        missileTimer = mapPresentation?.GameOver.CursorFrameDuration ??
            MenuMissileAnimationDefinitions.FrameDuration;
    }
}

/// <summary>Native game-over menu indexes; indexes five and six share <see cref="GameOverMenuPhase.FadeOutToContinue"/>, and ordinals are not native menu-index values.</summary>
public enum GameOverMenuPhase
{
    // Ordinals are serialized; ConfigureGraphics is appended rather than placed first.
    /// <summary>Native menu-index-one setup: queues music stop and data load, starts the Baby animation, selects Yes, and begins with zero brightness.</summary>
    Initialize,
    /// <summary>Native menu-index-two music gate: continues Baby animation while waiting for the shared music queue to empty, then queues the game-over track.</summary>
    WaitForInitialMusic,
    /// <summary>Native menu-index-three fade-in: advances the Baby and raises brightness by one per update until full intensity fifteen enables interaction.</summary>
    FadeIn,
    /// <summary>Native menu-index-four interaction: advances the Baby, toggles Yes/No on navigation edges, and accepts the selected answer on an A edge.</summary>
    Main,
    /// <summary>Native menu indexes five/six: fades the accepted Yes answer to black and publishes <see cref="GameOverMenuState.ContinueRequested"/> for the outer map-view transition.</summary>
    FadeOutToContinue,
    /// <summary>Native menu-index-seven path: fades the accepted No answer to black and publishes <see cref="GameOverMenuState.TitleRequested"/> for the outer soft reset.</summary>
    FadeOutToTitle,
    /// <summary>Native menu-index-zero setup: fades out (already black after state $19), cancels library-three sounds, and waits for NMI before configuring the menu graphics.</summary>
    ConfigureGraphics,
}
