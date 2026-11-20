using SuperMetroid.Core.Frontend.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Complete options-menu owner, including both secondary pages.</summary>
public sealed class GameOptionsMenuState : IMenuFrontendScreen
{
    /// <inheritdoc/>
    [MemberNotNull(nameof(spriteSheets))]
    public void BindSpriteSheets(InstalledSpriteSheets sheets)
    {
        spriteSheets = Ensure.NotNull(sheets);
        screen.BindScreenPictures(sheets);
    }

    /// <summary>The installed sprite sheets the headings and cursor draw from; artwork, never saved.</summary>
    private InstalledSpriteSheets spriteSheets;

    /// <summary>Receives the cursor and confirmation sounds.</summary>
    private readonly AudioQueues audio;
    /// <summary>The menu screen the options menu draws: its artwork, pictures, colours and sprite entries.</summary>
    private readonly MenuScreen screen;
    /// <summary>Derives newly pressed buttons from the held controller buttons.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>The installed menu art, bound when the game is bound to its installed content.</summary>
    private InstalledScreenContent screenContent;
    /// <summary>The page last loaded and the state its labels, language rows and toggle choices were drawn from (#1467).</summary>
    private ShownOptionsPage ShownPage { get; set; }
    /// <summary>Page whose input and cursor policy currently apply.</summary>
    private GameOptionsPage page = GameOptionsPage.Primary;
    /// <summary>Page to load when the current page has dissolved to black.</summary>
    private GameOptionsPage dissolveDestination = GameOptionsPage.Primary;
    /// <summary>The screen's brightness, which the page fades and final exits step one level an update.</summary>
    private readonly ScreenFade fade = new(0);
    /// <summary>The primary page's selected row, which its cursor shows while the primary page does.</summary>
    private GameOptionsPrimaryRow primaryRow = GameOptionsPageRows.Primary.Top;
    /// <summary>The controller page's selected row, which its cursor shows while the controller page does.</summary>
    private GameOptionsControllerRow controllerRow = GameOptionsPageRows.Controller.Top;
    /// <summary>The special page's selected row, which its cursor shows while the special page does.</summary>
    private GameOptionsSpecialRow specialRow = GameOptionsPageRows.Special.Top;
    /// <summary>The page's vertical scroll, which exposes the lower controller rows.</summary>
    private int pageVerticalScroll;
    /// <summary>The selection missile's frame cycle.</summary>
    private readonly MenuMissileAnimation missile;
    /// <summary>The opening's black hold (<see cref="GameOptionsPhase.BlackHold"/>), counted once each update.</summary>
    private readonly UpdateCountdown blackHold = UpdateCountdown.Lasting(GameOptionsDefinitions.BlackHoldUpdates);

    /// <summary>Creates the complete primary, controller, and special options-menu state.</summary>
    /// <param name="audio">The audio queues the cursor and confirmation sounds join.</param>
    /// <param name="screenContent">The installed screen content, whose options part the screen draws.</param>
    /// <param name="spriteSheets">The installed sprite sheets, which the headings and cursor draw from.</param>
    /// <param name="controllerBindings">Initial controller-button permutation; a new game's are the defaults.</param>
    /// <param name="iconCancelEnabled">Initial state of the automatic item-cancel option.</param>
    /// <param name="moonwalkEnabled">Initial state of the moonwalk option.</param>
    /// <param name="japaneseText">Whether the Japanese-language page variants are initially selected.</param>
    [IndependentFlag("Icon cancel, moonwalk and Japanese text are three independent saved options.")]
    public GameOptionsMenuState(
        AudioQueues audio,
        InstalledScreenContent screenContent,
        InstalledSpriteSheets spriteSheets,
        ControllerBindings controllerBindings,
        bool iconCancelEnabled,
        bool moonwalkEnabled,
        bool japaneseText)
    {
        this.audio = Ensure.NotNull(audio);
        this.screenContent = Ensure.NotNull(screenContent);
        missile = new MenuMissileAnimation(screenContent.GameOptions.CursorFrameDuration);
        screen = new MenuScreen(screenContent.Backdrops.Menus, Ensure.NotNull(spriteSheets));
        BindSpriteSheets(spriteSheets);

        ControllerBindings = controllerBindings.RequireUniquePermutation();
        IconCancelEnabled = iconCancelEnabled;
        MoonwalkEnabled = moonwalkEnabled;
        JapaneseText = japaneseText;
        LoadVisiblePage();
        Phase = GameOptionsPhase.BlackHold;
    }

