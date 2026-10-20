using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Complete game-state-$02 options owner, including both secondary pages.</summary>
public sealed class GameOptionsMenuState
{
    /// <summary>Receives cursor and confirmation sounds when available.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>The menu screen the options menu draws: its artwork, tilemaps, palette and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>Holds the heading and animated selection missile for rendering.</summary>
    /// <summary>Derives newly pressed buttons from the raw held controller word.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Provides editable menu art after construction or debugger restoration.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    /// <summary>Mutable primary page whose language palette bits can change in place.</summary>
    private readonly byte[] primaryTilemap;
    /// <summary>English controller page updated with current button labels.</summary>
    private readonly byte[] controllerEnglishTilemap;
    /// <summary>Japanese controller page updated with current button labels.</summary>
    private readonly byte[] controllerJapaneseTilemap;
    /// <summary>English special page updated with toggle palette bits.</summary>
    private readonly byte[] specialEnglishTilemap;
    /// <summary>Japanese special page updated with toggle palette bits.</summary>
    private readonly byte[] specialJapaneseTilemap;
    /// <summary>Page currently loaded into BG1.</summary>
    private byte[] visibleTilemap;
    /// <summary>Page whose input and cursor policy currently apply.</summary>
    private GameOptionsPage page = GameOptionsPage.Primary;
    /// <summary>Page to load when the current page has dissolved to black.</summary>
    private GameOptionsPage dissolveDestination = GameOptionsPage.Primary;
    /// <summary>Current INIDISP level for page fades and final exits.</summary>
    private int brightness;
    /// <summary>BG1 scroll used to expose the lower controller rows.</summary>
    private int bg1VerticalScroll;
    /// <summary>Countdown to the next selection-missile animation frame.</summary>
    private int missileTimer = 1;
    /// <summary>Current selection-missile artwork frame.</summary>
    private int missileFrame;

    /// <summary>Creates the complete primary, controller, and special options-menu state.</summary>
    /// <param name="audio">Optional cartridge audio queues for cursor and confirmation sounds.</param>
    /// <param name="controllerBindings">Initial retail controller-button permutation.</param>
    /// <param name="iconCancelEnabled">Initial state of the automatic item-cancel option.</param>
    /// <param name="moonwalkEnabled">Initial state of the moonwalk option.</param>
    /// <param name="japaneseText">Whether the Japanese-language page variants are initially selected.</param>
    /// <param name="mapPresentation">Required installed menu presentation assets.</param>
    public GameOptionsMenuState(
        CartridgeAudioState? audio = null,
        ControllerBindings? controllerBindings = null,
        bool iconCancelEnabled = false,
        bool moonwalkEnabled = false,
        bool japaneseText = false,
        AreaMapPresentationCatalog? mapPresentation = null)
    {
        this.audio = audio;
        this.mapPresentation = mapPresentation ?? throw new InvalidOperationException(
            "Options menu requires installed presentation assets.");
        screen = new MenuScreen(mapPresentation.Tiles, mapPresentation.Palettes,
            mapPresentation.WorldArtwork, mapPresentation.Sprites);
        mapPresentation.GameOptions.LoadBackground(screen.Tilemaps);

        // Separate installed page copies prevent a language toggle from mutating
        // the other language's source page.
        primaryTilemap = mapPresentation.GameOptions.CreatePage(GameOptionsPresentationDefinitions.PrimaryPage);
        controllerEnglishTilemap = mapPresentation.GameOptions.CreatePage(GameOptionsPresentationDefinitions.ControllerEnglishPage);
        controllerJapaneseTilemap = mapPresentation.GameOptions.CreatePage(GameOptionsPresentationDefinitions.ControllerJapanesePage);
        specialEnglishTilemap = mapPresentation.GameOptions.CreatePage(GameOptionsPresentationDefinitions.SpecialEnglishPage);
        specialJapaneseTilemap = mapPresentation.GameOptions.CreatePage(GameOptionsPresentationDefinitions.SpecialJapanesePage);

        ControllerBindings = (controllerBindings ?? Input.ControllerBindings.Default)
            .RequireRetailPermutation();
        IconCancelEnabled = iconCancelEnabled;
        MoonwalkEnabled = moonwalkEnabled;
        JapaneseText = japaneseText;
        visibleTilemap = primaryTilemap;
        ApplyLanguagePaletteBits();
        LoadVisiblePage();
        Phase = GameOptionsPhase.FinishFadingOut;
    }

