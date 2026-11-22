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
public sealed partial class GameOptionsMenuState : IMenuFrontendScreen, IComposedFrontendScene<LayeredScene>
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
    /// <summary>The menu screen the options menu draws: its artwork, pictures, colours and sprites.</summary>
    internal readonly MenuScreen screen;
    /// <summary>Derives newly pressed buttons from the held controller buttons.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>The installed menu art, bound when the game is bound to its installed content.</summary>
    private InstalledScreenContent screenContent;
    /// <summary>The page last loaded and the state its labels, language rows and toggle choices were drawn from (#1467).</summary>
    internal ShownOptionsPage ShownPage { get; set; }

    /// <summary>Page to load when the current page has dissolved to black.</summary>
    private GameOptionsPage dissolveDestination = GameOptionsPage.Primary;
    /// <summary>The screen's brightness, which the page fades and final exits step one level an update.</summary>
    private readonly ScreenFade fade = new(DisplayBrightness.Black);
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
    /// <param name="language">The text language initially chosen, whose page variants the menu shows.</param>
    public GameOptionsMenuState(
        AudioQueues audio,
        InstalledScreenContent screenContent,
        InstalledSpriteSheets spriteSheets,
        ControllerBindings controllerBindings,
        bool iconCancelEnabled,
        bool moonwalkEnabled,
        TextLanguage language)
    {
        this.audio = Ensure.NotNull(audio);
        this.screenContent = Ensure.NotNull(screenContent);
        missile = new MenuMissileAnimation(screenContent.GameOptions.CursorFrameDuration);
        screen = new MenuScreen(screenContent.Backdrops.Menus, Ensure.NotNull(spriteSheets));
        BindSpriteSheets(spriteSheets);

        ControllerBindings = controllerBindings.RequireUniquePermutation();
        IconCancelEnabled = iconCancelEnabled;
        MoonwalkEnabled = moonwalkEnabled;
        Language = language;
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

    /// <summary>The row the cursor selects on each page, read from the menu's row on that page.</summary>
    private static IdentityTable<GameOptionsPage, Func<GameOptionsMenuState, GameOptionsRow>> SelectedRows { get; } = new(new Dictionary<GameOptionsPage, Func<GameOptionsMenuState, GameOptionsRow>>
    {
        [GameOptionsPage.Primary] = static menu => new GameOptionsRow.OnPrimaryPage(menu.primaryRow),
        [GameOptionsPage.Controller] = static menu => new GameOptionsRow.OnControllerPage(menu.controllerRow),
        [GameOptionsPage.Special] = static menu => new GameOptionsRow.OnSpecialPage(menu.specialRow),
    });

    /// <summary>The row the cursor is on: the selected row of the page the menu shows (#2206).</summary>
    internal GameOptionsRow SelectedRow => SelectedRows[Page](this);

    /// <summary>
    /// The selected save's startup route, loaded from the save by file select.
    /// A new game's is the opening-cinematic route.
    /// </summary>
    public SaveStartupRoute StartupRoute { get; init; }

    /// <summary>Gets whether the Japanese-language options pages are selected.</summary>
    public TextLanguage Language { get; private set; }

    /// <summary>The page the menu shows: the page taking input, or the page a transition shows (#2560).</summary>
    internal GameOptionsPage Page { get; set; } = GameOptionsPage.Primary;

    /// <summary>The seven live button assignments committed when the controller page exits.</summary>
    public ControllerBindings ControllerBindings { get; private set; }

    /// <summary>The automatic item-cancel option, toggled by the special page's Icon Cancel row.</summary>
    public bool IconCancelEnabled { get; private set; }

    /// <summary>The moonwalk option, toggled by the special page's Moonwalk row.</summary>
    public bool MoonwalkEnabled { get; private set; }

    /// <summary>Gets the options menu's current phase.</summary>
    public GameOptionsPhase Phase { get; internal set; }

    /// <summary>
    /// How the menu hands off to the frontend (#2562): the opening sequence or file select, kept once it reaches one;
    /// absent while the menu runs, before it has reached either (#2587).
    /// </summary>
    public GameOptionsMenuOutcome? Outcome { get; private set; }

    /// <summary>What browsing does on each page: its own rows' controls.</summary>
    private IdentityTable<GameOptionsPage, Action<ControllerButton>> BrowsePage => field ??= new(new Dictionary<GameOptionsPage, Action<ControllerButton>>
    {
        [GameOptionsPage.Primary] = StepPrimary,
        [GameOptionsPage.Controller] = StepController,
        [GameOptionsPage.Special] = StepSpecial,
    });

    /// <summary>What <c>Step</c> does for each <see cref="GameOptionsPhase"/> member, each member's own arm.</summary>
    private IdentityTable<GameOptionsPhase, Action<ControllerButton>> StepForGameOptionsPhase => field ??= new(new Dictionary<GameOptionsPhase, Action<ControllerButton>>
    {
        [GameOptionsPhase.BlackHold] = pressed =>
        {
            if (blackHold.StepEnds())
                Phase = GameOptionsPhase.FadeIn;
        },
        [GameOptionsPhase.FadeIn] = pressed =>
        {
            if (fade.Brighten())
                Phase = GameOptionsPhase.Browsing;
        },
        [GameOptionsPhase.Browsing] = pressed => BrowsePage[Page](pressed),
        [GameOptionsPhase.DissolveOut] = pressed =>
        {
            if (fade.Darken())
            {
                Page = dissolveDestination;
                SelectFirstRow(Page);
                pageVerticalScroll = 0;
                LoadVisiblePage();
                Phase = GameOptionsPhase.DissolveIn;
            }
        },
        [GameOptionsPhase.DissolveIn] = pressed =>
        {
            if (fade.Brighten())
                Phase = GameOptionsPhase.Browsing;
        },
        [GameOptionsPhase.ScrollControllerDown] = pressed =>
        {
            pageVerticalScroll += GameOptionsDefinitions.ControllerScrollPixelsPerFrame;
            if (pageVerticalScroll == GameOptionsDefinitions.ControllerScrollLimit)
                Phase = GameOptionsPhase.Browsing;
        },
        [GameOptionsPhase.ScrollControllerUp] = pressed =>
        {
            pageVerticalScroll -= GameOptionsDefinitions.ControllerScrollPixelsPerFrame;
            if (pageVerticalScroll == 0)
                Phase = GameOptionsPhase.Browsing;
        },
        [GameOptionsPhase.FadeOutToIntro] = pressed =>
        {
            // Reaching black selects the start-game step, which
            // runs the loading game state on the following update.
            if (fade.Darken())
                Phase = GameOptionsPhase.StartGame;
        },
        [GameOptionsPhase.StartGame] = pressed => Outcome = GameOptionsMenuOutcome.Intro,
        [GameOptionsPhase.FadeOutToFileSelect] = pressed =>
        {
            if (fade.Darken())
                Outcome = GameOptionsMenuOutcome.FileSelect;
        },
    });

    /// <summary>Advances one options-menu update using the held controller buttons.</summary>
    public void Step(ControllerButton controllerInput)
    {
        controller.Latch(controllerInput);
        ControllerButton pressed = controller.NewlyPressed;
        // The selection missile animates from the black hold's second update on.
        if (Phase != GameOptionsPhase.BlackHold || !blackHold.IsUncounted)
            missile.Step();

        StepForGameOptionsPhase[Phase](pressed);
    }

    /// <summary>Captures the backdrop, the scrolled page with what is drawn over it, and the selector without composing pixels.</summary>
    public LayeredScene ComposeScene()
    {
        PrepareRenderSprites();
        // The backdrop, the page and its labels, language rows and toggle choices are the menus' pictures (#1467).
        return MenuSceneComposer.Capture(screen, screen.PagePictures(GameOptionsNames.BackgroundFrame),
            screen.PagePictures(ShownPage.Frames()), pageVerticalScroll, fade.Level);
    }

    /// <summary>Draws the current page heading and animated cursor as sprites.</summary>
    private void PrepareRenderSprites()
    {
        ScreenSprites sprites = screen.Sprites;
        sprites.BeginFrame();
        var content = screenContent;
        SpriteSheet sheet = spriteSheets.Get(SpriteSheetId.GameOptions);
        content.GameOptions.DrawHeading(sprites, sheet, Page, pageVerticalScroll);
        // Transitions show no cursor: only a page phase places it, at the selected row of the page it shows.
        if (GameOptionsCursorPolicy.Select(Phase, Page) is { } cursorPage)
        {
            GameOptionsRow selected = SelectedRow;
            if (selected.Page != cursorPage)
                throw new InvalidOperationException($"Options phase {Phase} shows the {cursorPage} page's cursor on {selected}.");
            GameOptionsPresentation.DrawCursor(sprites, sheet, missile.Frame, content.GameOptions.CursorPosition(selected));
        }
        sprites.FinishFrame();
    }

    /// <summary>What <see cref="StepPrimary"/> does for each <see cref="GameOptionsPrimaryRow"/> member, each member's own arm.</summary>
    private IdentityTable<GameOptionsPrimaryRow, Action> StepPrimaryForGameOptionsPrimaryRow => field ??= new(new Dictionary<GameOptionsPrimaryRow, Action>
    {
        [GameOptionsPrimaryRow.StartGame] = () =>
        {
            // Starting: a save that resumes ordinary gameplay starts at once;
            // a new game and the cinematic routes fade out first.
            Phase = StartupRoute == SaveStartupRoute.MainGame
                ? GameOptionsPhase.StartGame
                : GameOptionsPhase.FadeOutToIntro;
        },
        [GameOptionsPrimaryRow.EnglishText] = () => StepPrimaryForGameOptionsPrimaryRowEnglishText(),
        [GameOptionsPrimaryRow.JapaneseText] = () => StepPrimaryForGameOptionsPrimaryRowEnglishText(),
        [GameOptionsPrimaryRow.ControllerSettings] = () => BeginDissolveTo(GameOptionsPage.Controller),
        [GameOptionsPrimaryRow.SpecialSettings] = () => BeginDissolveTo(GameOptionsPage.Special),
    });

    /// <summary>What <see cref="StepPrimaryForGameOptionsPrimaryRow"/> does for <see cref="GameOptionsPrimaryRow.EnglishText"/>, <see cref="GameOptionsPrimaryRow.JapaneseText"/>.</summary>
    private void StepPrimaryForGameOptionsPrimaryRowEnglishText()
    {
        Language = Language == TextLanguage.Japanese ? TextLanguage.English : TextLanguage.Japanese;
        SelectFirstRow(GameOptionsPage.Primary);
        LoadVisiblePage();
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
        StepPrimaryForGameOptionsPrimaryRow[primaryRow]();
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
        controllerRow.React(StepControllerReactions, this);
    }

    /// <summary>What <see cref="StepController"/> does on each <see cref="GameOptionsControllerRow"/>, built once (#2553).</summary>
    private static GameOptionsControllerRowReactions<GameOptionsMenuState> StepControllerReactions { get; } = new(
        AssignsAction: static (_, _) =>
        {
            throw new InvalidOperationException("Controller action rows are handled before navigation rows.");
        },
        Exit: static (_, owner) => owner.BeginDissolveTo(GameOptionsPage.Primary),
        Reset: static (_, owner) =>
        {
            owner.ControllerBindings = Input.ControllerBindings.Default;
            owner.LoadVisiblePage();
        });

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
        SpecialRowChoices[specialRow](this);
    }

    /// <summary>
    /// What choosing each special-page row does (#2553): a toggle flips its setting and redraws the page, and Exit dissolves
    /// to the primary page.
    /// </summary>
    private static IdentityTable<GameOptionsSpecialRow, Action<GameOptionsMenuState>> SpecialRowChoices => field ??= new(
        new Dictionary<GameOptionsSpecialRow, Action<GameOptionsMenuState>>
        {
            [GameOptionsSpecialRow.IconCancel] = static menu =>
            {
                menu.IconCancelEnabled = !menu.IconCancelEnabled;
                menu.LoadVisiblePage();
            },
            [GameOptionsSpecialRow.Moonwalk] = static menu =>
            {
                menu.MoonwalkEnabled = !menu.MoonwalkEnabled;
                menu.LoadVisiblePage();
            },
            [GameOptionsSpecialRow.Exit] = static menu => menu.BeginDissolveTo(GameOptionsPage.Primary),
        });

    /// <summary>What <see cref="SelectFirstRow"/> does for each <see cref="GameOptionsPage"/> member, each member's own arm.</summary>
    private IdentityTable<GameOptionsPage, Action> SelectFirstRowForGameOptionsPage => field ??= new(new Dictionary<GameOptionsPage, Action>
    {
        [GameOptionsPage.Primary] = () => primaryRow = GameOptionsPageRows.Primary.Top,
        [GameOptionsPage.Controller] = () => controllerRow = GameOptionsPageRows.Controller.Top,
        [GameOptionsPage.Special] = () => specialRow = GameOptionsPageRows.Special.Top,
    });

    /// <summary>Selects <paramref name="shown"/>'s top row, as every page load and language change does.</summary>
    private void SelectFirstRow(GameOptionsPage shown)
    {
        SelectFirstRowForGameOptionsPage[shown]();
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
        ShownPage = new ShownOptionsPage(Page, Language, ControllerBindings, IconCancelEnabled, MoonwalkEnabled);
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
