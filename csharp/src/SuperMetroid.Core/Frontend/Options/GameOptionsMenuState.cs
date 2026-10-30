using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Complete game-state-$02 options owner, including both secondary pages.</summary>
public sealed class GameOptionsMenuState : IMenuFrontendScreen
{
    /// <inheritdoc/>
    [MemberNotNull(nameof(spriteSheets))]
    public void BindSpriteSheets(InstalledSpriteSheets? sheets)
    {
        spriteSheets = sheets ?? throw new InvalidOperationException("Options menu requires the installed sprite sheets.");
        screen.BindScreenTiles(sheets);
    }

    /// <summary>The installed sprite sheets the headings and cursor draw from; artwork, never saved.</summary>
    [NonSerialized] private InstalledSpriteSheets spriteSheets;

    /// <summary>Receives cursor and confirmation sounds when available.</summary>
    private readonly AudioQueues? audio;
    /// <summary>The menu screen the options menu draws: its artwork, tilemaps, palette and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>Holds the heading and animated selection missile for rendering.</summary>
    /// <summary>Derives newly pressed buttons from the raw held controller word.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Provides editable menu art after construction or debugger restoration.</summary>
    [NonSerialized] internal AreaMapPresentation? mapPresentation;
    /// <summary>The page last loaded and the state its labels, language rows and toggle choices were drawn from (#1467).</summary>
    internal ShownOptionsPage shownPage;
    /// <summary>Page whose input and cursor policy currently apply.</summary>
    private GameOptionsPage page = GameOptionsPage.Primary;
    /// <summary>Page to load when the current page has dissolved to black.</summary>
    private GameOptionsPage dissolveDestination = GameOptionsPage.Primary;
    /// <summary>Screen brightness level, 0 (black) through 15, that the page fades and final exits step.</summary>
    private int brightness;
    /// <summary>BG1 scroll used to expose the lower controller rows.</summary>
    private int bg1VerticalScroll;
    /// <summary>The selection missile's frame cycle.</summary>
    private readonly MenuMissileAnimation missile = new();

    /// <summary>Creates the complete primary, controller, and special options-menu state.</summary>
    /// <param name="audio">Optional cartridge audio queues for cursor and confirmation sounds.</param>
    /// <param name="controllerBindings">Initial retail controller-button permutation.</param>
    /// <param name="iconCancelEnabled">Initial state of the automatic item-cancel option.</param>
    /// <param name="moonwalkEnabled">Initial state of the moonwalk option.</param>
    /// <param name="japaneseText">Whether the Japanese-language page variants are initially selected.</param>
    /// <param name="mapPresentation">Required installed menu presentation assets.</param>
    /// <param name="spriteSheets">Required installed sprite sheets, which the headings and cursor draw from.</param>
    [IndependentFlag("Icon cancel, moonwalk and Japanese text are three independent saved options.")]
    public GameOptionsMenuState(
        AudioQueues? audio = null,
        ControllerBindings? controllerBindings = null,
        bool iconCancelEnabled = false,
        bool moonwalkEnabled = false,
        bool japaneseText = false,
        AreaMapPresentation? mapPresentation = null,
        InstalledSpriteSheets? spriteSheets = null)
    {
        this.audio = audio;
        this.mapPresentation = mapPresentation ?? throw new InvalidOperationException(
            "Options menu requires installed presentation assets.");
        screen = new MenuScreen(mapPresentation.Palettes,
            mapPresentation.WorldArtwork, spriteSheets);
        BindSpriteSheets(spriteSheets);

        ControllerBindings = (controllerBindings ?? Input.ControllerBindings.Default)
            .RequireRetailPermutation();
        IconCancelEnabled = iconCancelEnabled;
        MoonwalkEnabled = moonwalkEnabled;
        JapaneseText = japaneseText;
        LoadVisiblePage();
        Phase = GameOptionsPhase.FinishFadingOut;
    }

    /// <summary>Rebinds host-owned visual assets after a debugger-state restore.</summary>
    public void BindMapPresentation(AreaMapPresentation? catalog)
    {
        mapPresentation = catalog ?? throw new InvalidOperationException(
            "Options menu requires installed presentation assets.");
        screen.BindWorldArtwork(catalog.WorldArtwork);

        screen.BindMapPalettes(catalog.Palettes);
        LoadVisiblePage();
    }

