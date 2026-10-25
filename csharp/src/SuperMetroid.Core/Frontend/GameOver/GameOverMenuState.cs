using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Retail game-over prompt at <c>$81:90AE-$81:93F7</c>.</summary>
public sealed class GameOverMenuState : IMenuFrontendScreen
{
    /// <inheritdoc/>
    [MemberNotNull(nameof(spriteSheets))]
    public void BindSpriteSheets(InstalledSpriteSheets? sheets)
    {
        spriteSheets = sheets ?? throw new InvalidOperationException("Game-over screen requires the installed sprite sheets.");
        screen.BindScreenTiles(sheets);
    }

    /// <summary>The installed sprite sheets the Baby, egg and cursor draw from; artwork, never saved.</summary>
    [NonSerialized] private InstalledSpriteSheets spriteSheets;

    /// <summary>Shared music and sound queue advanced by the menu scene.</summary>
    private readonly AudioQueues audio;
    /// <summary>The menu screen game over draws: its artwork, tilemaps, palette and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>Accumulates Baby, egg, and cursor sprites for each rendered frame.</summary>
    /// <summary>Detects newly pressed answer-navigation and confirmation buttons.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Native master brightness stepped once per fade update.</summary>
    private int brightness;
    /// <summary>The selection missile's frame cycle, shared by the Yes and No positions.</summary>
    internal readonly MenuMissileAnimation missile = new();
    /// <summary>Current step of the Baby pose program, or null before the first animation update of this visit.</summary>
    internal int? BabyStep { get; private set; }
    /// <summary>Remaining duration of the current Baby pose or accepted-answer hold.</summary>
    internal WrappingCountdown BabyInstructionTimer { get; private set; }
    /// <summary>Routes Yes directly to the Ceres loader for the saved arrival state.</summary>
    private readonly bool continueLoadsCeresArrival;
    /// <summary>Installed artwork rebound after debugger restoration without changing menu timing.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    /// <summary>The installed map presentation the game-over screen draws from.</summary>
    private AreaMapPresentationCatalog MapPresentation => mapPresentation ??
        throw new InvalidOperationException("No map presentation is installed for the game-over screen.");

    /// <summary>Creates the game-over scene with installed artwork and its BG1 tilemap, leaving music, animation, and fade initialization for the first update.</summary>
    /// <param name="audio">Shared cartridge-style music and sound queue used by menu updates.</param>
    /// <param name="mapPresentation">Required installed presentation bundle.</param>
    /// <param name="spriteSheets">Required installed sprite sheets, which the Baby, egg and cursor draw from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="audio"/> is null.</exception>
    /// <exception cref="InvalidOperationException">No installed presentation bundle or sprite sheets are supplied.</exception>
    /// <param name="continueLoadsCeresArrival">
    /// True when <c>SRAMMirror_LoadingGameState</c> is <c>$1F</c>: <c>$81:915D</c> then answers Yes by
    /// publishing the Ceres loader immediately instead of fading into the area map.
    /// </param>
    public GameOverMenuState(
        AudioQueues audio,
        AreaMapPresentationCatalog? mapPresentation,
        InstalledSpriteSheets? spriteSheets,
        bool continueLoadsCeresArrival = false)
    {
        this.continueLoadsCeresArrival = continueLoadsCeresArrival;
        this.audio = Ensure.NotNull(audio);
        this.mapPresentation = mapPresentation ?? throw new InvalidOperationException(
            "Game-over screen requires installed presentation assets.");
        screen = new MenuScreen(mapPresentation.Tiles, mapPresentation.Palettes,
            mapPresentation.WorldArtwork, mapPresentation.Sprites, spriteSheets);
        BindSpriteSheets(spriteSheets);
        LoadGameOverTilemap(mapPresentation.GameOver);
        Phase = GameOverMenuPhase.ConfigureGraphics;
    }

    /// <summary>Zero selects Yes; one selects No, matching <c>file_select_map_area_index</c>.</summary>
    public YesNoChoice SelectedItem { get; private set; }

    /// <summary>Current native menu index; these enum ordinals are not native menu-index words.</summary>
    public GameOverMenuPhase Phase { get; internal set; }

    /// <summary>
    /// True when the current menu index stopped at its own NMI wait (<c>$81:8D1D</c> or
    /// <c>$81:9260</c>); the next update finishes that index instead of entering MainGameLoop.
    /// </summary>
    public bool ResumesAfterFrameWait { get; internal set; }

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