    /// <summary>Rebinds host-owned visual assets after a debugger-state restore.</summary>
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        mapPresentation = catalog ?? throw new InvalidOperationException(
            "Options menu requires installed presentation assets.");
        screen.BindWorldArtwork(catalog.WorldArtwork);
        screen.BindMapTiles(catalog.Tiles);
        screen.BindMapSprites(catalog.Sprites);
        screen.BindMapPalettes(catalog.Palettes);
        catalog.GameOptions.LoadBackground(screen.Tilemaps);
        Copy(catalog.GameOptions.CreatePage(GameOptionsPresentationDefinitions.PrimaryPage), primaryTilemap);
        Copy(catalog.GameOptions.CreatePage(GameOptionsPresentationDefinitions.ControllerEnglishPage), controllerEnglishTilemap);
        Copy(catalog.GameOptions.CreatePage(GameOptionsPresentationDefinitions.ControllerJapanesePage), controllerJapaneseTilemap);
        Copy(catalog.GameOptions.CreatePage(GameOptionsPresentationDefinitions.SpecialEnglishPage), specialEnglishTilemap);
        Copy(catalog.GameOptions.CreatePage(GameOptionsPresentationDefinitions.SpecialJapanesePage), specialJapaneseTilemap);
        ApplyLanguagePaletteBits();
        LoadVisiblePage();