    /// <summary>Current menu row within the active page.</summary>
    [IntegerQuantity("cursor row position on the current page; each page decodes it into its own row type")]
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
    public void Step(ControllerButton controllerInput)
    {
        controller.Latch(controllerInput);
        ControllerButton pressed = controller.NewlyPressed;
        // The selection missile is spawned by index one ($82:ECD3); the object handler
        // then runs after every index's work, including that first one.
        if (Phase != GameOptionsPhase.FinishFadingOut)
            missile.Step(mapPresentation?.GameOptions.CursorFrameDuration);

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
                brightness = Math.Min(GameOptionsDefinitions.MaximumBrightness, brightness + 1);
                if (brightness == GameOptionsDefinitions.MaximumBrightness)
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
                brightness = Math.Min(GameOptionsDefinitions.MaximumBrightness, brightness + 1);
                if (brightness == GameOptionsDefinitions.MaximumBrightness)
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
                bg1VerticalScroll += GameOptionsDefinitions.ControllerScrollPixelsPerFrame;
                if (bg1VerticalScroll == GameOptionsDefinitions.ControllerScrollLimit)
                    Phase = GameOptionsPhase.ControllerSettings;
                break;

            case GameOptionsPhase.ScrollControllerUp:
                bg1VerticalScroll -= GameOptionsDefinitions.ControllerScrollPixelsPerFrame;
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

    /// <summary>Captures the backdrop, the scrolled page with what is drawn over it, and the selector without composing pixels.</summary>
    public LayeredScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        // The backdrop, the page and its labels, language rows and toggle choices are the menus' pictures (#1467).
        return MenuRenderSnapshotCapture.Capture(screen, screen.PagePictures(GameOptionsNames.BackgroundFrame),
            screen.PagePictures(shownPage.Frames()), unchecked((ushort)bg1VerticalScroll), checked((byte)brightness));
    }

    /// <summary>Draws the current page heading and animated cursor as sprite entries.</summary>
    private void PrepareRenderSprites()
    {
        ScreenSprites sprites = screen.Objects;
        sprites.BeginFrame();
        var content = mapPresentation ?? throw new InvalidOperationException(
            "Options cursor requires installed presentation assets.");
        SpriteSheet sheet = spriteSheets.Get(SpriteSheetId.GameOptions);
        content.GameOptions.DrawHeading(sprites, sheet, page, bg1VerticalScroll);
        GameOptionsPresentation.DrawCursor(sprites, sheet, missile.Frame, CursorPosition());
        sprites.FinishFrame();
    }

    /// <summary>Handles primary rows for language, subpages, start, and cancel.</summary>
    private void StepPrimary(ControllerButton pressed)
    {
        if ((pressed & ControllerButton.Up) != ControllerButton.None)
        {
            SelectedItem = SelectedItem == 0
                ? GameOptionsDefinitions.Rows.PrimaryCount - 1
                : SelectedItem - 1;
            QueueMoveSound();
        }
        else if ((pressed & ControllerButton.Down) != ControllerButton.None)
        {
            SelectedItem = SelectedItem == GameOptionsDefinitions.Rows.PrimaryCount - 1
                ? 0
                : SelectedItem + 1;
            QueueMoveSound();
        }

        if ((pressed & ControllerButton.B) != ControllerButton.None)
        {
            Phase = GameOptionsPhase.FadeOutToFileSelect;
            return;
        }
        if ((pressed & (ControllerButton.Start | ControllerButton.A)) == ControllerButton.None)
            return;

        QueueSelectSound();
        GameOptionsPrimaryRow row = GameOptionsPositions.PrimaryRowAt(SelectedItem)
            ?? throw new InvalidOperationException($"Primary options row {SelectedItem} is outside the five-row page.");
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
    private void StepController(ControllerButton pressed)
    {
        if ((pressed & ControllerButton.Up) != ControllerButton.None)
        {
            QueueMoveSound();
            SelectedItem--;
            if (SelectedItem < 0)
            {
                SelectedItem = GameOptionsDefinitions.Rows.ControllerReset;
                Phase = GameOptionsPhase.ScrollControllerDown;
            }
            else if (SelectedItem == GameOptionsDefinitions.Rows.ControllerActionCount - 1)
            {
                Phase = GameOptionsPhase.ScrollControllerUp;
            }
            return;
        }
        if ((pressed & ControllerButton.Down) != ControllerButton.None)
        {
            QueueMoveSound();
            SelectedItem++;
            if (SelectedItem == GameOptionsDefinitions.Rows.ControllerCount)
            {
                SelectedItem = 0;
                Phase = GameOptionsPhase.ScrollControllerUp;
            }
            else if (GameOptionsPositions.ControllerRowKind(SelectedItem) == GameOptionsControllerRowKind.Exit)
            {
                Phase = GameOptionsPhase.ScrollControllerDown;
            }
            return;
        }
        if (pressed == ControllerButton.None)
            return;

        // Native queues the confirmation sound for any newly pressed word on this page,
        // even when an action row cannot find an assignable button in that word.
        QueueSelectSound();
        GameOptionsControllerRowKind kind = GameOptionsPositions.ControllerRowKind(SelectedItem);
        if (kind == GameOptionsControllerRowKind.Action)
        {
            for (int button = Input.ControllerBindings.AssignableButtonCount - 1; button >= 0; button--)
            {
                ControllerButton physicalButton = Input.ControllerBindings.AssignableButton(button);
                if ((pressed & physicalButton) == ControllerButton.None)
                    continue;
                ControllerBindings = ControllerBindings.AssignAndSwap(SelectedItem, physicalButton);
                LoadVisiblePage();
                break;
            }
            return;
        }

        if ((pressed & (ControllerButton.Start | ControllerButton.A)) == ControllerButton.None)
            return;
        switch (kind)
        {
            case GameOptionsControllerRowKind.Exit:
                BeginDissolveTo(GameOptionsPage.Primary);
                break;
            case GameOptionsControllerRowKind.Reset:
                ControllerBindings = Input.ControllerBindings.Default;
                LoadVisiblePage();
                break;
            case GameOptionsControllerRowKind.Action:
                throw new InvalidOperationException("Controller action rows are handled before navigation rows.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(GameOptionsControllerRowKind)} {kind}.");
        }
    }

    /// <summary>Handles icon-cancel and moonwalk toggles or returns to the primary page.</summary>
    private void StepSpecial(ControllerButton pressed)
    {
        if ((pressed & ControllerButton.Up) != ControllerButton.None)
        {
            SelectedItem = SelectedItem == 0
                ? GameOptionsDefinitions.Rows.SpecialCount - 1
                : SelectedItem - 1;
            QueueMoveSound();
        }
        else if ((pressed & ControllerButton.Down) != ControllerButton.None)
        {
            SelectedItem = SelectedItem == GameOptionsDefinitions.Rows.SpecialCount - 1
                ? 0
                : SelectedItem + 1;
            QueueMoveSound();
        }

        if ((pressed & ControllerButton.B) != ControllerButton.None)
        {
            QueueSelectSound();
            BeginDissolveTo(GameOptionsPage.Primary);
            return;
        }
        if ((pressed & (ControllerButton.Start | ControllerButton.Left | ControllerButton.Right | ControllerButton.A)) == ControllerButton.None)
            return;

        QueueSelectSound();
        GameOptionsSpecialRow row = GameOptionsPositions.SpecialRowAt(SelectedItem)
            ?? throw new InvalidOperationException($"Special options row {SelectedItem} is outside the three-row page.");
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
                throw new InvalidOperationException($"Undefined {nameof(GameOptionsSpecialRow)} {row}.");
        }
        LoadVisiblePage();
    }