        screen.BindMapTiles(catalog.Tiles);
        screen.BindMapSprites(catalog.Sprites);
        screen.BindMapPalettes(catalog.Palettes);
        LoadGameOverTilemap(catalog.GameOver);
        if (BabyStep is int step)
        {
            GameOverBabyInstruction instruction = GameOverBabyAnimation.Get(step);
            catalog.GameOver.ApplyBabyPalette(screen.Palette, instruction.Palette);
        }
    }

    /// <summary>Advances one menu update, latching input edges, animating the cursor and Baby, and progressing the music wait and one-level-per-update brightness fades.</summary>
    /// <param name="controllerInput">Current held-button word in native <see cref="SnesButton"/> bit layout; only newly pressed buttons affect the interactive menu.</param>
    /// <remarks>Select, Up, or Down toggles the answer and takes priority over A confirmation in the same update. Accepting either answer holds the current Baby frame for 180 updates while fading out.</remarks>
    public void Step(SnesButton controllerInput)
    {
        controller.Latch(controllerInput);
        if (ResumesAfterFrameWait)
        {
            // Both waits are the last call of their index; the remainder only configures
            // presentation and advances the index.
            ResumesAfterFrameWait = false;
            Phase = Phase switch
            {
                GameOverMenuPhase.ConfigureGraphics => GameOverMenuPhase.Initialize,
                GameOverMenuPhase.Initialize => GameOverMenuPhase.WaitForInitialMusic,
                GameOverMenuPhase.WaitForInitialMusic or
                    GameOverMenuPhase.FadeIn or
                    GameOverMenuPhase.Main or
                    GameOverMenuPhase.FadeOutToContinue or
                    GameOverMenuPhase.FadeOutToTitle => throw new InvalidOperationException($"Game-over phase {Phase} has no NMI wait."),
                _ => throw new InvalidOperationException($"Game-over phase {Phase} has no NMI wait."),
            };
            return;
        }

        SnesButton pressed = controller.NewlyPressed;
        // Only indexes three, four, five and seven draw the selection missile.
        if (Phase is GameOverMenuPhase.FadeIn or GameOverMenuPhase.Main or
            GameOverMenuPhase.FadeOutToContinue or GameOverMenuPhase.FadeOutToTitle)
            missile.Step(mapPresentation?.GameOver.CursorFrameDuration);

        switch (Phase)
        {
            case GameOverMenuPhase.ConfigureGraphics:
                // State $19 hands over at brightness zero, so HandleFadingOut leaves the screen
                // black and $81:8D1D blanks it and waits for NMI before configuring the menu.
                if (brightness != 0)
                    throw new InvalidOperationException("Game-over menu must begin from a black screen.");
                audio.QueueSound(Library3Sound.Silence, maximumQueued: GameOverMenuDefinitions.MaximumQueuedSounds);
                ResumesAfterFrameWait = true;
                break;

            case GameOverMenuPhase.Initialize:
                audio.QueueMusicDelayed8(MusicRequest.Stop);
                audio.QueueMusicDelayed8(MusicRequest.LoadDataSet(MusicDataSet.TitleSequence));
                BabyStep = null;
                BabyInstructionTimer = WrappingCountdown.Start(0);
                StepBabyMetroid();
                SelectedItem = YesNoChoice.Yes;
                brightness = 0;
                // $81:9260 unblanks and waits for NMI before advancing the index.
                ResumesAfterFrameWait = true;
                break;

            case GameOverMenuPhase.WaitForInitialMusic:
                // $81:93E8 only polls the music queue; it neither animates the Baby nor draws the missile.
                if (!audio.HasQueuedMusic)
                {
                    audio.QueueMusicDelayed8(
                        MusicRequest.SelectTrack(GameOverMenuDefinitions.MusicTrack));
                    Phase = GameOverMenuPhase.FadeIn;
                }
                break;

            case GameOverMenuPhase.FadeIn:
                StepBabyMetroid();
                brightness = Math.Min(GameOverMenuDefinitions.MaximumBrightness, brightness + 1);
                if (brightness == GameOverMenuDefinitions.MaximumBrightness)
                    Phase = GameOverMenuPhase.Main;
                break;

            case GameOverMenuPhase.Main:
                StepBabyMetroid();
                if ((pressed & (SnesButton.Select | SnesButton.Up | SnesButton.Down)) != 0)
                {
                    audio.QueueSound(
                        Library1Sound.MenuCursor,
                        maximumQueued: GameOverMenuDefinitions.MaximumQueuedSounds);
                    SelectedItem = SelectedItem == YesNoChoice.Yes ? YesNoChoice.No : YesNoChoice.Yes;
                }
                else if ((pressed & SnesButton.A) != 0)
                {
                    // `$81:914B` holds the current Baby frame for 180 ticks once either
                    // answer is accepted, while the selected fade owner continues drawing.
                    BabyInstructionTimer =
                        WrappingCountdown.Start(GameOverMenuDefinitions.AcceptedAnswerHoldDuration);
                    if (SelectedItem == YesNoChoice.Yes && continueLoadsCeresArrival)
                        CeresArrivalRequested = true;
                    else
                        Phase = SelectedItem == YesNoChoice.Yes
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

    /// <summary>Copies installed BG1 game-over cells into the menu's BG1 tilemap.</summary>
    private void LoadGameOverTilemap(GameOverPresentation presentation) =>
        presentation.LoadTilemapTo(screen.Tilemaps.Bg1);

    /// <summary>Captures BG1/OBJ only; the game-over PPU does not enable BG2.</summary>
    public LayeredScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        RenderLayer[] layers = [new TileRenderLayer(ScreenTileMap.Of(screen.Tilemaps.Bg1,
            new Extent16<TilemapCells>(GameOverPresentationDefinitions.TilemapWidth, GameOverPresentationDefinitions.TilemapHeight), screen.MenuTiles),
            new Position16<BgPlanePixels>(0, 0), null), new ObjRenderLayer()];
        return new(screen.Paint(), layers, checked((byte)brightness));
    }

    /// <summary>Builds the scene's Baby, egg, and selected-answer cursor sprite entries from installed art.</summary>
    private void PrepareRenderSprites()
    {
        ScreenSprites sprites = screen.Objects;
        sprites.BeginFrame();
        var content = mapPresentation ?? throw new InvalidOperationException(
            "Game-over screen requires installed presentation assets.");
        SpriteSheet sheet = spriteSheets.Get(SpriteSheetId.GameOver);
        GameOverBabyInstruction instruction = GameOverBabyAnimation.Get(BabyStep ?? 0);
        content.GameOver.DrawBaby(sprites, sheet, instruction.Frame);
        content.GameOver.DrawEgg(sprites, sheet);
        content.GameOver.DrawCursor(sprites, sheet, missile.Frame, SelectedItem != YesNoChoice.Yes);
        sprites.FinishFrame();
    }

    /// <summary>Ports <c>HandleGameOverBabyMetroid</c>'s six/eight-byte instruction stream.</summary>
    private void StepBabyMetroid()
    {
        StepCompiledBabyMetroid();
    }

    /// <summary>Advances the compiled Baby pose stream and applies its palette and sound effects.</summary>
    private void StepCompiledBabyMetroid()
    {
        if (BabyInstructionTimer.IsZero)
        {
            BabyStep = 0;
            BabyInstructionTimer = WrappingCountdown.Start(GameOverBabyAnimation.Get(0).Duration);
        }

        BabyInstructionTimer = BabyInstructionTimer.Decrement().Remaining;
        GameOverBabyInstruction current = GameOverBabyAnimation.Get(BabyStep ??
            throw new InvalidOperationException("The game-over Baby program is held before its first step."));
        if (BabyInstructionTimer.IsZero)
        {
            if (current.SoundAfter != GameOverBabySound.None)
                QueueCompiledBabySound(current.SoundAfter);
            current = GameOverBabyAnimation.Get(current.NextStep);
            BabyStep = current.Step;
            BabyInstructionTimer = WrappingCountdown.Start(current.Duration);
        }

        MapPresentation.GameOver.ApplyBabyPalette(screen.Palette, current.Palette);
    }

    /// <summary>Queues a Baby animation sound marker's cry on the retail sound queue.</summary>
    private void QueueCompiledBabySound(GameOverBabySound sound) =>
        audio.QueueSound(CrySound(sound), maximumQueued: GameOverMenuDefinitions.MaximumQueuedSounds);

    /// <summary>The cry a Baby animation sound marker queues.</summary>
    /// <exception cref="InvalidDataException">The marker queues no cry.</exception>
    internal static SoundEffectId CrySound(GameOverBabySound sound) => sound switch
    {
        GameOverBabySound.FirstCry => GameOverMenuDefinitions.FirstCry,
        GameOverBabySound.SecondCry => GameOverMenuDefinitions.SecondCry,
        GameOverBabySound.ThirdCry => GameOverMenuDefinitions.ThirdCry,
        GameOverBabySound.None => throw new InvalidDataException($"Unknown compiled game-over Baby sound {sound}."),
        _ => throw new InvalidDataException($"Unknown compiled game-over Baby sound {sound}."),
    };

}
