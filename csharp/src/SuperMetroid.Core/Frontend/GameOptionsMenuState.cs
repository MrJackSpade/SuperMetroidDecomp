using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>Complete game-state-$02 options owner, including both secondary pages.</summary>
public sealed class GameOptionsMenuState
{
    // OBJ layer reused across renders; the span overload clears it first. Never saved state.
    [NonSerialized] private Rgba32[]? objectLayerScratch;
    // Final frame, reused by every render: a returned frame is valid until this scene renders again.
    [NonSerialized] private Rgba32[]? frameBuffer;

    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
    private readonly MenuPpuState ppu;
    private readonly OamBuffer oam = new();
    private readonly ControllerInputState controller = new();
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    private readonly byte[] primaryTilemap;
    private readonly byte[] controllerEnglishTilemap;
    private readonly byte[] controllerJapaneseTilemap;
    private readonly byte[] specialEnglishTilemap;
    private readonly byte[] specialJapaneseTilemap;
    private byte[] visibleTilemap;
    private GameOptionsPage page = GameOptionsPage.Primary;
    private GameOptionsPage dissolveDestination = GameOptionsPage.Primary;
    private int brightness;
    private int bg1VerticalScroll;
    private int missileTimer = 1;
    private int missileFrame;

    /// <summary>Creates the complete primary, controller, and special options-menu state.</summary>
    /// <param name="bus">Address space used by the menu PPU resource loader.</param>
    /// <param name="audio">Optional cartridge audio queues for cursor and confirmation sounds.</param>
    /// <param name="controllerBindings">Initial retail controller-button permutation.</param>
    /// <param name="iconCancelEnabled">Initial state of the automatic item-cancel option.</param>
    /// <param name="moonwalkEnabled">Initial state of the moonwalk option.</param>
    /// <param name="japaneseText">Whether the Japanese-language page variants are initially selected.</param>
    /// <param name="mapPresentation">Required installed menu presentation assets.</param>
    public GameOptionsMenuState(
        ISnesAddressSpace bus,
        CartridgeAudioState? audio = null,
        ControllerBindings? controllerBindings = null,
        bool iconCancelEnabled = false,
        bool moonwalkEnabled = false,
        bool japaneseText = false,
        AreaMapPresentationCatalog? mapPresentation = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio;
        this.mapPresentation = mapPresentation ?? throw new InvalidOperationException(
            "Options menu requires installed presentation assets.");
        ppu = new MenuPpuState(bus, mapPresentation.Tiles, mapPresentation.Palettes,
            mapPresentation.WorldArtwork, mapPresentation.Sprites, loadInitialBackground: false);
        mapPresentation.GameOptions.LoadBackground(ppu.Vram);

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
        ppu.BindWorldArtwork(catalog.WorldArtwork);
        ppu.BindMapTiles(catalog.Tiles);
        ppu.BindMapSprites(catalog.Sprites);
        ppu.BindMapPalettes(catalog.Palettes);
        catalog.GameOptions.LoadBackground(ppu.Vram);
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

    /// <summary>Gets whether the Japanese-language options pages are selected.</summary>
    public bool JapaneseText { get; private set; }

    /// <summary>The seven live controller words committed when the controller page exits.</summary>
    public ControllerBindings ControllerBindings { get; private set; }

    /// <summary>WRAM <c>$09EA</c>, toggled by special-settings row zero.</summary>
    public bool IconCancelEnabled { get; private set; }

    /// <summary>WRAM <c>$09E4</c>, toggled by special-settings row one.</summary>
    public bool MoonwalkEnabled { get; private set; }

    /// <summary>Gets the current options-menu coroutine phase.</summary>
    public GameOptionsPhase Phase { get; private set; }

    /// <summary>Gets whether the completed start-game fade requested the opening sequence.</summary>
    public bool IntroRequested { get; private set; }

    /// <summary>Gets whether the completed cancel fade requested the file-select menu.</summary>
    public bool FileSelectRequested { get; private set; }

    /// <summary>Advances one options-menu update using the raw held controller word.</summary>
    public void Step(ushort controllerInput)
    {
        controller.Latch(controllerInput);
        SnesButton pressed = controller.NewlyPressedButtons;
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

    /// <summary>Renders the current options page, cursor, and brightness into a reusable RGBA frame.</summary>
    public Rgba32[] Render()
    {
        Rgba32[] background = SnesLayerCompositor.CreateBackdrop(ppu.Cgram, FrontendFrame.Width * FrontendFrame.Height, frameBuffer ??= new Rgba32[FrontendFrame.Width * FrontendFrame.Height]);
        SnesBgTilemapRenderer.Composite4BppViewport(
            background,
            ppu.Vram, ppu.Cgram, MenuPpuState.Bg2TilemapWord, 0, 0, 0,
            FrontendFrame.Width, FrontendFrame.Height,
            GameOptionsRomData.MenuTilemapWidth, GameOptionsRomData.MenuTilemapHeight);
        SnesBgTilemapRenderer.Composite4BppViewport(
            background,
            ppu.Vram,
            ppu.Cgram,
            MenuPpuState.Bg1TilemapWord,
            0,
            0,
            unchecked((ushort)bg1VerticalScroll),
            FrontendFrame.Width,
            FrontendFrame.Height,
            GameOptionsRomData.MenuTilemapWidth,
            GameOptionsRomData.MenuTilemapHeight);

        PrepareRenderOam();
        Rgba32[] objectLayer = objectLayerScratch ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
        SnesObjRenderer.Render(objectLayer, oam, ppu.Vram, ppu.Cgram, obsel: MenuRenderDefinitions.ObjectSelection);
        SnesLayerCompositor.Composite(
            background,
            objectLayer);
        ApplyBrightness(background);
        return background;
    }

    /// <summary>Captures the scrolled options page and selector without composing pixels.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderOam();
        return MenuRenderSnapshotCapture.Capture(ppu, oam,
            unchecked((ushort)bg1VerticalScroll), checked((byte)brightness));
    }

    private void PrepareRenderOam()
    {
        oam.BeginFrame();
        var content = mapPresentation ?? throw new InvalidOperationException(
            "Options cursor requires installed presentation assets.");
        string pageName = PresentationPageName(page);
        content.GameOptions.DrawHeading(oam, pageName, bg1VerticalScroll);
        (ushort authoredCursorX, ushort authoredCursorY) = CursorPosition();
        content.GameOptions.DrawCursor(oam, missileFrame,
            new(authoredCursorX, authoredCursorY));
        oam.FinalizeFrame();
    }

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
        switch (SelectedItem)
        {
            case GameOptionsRomData.Rows.PrimaryStartGame:
                Phase = GameOptionsPhase.FadeOutToIntro;
                break;
            case 1:
            case 2:
                JapaneseText = !JapaneseText;
                SelectedItem = 0;
                ApplyLanguagePaletteBits();
                LoadVisiblePage();
                break;
            case GameOptionsRomData.Rows.PrimaryControllerSettings:
                BeginDissolveTo(GameOptionsPage.Controller);
                break;
            case GameOptionsRomData.Rows.PrimarySpecialSettings:
                BeginDissolveTo(GameOptionsPage.Special);
                break;
        }
    }

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
            if (SelectedItem == GameOptionsRomData.Rows.ControllerExit)
            {
                Phase = GameOptionsPhase.ScrollControllerDown;
            }
            else if (SelectedItem == GameOptionsRomData.Rows.ControllerCount)
            {
                SelectedItem = 0;
                Phase = GameOptionsPhase.ScrollControllerUp;
            }
            return;
        }
        if (pressed == SnesButton.None)
            return;

        // Native queues the confirmation sound for any newly pressed word on this page,
        // even when an action row cannot find an assignable button in that word.
        QueueSelectSound();
        if (SelectedItem < GameOptionsRomData.Rows.ControllerActionCount)
        {
            for (int button = Input.ControllerBindings.AssignableButtonCount - 1; button >= 0; button--)
            {
                ushort physicalButton = Input.ControllerBindings.AssignableButton(button);
                if (((ushort)pressed & physicalButton) == 0)
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
        if (SelectedItem == GameOptionsRomData.Rows.ControllerExit)
        {
            BeginDissolveTo(GameOptionsPage.Primary);
        }
        else
        {
            ControllerBindings = Input.ControllerBindings.Default;
            ApplyControllerLabels();
            LoadVisiblePage();
        }
    }

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
        if (SelectedItem == GameOptionsRomData.Rows.SpecialIconCancel)
            IconCancelEnabled = !IconCancelEnabled;
        else if (SelectedItem == GameOptionsRomData.Rows.SpecialMoonwalk)
            MoonwalkEnabled = !MoonwalkEnabled;
        else
        {
            BeginDissolveTo(GameOptionsPage.Primary);
            return;
        }
        ApplySpecialPaletteBits();
        LoadVisiblePage();
    }

    private void BeginDissolveTo(GameOptionsPage destination)
    {
        dissolveDestination = destination;
        Phase = GameOptionsPhase.DissolveOut;
    }

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
        ppu.LoadBg1(visibleTilemap);
    }

    private void ApplyLanguagePaletteBits()
    {
        (mapPresentation ?? throw new InvalidOperationException(
            "Options language requires installed presentation assets."))
            .GameOptions.ApplyLanguage(primaryTilemap, JapaneseText);
    }

    private void ApplySpecialPaletteBits()
    {
        var content = mapPresentation ?? throw new InvalidOperationException(
            "Options toggles require installed presentation assets.");
        content.GameOptions.ApplySpecialToggle(visibleTilemap,
            GameOptionsPresentationDefinitions.IconCancelToggle, IconCancelEnabled);
        content.GameOptions.ApplySpecialToggle(visibleTilemap,
            GameOptionsPresentationDefinitions.MoonwalkToggle, MoonwalkEnabled);
    }

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

    private (ushort X, ushort Y) CursorPosition()
    {
        GameOptionsPage? cursorPage = GameOptionsCursorPolicy.Select(Phase);
        // Keep the cursor actor in OAM at the installed hidden anchor during transitions.
        if (cursorPage is null)
        {
            MapLabelPoint hidden = (mapPresentation ?? throw new InvalidOperationException(
                "Options cursor requires installed presentation assets."))
                .GameOptions.HiddenCursor;
            return (checked((ushort)hidden.X), checked((ushort)hidden.Y));
        }
        MapLabelPoint point = (mapPresentation ?? throw new InvalidOperationException(
            "Options cursor requires installed presentation assets."))
            .GameOptions.CursorPosition(PresentationPageName(cursorPage.Value), SelectedItem);
        return (checked((ushort)point.X), checked((ushort)point.Y));
    }

    private void StepMissile()
    {
        if (--missileTimer != 0)
            return;
        missileFrame = (missileFrame + 1) % MenuMissileAnimationDefinitions.FrameCount;
        missileTimer = mapPresentation?.GameOptions.CursorFrameDuration ??
            MenuMissileAnimationDefinitions.FrameDuration;
    }

    private static string PresentationPageName(GameOptionsPage value) => value switch
    {
        GameOptionsPage.Primary => GameOptionsPresentationDefinitions.PrimaryMenu,
        GameOptionsPage.Controller => GameOptionsPresentationDefinitions.ControllerMenu,
        GameOptionsPage.Special => GameOptionsPresentationDefinitions.SpecialMenu,
        _ => throw new InvalidOperationException($"Unknown options page {value}."),
    };

    private void QueueMoveSound() =>
        audio?.QueueSound(
            SoundEffectLibrary1Sounds.MenuCursor,
            maximumQueued: GameOptionsRomData.MaximumQueuedMenuSounds);

    private void QueueSelectSound() =>
        audio?.QueueSound(
            SoundEffectLibrary1Sounds.MenuConfirm,
            maximumQueued: GameOptionsRomData.MaximumQueuedMenuSounds);

    private void ApplyBrightness(Span<Rgba32> pixels)
    {
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            Rgba32 color = pixels[pixel];
            if (brightness <= 0)
                pixels[pixel] = new Rgba32(0, 0, 0, color.A);
            else if (brightness < GameOptionsRomData.MaximumBrightness)
            {
                pixels[pixel] = new Rgba32(
                    (byte)(color.R * brightness / GameOptionsRomData.MaximumBrightness),
                    (byte)(color.G * brightness / GameOptionsRomData.MaximumBrightness),
                    (byte)(color.B * brightness / GameOptionsRomData.MaximumBrightness),
                    color.A);
            }
        }
    }
}