        static void Copy(byte[] source, byte[] destination)
        {
            if (source.Length != destination.Length)
                throw new InvalidDataException("Rebound options page size changed.");
            source.CopyTo(destination, 0);
        }
    }

    /// <summary>Current menu row within the active page.</summary>
    public int SelectedItem { get; private set; }

    /// <summary>
    /// The selected save's loading game state (<c>SRAMMirror_LoadingGameState</c>, loaded by file select).
    /// A new game's is zero, the opening-cinematic route.
    /// </summary>
    public SaveLoadingGameState LoadingGameState { get; init; }

    /// <summary>Gets whether the Japanese-language options pages are selected.</summary>
    public bool JapaneseText { get; private set; }

    /// <summary>The seven live controller words committed when the controller page exits.</summary>
    public ControllerBindings ControllerBindings { get; private set; }

    /// <summary>WRAM <c>$09EA</c>, toggled by special-settings row zero.</summary>
    public bool IconCancelEnabled { get; private set; }

    /// <summary>WRAM <c>$09E4</c>, toggled by special-settings row one.</summary>
    public bool MoonwalkEnabled { get; private set; }

    /// <summary>Gets the current options-menu coroutine phase.</summary>
    public GameOptionsPhase Phase { get; internal set; }

    /// <summary>Gets whether the completed start-game fade requested the opening sequence.</summary>
    public bool IntroRequested { get; private set; }

    /// <summary>Gets whether the completed cancel fade requested the file-select menu.</summary>
    public bool FileSelectRequested { get; private set; }

    /// <summary>Advances one options-menu update using the raw held controller word.</summary>
    public void Step(SnesButton controllerInput)
    {
        controller.Latch(controllerInput);
        SnesButton pressed = controller.NewlyPressed;
        // The selection missile is spawned by index one ($82:ECD3); the object handler
        // then runs after every index's work, including that first one.
        if (Phase != GameOptionsPhase.FinishFadingOut)
            StepMissile();

        switch (Phase)
        {
            case GameOptionsPhase.FinishFadingOut:
                // $82:EBDB index zero: the previous menu already reached forced blank, so
                // HandleFadingOut leaves it there and the index advances.
                Phase = GameOptionsPhase.LoadingMenu;
                break;

            case GameOptionsPhase.LoadingMenu:
                // $82:EC11 index one loads the menu under forced blank; fading starts next update.
                Phase = GameOptionsPhase.FadeIn;
                break;

            case GameOptionsPhase.FadeIn:
                brightness = Math.Min(GameOptionsRomData.MaximumBrightness, brightness + 1);
                if (brightness == GameOptionsRomData.MaximumBrightness)
                    Phase = GameOptionsPhase.Main;
                break;

            case GameOptionsPhase.Main:
                StepPrimary(pressed);
                break;

            case GameOptionsPhase.DissolveOut:
                brightness = Math.Max(0, brightness - 1);
                if (brightness == 0)
                {
                    page = dissolveDestination;
                    SelectedItem = 0;
                    bg1VerticalScroll = 0;
                    LoadVisiblePage();
                    Phase = GameOptionsPhase.DissolveIn;
                }
                break;

            case GameOptionsPhase.DissolveIn:
                brightness = Math.Min(GameOptionsRomData.MaximumBrightness, brightness + 1);
                if (brightness == GameOptionsRomData.MaximumBrightness)
                {
                    Phase = page switch
                    {
                        GameOptionsPage.Primary => GameOptionsPhase.Main,
                        GameOptionsPage.Controller => GameOptionsPhase.ControllerSettings,
                        GameOptionsPage.Special => GameOptionsPhase.SpecialSettings,
                        _ => throw new InvalidOperationException($"Unknown options page {page}."),
                    };
                }
                break;

            case GameOptionsPhase.ControllerSettings:
                StepController(pressed);
                break;

            case GameOptionsPhase.SpecialSettings:
                StepSpecial(pressed);
                break;

            case GameOptionsPhase.ScrollControllerDown:
                bg1VerticalScroll += GameOptionsRomData.ControllerScrollPixelsPerFrame;
                if (bg1VerticalScroll == GameOptionsRomData.ControllerScrollLimit)
                    Phase = GameOptionsPhase.ControllerSettings;
                break;

            case GameOptionsPhase.ScrollControllerUp:
                bg1VerticalScroll -= GameOptionsRomData.ControllerScrollPixelsPerFrame;
                if (bg1VerticalScroll == 0)
                    Phase = GameOptionsPhase.ControllerSettings;
                break;

            case GameOptionsPhase.FadeOutToIntro:
                // $82:EE92 index $0C: reaching forced blank selects index four, which
                // dispatches the loading game state on the following update.
                brightness = Math.Max(0, brightness - 1);
                if (brightness == 0)
                    Phase = GameOptionsPhase.StartGame;
                break;

            case GameOptionsPhase.StartGame:
                // $82:EEB4 index four.
                IntroRequested = true;
                break;

            case GameOptionsPhase.FadeOutToFileSelect:
                brightness = Math.Max(0, brightness - 1);
                if (brightness == 0)
                    FileSelectRequested = true;
                break;

            default:
                throw new InvalidOperationException($"Unknown options phase {Phase}.");
        }
    }

    /// <summary>Captures the scrolled options page and selector without composing pixels.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        return MenuRenderSnapshotCapture.Capture(screen,
            unchecked((ushort)bg1VerticalScroll), checked((byte)brightness));
    }

    /// <summary>Draws the current page heading and animated cursor as sprite entries.</summary>
    private void PrepareRenderSprites()
    {
        ScreenSprites sprites = screen.Objects;
        sprites.BeginFrame();
        var content = mapPresentation ?? throw new InvalidOperationException(
            "Options cursor requires installed presentation assets.");
        string pageName = PresentationPageName(page);
        content.GameOptions.DrawHeading(sprites, pageName, bg1VerticalScroll);
        (ushort authoredCursorX, ushort authoredCursorY) = CursorPosition();
        content.GameOptions.DrawCursor(sprites, missileFrame,
            new(authoredCursorX, authoredCursorY));
        sprites.FinishFrame();
    }

    /// <summary>Handles primary rows for language, subpages, start, and cancel.</summary>
    private void StepPrimary(SnesButton pressed)
    {
        if ((pressed & SnesButton.Up) != 0)
        {
            SelectedItem = SelectedItem == 0
                ? GameOptionsRomData.Rows.PrimaryCount - 1
                : SelectedItem - 1;
            QueueMoveSound();
        }
        else if ((pressed & SnesButton.Down) != 0)
        {
            SelectedItem = SelectedItem == GameOptionsRomData.Rows.PrimaryCount - 1
                ? 0
                : SelectedItem + 1;
            QueueMoveSound();
        }

        if ((pressed & SnesButton.B) != 0)
        {
            Phase = GameOptionsPhase.FadeOutToFileSelect;
            return;
        }
        if ((pressed & (SnesButton.Start | SnesButton.A)) == 0)
            return;

        QueueSelectSound();
        var row = (GameOptionsPrimaryRow)SelectedItem;
        if (!Enum.IsDefined(row))
            throw new InvalidOperationException($"Primary options row {SelectedItem} is outside the five-row page.");
        switch (row)
        {
            case GameOptionsPrimaryRow.StartGame:
                // $82:EDBD: a save that resumes ordinary gameplay starts at once (index four);
                // a new game and the cinematic routes fade out first (index $0C).
                Phase = LoadingGameState == SaveLoadingGameState.MainGame
                    ? GameOptionsPhase.StartGame
                    : GameOptionsPhase.FadeOutToIntro;
                break;
            case GameOptionsPrimaryRow.EnglishText:
            case GameOptionsPrimaryRow.JapaneseText:
                JapaneseText = !JapaneseText;
                SelectedItem = 0;
                ApplyLanguagePaletteBits();
                LoadVisiblePage();
                break;
            case GameOptionsPrimaryRow.ControllerSettings:
                BeginDissolveTo(GameOptionsPage.Controller);
                break;
            case GameOptionsPrimaryRow.SpecialSettings:
                BeginDissolveTo(GameOptionsPage.Special);
                break;
            default:
                throw new InvalidOperationException($"Undefined GameOptionsPrimaryRow {row}.");
        }
    }

    /// <summary>Handles button swaps, reset and exit rows, and upper/lower page scrolling.</summary>
    private void StepController(SnesButton pressed)
    {
        if ((pressed & SnesButton.Up) != 0)
        {
            QueueMoveSound();
            SelectedItem--;
            if (SelectedItem < 0)
            {
                SelectedItem = GameOptionsRomData.Rows.ControllerReset;
                Phase = GameOptionsPhase.ScrollControllerDown;
            }
            else if (SelectedItem == GameOptionsRomData.Rows.ControllerActionCount - 1)
            {
                Phase = GameOptionsPhase.ScrollControllerUp;
            }
            return;
        }
        if ((pressed & SnesButton.Down) != 0)
        {
            QueueMoveSound();
            SelectedItem++;
            if (SelectedItem == GameOptionsRomData.Rows.ControllerCount)
            {
                SelectedItem = 0;
                Phase = GameOptionsPhase.ScrollControllerUp;
            }
            else if (GameOptionsRomData.Rows.ControllerRowKind(SelectedItem) == GameOptionsControllerRowKind.Exit)
            {
                Phase = GameOptionsPhase.ScrollControllerDown;
            }
            return;
        }
        if (pressed == SnesButton.None)
            return;

        // Native queues the confirmation sound for any newly pressed word on this page,
        // even when an action row cannot find an assignable button in that word.
        QueueSelectSound();
        GameOptionsControllerRowKind kind = GameOptionsRomData.Rows.ControllerRowKind(SelectedItem);
        if (kind == GameOptionsControllerRowKind.Action)
        {
            for (int button = Input.ControllerBindings.AssignableButtonCount - 1; button >= 0; button--)
            {
                SnesButton physicalButton = Input.ControllerBindings.AssignableButton(button);
                if ((pressed & physicalButton) == SnesButton.None)
                    continue;
                ControllerBindings = ControllerBindings.AssignAndSwap(SelectedItem, physicalButton);
                ApplyControllerLabels();
                LoadVisiblePage();
                break;
            }
            return;
        }

        if ((pressed & (SnesButton.Start | SnesButton.A)) == 0)
            return;
        switch (kind)
        {
            case GameOptionsControllerRowKind.Exit:
                BeginDissolveTo(GameOptionsPage.Primary);
                break;
            case GameOptionsControllerRowKind.Reset:
                ControllerBindings = Input.ControllerBindings.Default;
                ApplyControllerLabels();
                LoadVisiblePage();
                break;
            case GameOptionsControllerRowKind.Action:
                throw new InvalidOperationException("Controller action rows are handled before navigation rows.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(GameOptionsControllerRowKind)} {(int)kind}.");
        }
    }

    /// <summary>Handles icon-cancel and moonwalk toggles or returns to the primary page.</summary>
    private void StepSpecial(SnesButton pressed)
    {
        if ((pressed & SnesButton.Up) != 0)
        {
            SelectedItem = SelectedItem == 0
                ? GameOptionsRomData.Rows.SpecialCount - 1
                : SelectedItem - 1;
            QueueMoveSound();
        }
        else if ((pressed & SnesButton.Down) != 0)
        {
            SelectedItem = SelectedItem == GameOptionsRomData.Rows.SpecialCount - 1
                ? 0
                : SelectedItem + 1;
            QueueMoveSound();
        }

        if ((pressed & SnesButton.B) != 0)
        {
            QueueSelectSound();
            BeginDissolveTo(GameOptionsPage.Primary);
            return;
        }
        if ((pressed & (SnesButton.Start | SnesButton.Left | SnesButton.Right | SnesButton.A)) == 0)
            return;

        QueueSelectSound();
        var row = (GameOptionsSpecialRow)SelectedItem;
        if (!Enum.IsDefined(row))
            throw new InvalidOperationException($"Special options row {SelectedItem} is outside the three-row page.");
        switch (row)
        {
            case GameOptionsSpecialRow.IconCancel:
                IconCancelEnabled = !IconCancelEnabled;
                break;
            case GameOptionsSpecialRow.Moonwalk:
                MoonwalkEnabled = !MoonwalkEnabled;
                break;
            case GameOptionsSpecialRow.Exit:
                BeginDissolveTo(GameOptionsPage.Primary);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(GameOptionsSpecialRow)} {(int)row}.");
        }
        ApplySpecialPaletteBits();
        LoadVisiblePage();
    }

    /// <summary>Records the destination before starting the page fade-out.</summary>
    private void BeginDissolveTo(GameOptionsPage destination)
    {
        dissolveDestination = destination;
        Phase = GameOptionsPhase.DissolveOut;
    }

    /// <summary>Selects the language-specific page, reapplies live labels or toggles, and loads BG1.</summary>
    private void LoadVisiblePage()
    {
        visibleTilemap = page switch
        {
            GameOptionsPage.Primary => primaryTilemap,
            GameOptionsPage.Controller => JapaneseText
                ? controllerJapaneseTilemap
                : controllerEnglishTilemap,
            GameOptionsPage.Special => JapaneseText
                ? specialJapaneseTilemap
                : specialEnglishTilemap,
            _ => throw new InvalidOperationException($"Unknown options page {page}."),
        };

        if (page == GameOptionsPage.Controller)
            ApplyControllerLabels();
        else if (page == GameOptionsPage.Special)
            ApplySpecialPaletteBits();
        MenuTilemaps.Load(screen.Tilemaps.Bg1, visibleTilemap);
    }

    /// <summary>Updates primary-page palette bits for the selected text language.</summary>
    private void ApplyLanguagePaletteBits()
    {
        (mapPresentation ?? throw new InvalidOperationException(
            "Options language requires installed presentation assets."))
            .GameOptions.ApplyLanguage(primaryTilemap, JapaneseText);
    }

    /// <summary>Marks the two special-setting toggle rows using installed presentation art.</summary>
    private void ApplySpecialPaletteBits()
    {
        var content = mapPresentation ?? throw new InvalidOperationException(
            "Options toggles require installed presentation assets.");
        content.GameOptions.ApplySpecialToggle(visibleTilemap,
            GameOptionsPresentationDefinitions.IconCancelToggle, IconCancelEnabled);
        content.GameOptions.ApplySpecialToggle(visibleTilemap,
            GameOptionsPresentationDefinitions.MoonwalkToggle, MoonwalkEnabled);
    }

    /// <summary>Writes current physical-button labels to every controller action row.</summary>
    private void ApplyControllerLabels()
    {
        var content = mapPresentation ?? throw new InvalidOperationException(
            "Controller labels require installed presentation assets.");
        for (int action = 0; action < GameOptionsRomData.Rows.ControllerActionCount; action++)
        {
            content.GameOptions.ApplyControllerLabel(visibleTilemap, action,
                Input.ControllerBindings.AssignableButtonIndex(ControllerBindings[action]) ?? 0);
        }
    }

    /// <summary>Uses the page cursor layout or the hidden anchor during transitions.</summary>
    private (ushort X, ushort Y) CursorPosition()
    {
        GameOptionsPage? cursorPage = GameOptionsCursorPolicy.Select(Phase);
        // Keep the cursor actor in OAM at the installed hidden anchor during transitions.
        if (cursorPage is null)
        {
            Position16<OamPixels> hidden = (mapPresentation ?? throw new InvalidOperationException(
                "Options cursor requires installed presentation assets."))
                .GameOptions.HiddenCursor;
            return (checked(hidden.X), checked(hidden.Y));
        }
        Position16<OamPixels> point = (mapPresentation ?? throw new InvalidOperationException(
            "Options cursor requires installed presentation assets."))
            .GameOptions.CursorPosition(PresentationPageName(cursorPage.Value), SelectedItem);
        return (checked(point.X), checked(point.Y));
    }

    /// <summary>Advances the selection missile after its installed frame duration.</summary>
    private void StepMissile()
    {
        if (--missileTimer != 0)
            return;
        missileFrame = (missileFrame + 1) % MenuMissileAnimationDefinitions.FrameCount;
        missileTimer = mapPresentation?.GameOptions.CursorFrameDuration ??
            MenuMissileAnimationDefinitions.FrameDuration;
    }

    /// <summary>Maps the active menu page to its installed presentation key.</summary>
    private static string PresentationPageName(GameOptionsPage value) => value switch
    {
        GameOptionsPage.Primary => GameOptionsPresentationDefinitions.PrimaryMenu,
        GameOptionsPage.Controller => GameOptionsPresentationDefinitions.ControllerMenu,
        GameOptionsPage.Special => GameOptionsPresentationDefinitions.SpecialMenu,
        _ => throw new InvalidOperationException($"Unknown options page {value}."),
    };

    /// <summary>Queues the native cursor sound within the menu sound limit.</summary>
    private void QueueMoveSound() =>
        audio?.QueueSound(
            SoundEffectLibrary1Sounds.MenuCursor,
            maximumQueued: GameOptionsRomData.MaximumQueuedMenuSounds);

    /// <summary>Queues the native confirmation sound within the menu sound limit.</summary>
    private void QueueSelectSound() =>
        audio?.QueueSound(
            SoundEffectLibrary1Sounds.MenuConfirm,
            maximumQueued: GameOptionsRomData.MaximumQueuedMenuSounds);
}