    /// <summary>Records the destination before starting the page fade-out.</summary>
    private void BeginDissolveTo(GameOptionsPage destination)
    {
        dissolveDestination = destination;
        Phase = GameOptionsPhase.DissolveOut;
    }

    /// <summary>
    /// Selects the language-specific page and shows it with the current language highlight, controller labels and toggle
    /// choices, as the cartridge reapplies them whenever it loads the page.
    /// </summary>
    private void LoadVisiblePage()
    {
        GameOptionsTilemap shown = page switch
        {
            GameOptionsPage.Primary => GameOptionsTilemap.Primary,
            GameOptionsPage.Controller => JapaneseText ? GameOptionsTilemap.ControllerJapanese : GameOptionsTilemap.ControllerEnglish,
            GameOptionsPage.Special => JapaneseText ? GameOptionsTilemap.SpecialJapanese : GameOptionsTilemap.SpecialEnglish,
            _ => throw new InvalidOperationException($"Unknown options page {page}."),
        };
        shownPage = new ShownOptionsPage(shown, JapaneseText, ControllerBindings, IconCancelEnabled, MoonwalkEnabled);
    }

    /// <summary>Uses the page cursor layout or the hidden anchor during transitions.</summary>
    private Position16<SpritePixels> CursorPosition()
    {
        GameOptionsPage? cursorPage = GameOptionsCursorPolicy.Select(Phase);
        // Keep the cursor actor in OAM at the installed hidden anchor during transitions.
        if (cursorPage is null)
        {
            Position16<SpritePixels> hidden = (mapPresentation ?? throw new InvalidOperationException(
                "Options cursor requires installed presentation assets."))
                .GameOptions.HiddenCursor;
            return new Position16<SpritePixels>(checked(hidden.X), checked(hidden.Y));
        }
        Position16<SpritePixels> point = (mapPresentation ?? throw new InvalidOperationException(
            "Options cursor requires installed presentation assets."))
            .GameOptions.CursorPosition(cursorPage.Value, SelectedItem);
        return new Position16<SpritePixels>(checked(point.X), checked(point.Y));
    }

    /// <summary>Queues the native cursor sound within the menu sound limit.</summary>
    private void QueueMoveSound() =>
        audio?.QueueSound(
            Library1Sound.MenuCursor,
            maximumQueued: GameOptionsDefinitions.MaximumQueuedMenuSounds);

    /// <summary>Queues the native confirmation sound within the menu sound limit.</summary>
    private void QueueSelectSound() =>
        audio?.QueueSound(
            Library1Sound.MenuConfirm,
            maximumQueued: GameOptionsDefinitions.MaximumQueuedMenuSounds);
}
