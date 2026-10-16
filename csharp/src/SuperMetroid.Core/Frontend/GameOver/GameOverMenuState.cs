using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Retail game-over prompt at <c>$81:90AE-$81:93F7</c>.</summary>
public sealed class GameOverMenuState
{

    /// <summary>Address-space context retained for menu PPU construction and rebinding.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Shared music and sound queue advanced by the menu scene.</summary>
    private readonly CartridgeAudioState audio;
    /// <summary>Owns the scene's VRAM, palette, and installed menu presentation state.</summary>
    private readonly MenuPpuState ppu;
    /// <summary>Accumulates Baby, egg, and cursor sprites for each rendered frame.</summary>
    private readonly OamBuffer oam = new();
    /// <summary>Detects newly pressed answer-navigation and confirmation buttons.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Native master brightness stepped once per fade update.</summary>
    private int brightness;
    /// <summary>Counts updates until the selection missile advances a frame.</summary>
    internal int missileTimer = 1;
    /// <summary>Current cursor animation frame shared by Yes and No positions.</summary>
    private int missileFrame;
    /// <summary>Current compiled Baby animation instruction address.</summary>
    internal ushort babyInstructionPointer;
    /// <summary>Remaining duration of the current Baby pose or accepted-answer hold.</summary>
    internal NativeCountdown babyInstructionTimer;
    /// <summary>Native spritemap identity corresponding to the current Baby frame.</summary>
    internal ushort babySpritemap = GameOverRomData.BabyAnimation.InitialSpritemap;
    /// <summary>Routes Yes directly to the Ceres loader for the saved arrival state.</summary>
    private readonly bool continueLoadsCeresArrival;
    /// <summary>Installed artwork rebound after debugger restoration without changing menu timing.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    /// <summary>The installed map presentation the game-over screen draws from.</summary>
    private AreaMapPresentationCatalog MapPresentation => mapPresentation ??
        throw new InvalidOperationException("No map presentation is installed for the game-over screen.");

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
        this.bus = Ensure.NotNull(bus);
        this.audio = Ensure.NotNull(audio);
        this.mapPresentation = mapPresentation ?? throw new InvalidOperationException(
            "Game-over screen requires installed presentation assets.");
        ppu = new MenuPpuState(mapPresentation.Tiles, mapPresentation.Palettes,
            mapPresentation.WorldArtwork, mapPresentation.Sprites,
            loadInitialBackground: false);
        LoadGameOverTilemap(mapPresentation.GameOver);
        Phase = GameOverMenuPhase.ConfigureGraphics;
    }

    /// <summary>Zero selects Yes; one selects No, matching <c>file_select_map_area_index</c>.</summary>
    public int SelectedItem { get; private set; }

    /// <summary>Current native menu index; these enum ordinals are not native menu-index words.</summary>
    public GameOverMenuPhase Phase { get; internal set; }

    /// <summary>
    /// True when the current menu index stopped at its own NMI wait (<c>$81:8D1D</c> or
    /// <c>$81:9260</c>); the next update finishes that index instead of entering MainGameLoop.
    /// </summary>
    public bool ResumesAfterNmiWait { get; internal set; }

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
        LoadGameOverTilemap(catalog.GameOver);
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
    public void Step(SnesButton controllerInput)
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

        SnesButton pressed = controller.NewlyPressed;
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
                babyInstructionTimer = NativeCountdown.Start(0);
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
                        NativeCountdown.Start(GameOverRomData.BabyAnimation.AcceptedAnswerHoldDuration);
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

    /// <summary>Copies installed BG1 game-over cells into the menu VRAM tilemap.</summary>
    private void LoadGameOverTilemap(GameOverPresentation presentation) =>
        presentation.LoadTilemapTo(new MenuVram(ppu.Vram).Bg1Tilemap);

    /// <summary>Captures BG1/OBJ only; the game-over PPU does not enable BG2.</summary>
    [PpuLayoutBoundary("codec: game-over render packet layers")]
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderOam();
        RenderLayer[] layers = [new Bg4BppRenderLayer(MenuVramLayout.Bg1TilemapWord, 0,
            new Position16<BgPlanePixels>(0, 0), new Extent16<TilemapCells>(GameOverRomData.TilemapWidth, GameOverRomData.TilemapHeight), null), new ObjRenderLayer()];
        return new(PpuMemorySnapshot.Capture(ppu.Vram, ppu.Cgram, oam), layers,
            MenuRenderDefinitions.ObjectSelection, checked((byte)brightness));
    }

    /// <summary>Builds the scene's Baby, egg, and selected-answer cursor OAM from installed art.</summary>
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

    /// <summary>Advances the compiled Baby pose stream and applies its palette and sound effects.</summary>
    private void StepCompiledBabyMetroid()
    {
        if (babyInstructionTimer.IsZero)
        {
            babyInstructionPointer = GameOverBabyAnimationDefinitions.FirstPointer;
            babyInstructionTimer =
                NativeCountdown.FromRaw(GameOverBabyAnimationDefinitions.Get(babyInstructionPointer).Duration);
        }

        babyInstructionTimer = babyInstructionTimer.Decrement().Remaining;
        if (babyInstructionTimer.IsZero)
        {
            GameOverBabyInstruction completed =
                GameOverBabyAnimationDefinitions.Get(babyInstructionPointer);
            if (completed.SoundAfter != GameOverBabySound.None)
                QueueCompiledBabySound(completed.SoundAfter);
            babyInstructionPointer = completed.NextPointer;
            babyInstructionTimer =
                NativeCountdown.FromRaw(GameOverBabyAnimationDefinitions.Get(completed.NextPointer).Duration);
        }

        GameOverBabyInstruction current =
            GameOverBabyAnimationDefinitions.Get(babyInstructionPointer);
        babySpritemap = GameOverBabyAnimationDefinitions.NativeSpritemap(current.Frame);
        MapPresentation.GameOver.ApplyBabyPalette(ppu.Cgram, current.Palette);
    }

    /// <summary>Maps a Baby animation sound marker onto the retail sound queue.</summary>
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

    /// <summary>Cycles the selection cursor when its installed frame duration expires.</summary>
    private void StepMissileAnimation()
    {
        if (--missileTimer != 0)
            return;
        missileFrame = (missileFrame + 1) % MenuMissileAnimationDefinitions.FrameCount;
        missileTimer = mapPresentation?.GameOver.CursorFrameDuration ??
            MenuMissileAnimationDefinitions.FrameDuration;
    }
}