    /// <summary>Binds the installed menu art when the game is bound to its installed content.</summary>
    public void BindScreenContent(InstalledScreenContent catalog)
    {
        screenContent = Ensure.NotNull(catalog);
        missile.Rebind(catalog.GameOptions.CursorFrameDuration);
        screen.Backdrop = catalog.Backdrops.Menus;
        LoadVisiblePage();
    }

    /// <summary>The row the cursor is on: the selected row of the page the menu shows (#2206).</summary>
    private GameOptionsRow SelectedRow => page switch
    {
        GameOptionsPage.Primary => new GameOptionsRow.OnPrimaryPage(primaryRow),
        GameOptionsPage.Controller => new GameOptionsRow.OnControllerPage(controllerRow),
        GameOptionsPage.Special => new GameOptionsRow.OnSpecialPage(specialRow),
        _ => throw new InvalidOperationException($"Unknown options page {page}."),
    };

    /// <summary>
    /// The selected save's startup route, loaded from the save by file select.
    /// A new game's is the opening-cinematic route.
    /// </summary>
    public SaveStartupRoute StartupRoute { get; init; }

    /// <summary>Gets whether the Japanese-language options pages are selected.</summary>
    public bool JapaneseText { get; private set; }

    /// <summary>The seven live button assignments committed when the controller page exits.</summary>
    public ControllerBindings ControllerBindings { get; private set; }

    /// <summary>The automatic item-cancel option, toggled by the special page's Icon Cancel row.</summary>
    public bool IconCancelEnabled { get; private set; }

    /// <summary>The moonwalk option, toggled by the special page's Moonwalk row.</summary>
    public bool MoonwalkEnabled { get; private set; }

    /// <summary>Gets the options menu's current phase.</summary>
    public GameOptionsPhase Phase { get; private set; }

    /// <summary>How the menu hands off to the frontend (#2562): the opening sequence or file select, kept once it reaches one.</summary>
    public GameOptionsMenuOutcome Outcome { get; private set; }

    /// <summary>Advances one options-menu update using the held controller buttons.</summary>
    public void Step(ControllerButton controllerInput)
    {
        controller.Latch(controllerInput);
        ControllerButton pressed = controller.NewlyPressed;
        // The selection missile animates from the black hold's second update on.
        if (Phase != GameOptionsPhase.BlackHold || !blackHold.IsUncounted)
            missile.Step();

        switch (Phase)
        {
            case GameOptionsPhase.BlackHold:
                if (blackHold.StepEnds())
                    Phase = GameOptionsPhase.FadeIn;
                break;

            case GameOptionsPhase.FadeIn:
                if (fade.Brighten())
                    Phase = GameOptionsPhase.Main;
                break;

            case GameOptionsPhase.Main:
                StepPrimary(pressed);
                break;

            case GameOptionsPhase.DissolveOut:
                if (fade.Darken())
                {
                    page = dissolveDestination;
                    SelectFirstRow(page);
                    pageVerticalScroll = 0;
                    LoadVisiblePage();
                    Phase = GameOptionsPhase.DissolveIn;
                }
                break;

            case GameOptionsPhase.DissolveIn:
                if (fade.Brighten())
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
                pageVerticalScroll += GameOptionsDefinitions.ControllerScrollPixelsPerFrame;
                if (pageVerticalScroll == GameOptionsDefinitions.ControllerScrollLimit)
                    Phase = GameOptionsPhase.ControllerSettings;
                break;

            case GameOptionsPhase.ScrollControllerUp:
                pageVerticalScroll -= GameOptionsDefinitions.ControllerScrollPixelsPerFrame;
                if (pageVerticalScroll == 0)
                    Phase = GameOptionsPhase.ControllerSettings;
                break;

            case GameOptionsPhase.FadeOutToIntro:
                // Reaching black selects the start-game step, which
                // runs the loading game state on the following update.
                if (fade.Darken())
                    Phase = GameOptionsPhase.StartGame;
                break;

            case GameOptionsPhase.StartGame:
                Outcome = GameOptionsMenuOutcome.Intro;
                break;

            case GameOptionsPhase.FadeOutToFileSelect:
                if (fade.Darken())
                    Outcome = GameOptionsMenuOutcome.FileSelect;
                break;
        }
    }

