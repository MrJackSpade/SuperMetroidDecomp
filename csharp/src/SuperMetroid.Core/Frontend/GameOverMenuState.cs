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
    /// <summary>Scratch pixels for OBJ compositing, reused across renders and cleared before the scene's objects are drawn.</summary>
    [NonSerialized] private Rgba32[]? objectLayerScratch;
    // Final frame, reused by every render: a returned frame is valid until this scene renders again.
    /// <summary>Scene-owned row-major RGBA output storage returned by <see cref="Render"/> and overwritten by the next render.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;

    /// <summary>Address-space context used to construct and rebind the menu's PPU presentation.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Shared cartridge-style music and sound queue advanced by the game-over menu.</summary>
    private readonly CartridgeAudioState audio;
    /// <summary>PPU memory and register state used to render the menu's BG1 and OBJ layers.</summary>
    private readonly MenuPpuState ppu;
    /// <summary>Per-frame object attribute entries for the Baby, egg, and answer cursor.</summary>
    private readonly OamBuffer oam = new();
    /// <summary>Input edge history used to distinguish newly pressed menu buttons from held buttons.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Master PPU brightness applied to rendered pixels; menu fades change it by one level per update.</summary>
    private int brightness;
    /// <summary>Remaining updates before the animated cursor advances to its next missile frame.</summary>
    private int missileTimer = 1;
    /// <summary>Current cursor missile frame, cycled through the installed animation sequence.</summary>
    private int missileFrame;
    /// <summary>Pointer into the compiled Baby instruction sequence, with zero denoting an uninitialized sequence.</summary>
    private ushort babyInstructionPointer;
    /// <summary>Updates remaining in the current compiled Baby instruction.</summary>
    private ushort babyInstructionTimer;
    /// <summary>Native spritemap address corresponding to the currently selected Baby animation frame.</summary>
    private ushort babySpritemap = GameOverRomData.BabyAnimation.InitialSpritemap;
    /// <summary>Installed artwork and timing data used to draw this scene; rebound after host presentation restoration.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;

    /// <summary>Creates the game-over scene with installed artwork and its BG1 tilemap, leaving music, animation, and fade initialization for the first update.</summary>
    /// <param name="bus">Address-space context passed to the menu PPU and later presentation rebinding.</param>
    /// <param name="audio">Shared cartridge-style music and sound queue used by menu updates.</param>
    /// <param name="mapPresentation">Required installed presentation bundle, despite the compatibility default of null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> or <paramref name="audio"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No installed presentation bundle is supplied.</exception>
    public GameOverMenuState(
        ISnesAddressSpace bus,
        CartridgeAudioState audio,
        AreaMapPresentationCatalog? mapPresentation = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
        this.mapPresentation = mapPresentation ?? throw new InvalidOperationException(
            "Game-over screen requires installed presentation assets.");
        ppu = new MenuPpuState(bus, mapPresentation.Tiles, mapPresentation.Palettes,
            mapPresentation.WorldArtwork, mapPresentation.Sprites,
            loadInitialBackground: false);
        mapPresentation.GameOver.LoadTilemapTo(ppu.Vram, MenuPpuState.Bg1TilemapWord);
        Phase = GameOverMenuPhase.Initialize;
    }

    /// <summary>Zero selects Yes; one selects No, matching <c>file_select_map_area_index</c>.</summary>
    public int SelectedItem { get; private set; }

    /// <summary>Current host stage of initialization, music waiting, interaction, or fading; these enum ordinals are not native menu-index words.</summary>
    public GameOverMenuPhase Phase { get; private set; }

    /// <summary>Sticky request raised when the accepted Yes answer has faded fully to black; the outer dispatcher opens file-select map view rather than reloading the save inside this scene.</summary>
    public bool ContinueRequested { get; private set; }

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

        ppu.BindMapTiles(bus, catalog.Tiles);
        ppu.BindMapSprites(bus, catalog.Sprites);
        ppu.BindMapPalettes(bus, catalog.Palettes);
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
        SnesButton pressed = controller.NewlyPressedButtons;
        StepMissileAnimation();

        switch (Phase)
        {
            case GameOverMenuPhase.Initialize:
                audio.QueueMusicDelayed8(MusicCommand.Stop);
                audio.QueueMusicDelayed8(MusicCommand.LoadData(GameOverRomData.Music.DataIndex));
                babyInstructionPointer = 0;
                babyInstructionTimer = 0;
                StepBabyMetroid();
                SelectedItem = 0;
                brightness = 0;
                Phase = GameOverMenuPhase.WaitForInitialMusic;
                break;

            case GameOverMenuPhase.WaitForInitialMusic:
                StepBabyMetroid();
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

    /// <summary>Builds the current frame's object list from the Baby animation, egg, and selected-answer cursor.</summary>
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

    /// <summary>Advances the compiled Baby instruction timer, queues completed-instruction sounds, and applies its frame and palette.</summary>
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

    /// <summary>Translates a compiled Baby sound marker to its cartridge effect and queues it on the shared audio state.</summary>
    /// <param name="sound">Sound marker emitted when a Baby animation instruction completes.</param>
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

    /// <summary>Counts down the cursor animation interval and advances its frame when the interval expires.</summary>
    private void StepMissileAnimation()
    {
        if (--missileTimer != 0)
            return;
        missileFrame = (missileFrame + 1) % MenuMissileAnimationDefinitions.FrameCount;
        missileTimer = mapPresentation?.GameOver.CursorFrameDuration ??
            MenuMissileAnimationDefinitions.FrameDuration;
    }
}

/// <summary>Host stages of the retail game-over prompt; they combine native dispatcher boundaries and do not reproduce native menu-index numeric values.</summary>
public enum GameOverMenuPhase
{
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
}