/// <summary>Identifies the current stage of the game-options menu state machine.</summary>
public enum GameOptionsPhase
{
    /// <summary>Increase screen brightness to the menu maximum.</summary>
    FadeIn,
    /// <summary>Handle the primary options-page selection.</summary>
    Main,
    /// <summary>Fade the current page to black before changing pages.</summary>
    DissolveOut,
    /// <summary>Fade the newly loaded page in from black.</summary>
    DissolveIn,
    /// <summary>Handle controller-binding rows and commands.</summary>
    ControllerSettings,
    /// <summary>Handle icon-cancel and moonwalk settings.</summary>
    SpecialSettings,
    /// <summary>Scroll the controller page toward its lower rows.</summary>
    ScrollControllerDown,
    /// <summary>Scroll the controller page toward its upper rows.</summary>
    ScrollControllerUp,
    /// <summary>Fade to forced blank before starting the opening sequence.</summary>
    FadeOutToIntro,
    /// <summary>Fade to forced blank before returning to file select.</summary>
    FadeOutToFileSelect,
    // Appended to keep legacy debugger snapshot ordinals stable.
    /// <summary>Publish the opening-sequence request after the preceding fade completes.</summary>
    StartGame,
    /// <summary>Native index zero: finish the previous menu's fade-out at forced blank.</summary>
    FinishFadingOut,
    /// <summary>Native index one: load the menu pages and spawn the selection missile.</summary>
    LoadingMenu,
}

internal enum GameOptionsPage
{
    Primary,
    Controller,
    Special,
}