    /// <summary>Captures the backdrop, the scrolled page with what is drawn over it, and the selector without composing pixels.</summary>
    public LayeredScene ComposeScene()
    {
        PrepareRenderSprites();
        // The backdrop, the page and its labels, language rows and toggle choices are the menus' pictures (#1467).
        return MenuSceneComposer.Capture(screen, screen.PagePictures(GameOptionsNames.BackgroundFrame),
            screen.PagePictures(ShownPage.Frames()), pageVerticalScroll, fade.Level);
    }

    /// <summary>Draws the current page heading and animated cursor as sprite entries.</summary>
    private void PrepareRenderSprites()
    {
        ScreenSprites sprites = screen.Sprites;
        sprites.BeginFrame();
        var content = screenContent;
        SpriteSheet sheet = spriteSheets.Get(SpriteSheetId.GameOptions);
        content.GameOptions.DrawHeading(sprites, sheet, page, pageVerticalScroll);
        // Transitions show no cursor: only a page phase places it, at the selected row of the page it shows.
        if (GameOptionsCursorPolicy.Select(Phase) is { } cursorPage)
        {
            GameOptionsRow selected = SelectedRow;
            if (selected.Page != cursorPage)
                throw new InvalidOperationException($"Options phase {Phase} shows the {cursorPage} page's cursor on {selected}.");
            GameOptionsPresentation.DrawCursor(sprites, sheet, missile.Frame, content.GameOptions.CursorPosition(selected));
        }
        sprites.FinishFrame();
    }

    /// <summary>Handles primary rows for language, subpages, start, and cancel.</summary>
    private void StepPrimary(ControllerButton pressed)
    {
        if ((pressed & ControllerButton.Up) != ControllerButton.None)
        {
            primaryRow = GameOptionsPageRows.Primary.Above(primaryRow, out _);
            QueueMoveSound();
        }
        else if ((pressed & ControllerButton.Down) != ControllerButton.None)
        {
            primaryRow = GameOptionsPageRows.Primary.Below(primaryRow, out _);
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
        switch (primaryRow)
        {
            case GameOptionsPrimaryRow.StartGame:
                // Starting: a save that resumes ordinary gameplay starts at once;
                // a new game and the cinematic routes fade out first.
                Phase = StartupRoute == SaveStartupRoute.MainGame
                    ? GameOptionsPhase.StartGame
                    : GameOptionsPhase.FadeOutToIntro;
                break;
            case GameOptionsPrimaryRow.EnglishText:
            case GameOptionsPrimaryRow.JapaneseText:
                JapaneseText = !JapaneseText;
                SelectFirstRow(GameOptionsPage.Primary);
                LoadVisiblePage();
                break;
            case GameOptionsPrimaryRow.ControllerSettings:
                BeginDissolveTo(GameOptionsPage.Controller);
                break;
            case GameOptionsPrimaryRow.SpecialSettings:
                BeginDissolveTo(GameOptionsPage.Special);
                break;
        }
    }

    /// <summary>Handles button swaps, reset and exit rows, and upper/lower page scrolling.</summary>
    private void StepController(ControllerButton pressed)
    {
        // The page scrolls down to show Exit and Reset as the cursor reaches them, and back up to show the action rows.
        if ((pressed & ControllerButton.Up) != ControllerButton.None)
        {
            QueueMoveSound();
            GameOptionsControllerRow above = GameOptionsPageRows.Controller.Above(controllerRow, out bool wrapped);
            if (wrapped)
                Phase = GameOptionsPhase.ScrollControllerDown;
            else if (controllerRow is GameOptionsControllerRow.Exit && above is GameOptionsControllerRow.AssignsAction)
                Phase = GameOptionsPhase.ScrollControllerUp;
            controllerRow = above;
            return;
        }
        if ((pressed & ControllerButton.Down) != ControllerButton.None)
        {
            QueueMoveSound();
            controllerRow = GameOptionsPageRows.Controller.Below(controllerRow, out bool wrapped);
            if (wrapped)
                Phase = GameOptionsPhase.ScrollControllerUp;
            else if (controllerRow is GameOptionsControllerRow.Exit)
                Phase = GameOptionsPhase.ScrollControllerDown;
            return;
        }
        if (pressed == ControllerButton.None)
            return;

        // The game queues the confirmation sound for any newly pressed button on this page,
        // even when an action row cannot find an assignable button among them.
        QueueSelectSound();
        if (controllerRow is GameOptionsControllerRow.AssignsAction { Action: var action })
        {
            for (int button = Input.ControllerBindings.AssignableButtons.Length - 1; button >= 0; button--)
            {
                ControllerButton physicalButton = Input.ControllerBindings.AssignableButtons[button];
                if ((pressed & physicalButton) == ControllerButton.None)
                    continue;
                ControllerBindings = ControllerBindings.AssignAndSwap(action, physicalButton);
                LoadVisiblePage();
                break;
            }
            return;
        }

        if ((pressed & (ControllerButton.Start | ControllerButton.A)) == ControllerButton.None)
            return;
        switch (controllerRow)
        {
            case GameOptionsControllerRow.Exit:
                BeginDissolveTo(GameOptionsPage.Primary);
                break;
            case GameOptionsControllerRow.Reset:
                ControllerBindings = Input.ControllerBindings.Default;
                LoadVisiblePage();
                break;
            case GameOptionsControllerRow.AssignsAction:
                throw new InvalidOperationException("Controller action rows are handled before navigation rows.");
        }
    }

    /// <summary>Handles icon-cancel and moonwalk toggles or returns to the primary page.</summary>
    private void StepSpecial(ControllerButton pressed)
    {
        if ((pressed & ControllerButton.Up) != ControllerButton.None)
        {
            specialRow = GameOptionsPageRows.Special.Above(specialRow, out _);
            QueueMoveSound();
        }
        else if ((pressed & ControllerButton.Down) != ControllerButton.None)
        {
            specialRow = GameOptionsPageRows.Special.Below(specialRow, out _);
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
        switch (specialRow)
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
        }
        LoadVisiblePage();
    }

    /// <summary>Selects <paramref name="shown"/>'s top row, as every page load and language change does.</summary>
    private void SelectFirstRow(GameOptionsPage shown)
    {
        switch (shown)
        {
            case GameOptionsPage.Primary:
                primaryRow = GameOptionsPageRows.Primary.Top;
                break;
            case GameOptionsPage.Controller:
                controllerRow = GameOptionsPageRows.Controller.Top;
                break;
            case GameOptionsPage.Special:
                specialRow = GameOptionsPageRows.Special.Top;
                break;
        }
    }

    /// <summary>Records the destination before starting the page fade-out.</summary>
    private void BeginDissolveTo(GameOptionsPage destination)
    {
        dissolveDestination = destination;
        Phase = GameOptionsPhase.DissolveOut;
    }

    /// <summary>
    /// Selects the language-specific page and shows it with the current language highlight, controller labels and toggle
    /// choices, as the game reapplies them whenever it loads the page.
    /// </summary>
    [MemberNotNull(nameof(ShownPage))]
    private void LoadVisiblePage()
    {
        GameOptionsScreen shown = page switch
        {
            GameOptionsPage.Primary => GameOptionsScreen.Primary,
            GameOptionsPage.Controller => JapaneseText ? GameOptionsScreen.ControllerJapanese : GameOptionsScreen.ControllerEnglish,
            GameOptionsPage.Special => JapaneseText ? GameOptionsScreen.SpecialJapanese : GameOptionsScreen.SpecialEnglish,
            _ => throw new InvalidOperationException($"Unknown options page {page}."),
        };
        ShownPage = new ShownOptionsPage(shown, JapaneseText, ControllerBindings, IconCancelEnabled, MoonwalkEnabled);
    }

    /// <summary>Queues the cursor sound at the menu's sound priority.</summary>
    private void QueueMoveSound() =>
        audio.QueueSound(
            SamusSound.MenuCursor,
            priority: GameOptionsDefinitions.MenuSoundPriority);

    /// <summary>Queues the confirmation sound at the menu's sound priority.</summary>
    private void QueueSelectSound() =>
        audio.QueueSound(
            SamusSound.MenuConfirm,
            priority: GameOptionsDefinitions.MenuSoundPriority);
}
