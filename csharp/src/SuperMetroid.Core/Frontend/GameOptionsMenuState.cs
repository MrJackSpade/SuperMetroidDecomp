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
        Phase = GameOptionsPhase.FadeIn;
    }

    /// <summary>Rebinds host-owned visual assets after a debugger-state restore.</summary>
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        mapPresentation = catalog ?? throw new InvalidOperationException(
            "Options menu requires installed presentation assets.");
        ppu.BindWorldArtwork(bus, catalog.WorldArtwork);
        ppu.BindMapTiles(bus, catalog.Tiles);
        ppu.BindMapSprites(bus, catalog.Sprites);
        ppu.BindMapPalettes(bus, catalog.Palettes);
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

    public bool JapaneseText { get; private set; }

    /// <summary>The seven live controller words committed when the controller page exits.</summary>
    public ControllerBindings ControllerBindings { get; private set; }

    /// <summary>WRAM <c>$09EA</c>, toggled by special-settings row zero.</summary>
    public bool IconCancelEnabled { get; private set; }

    /// <summary>WRAM <c>$09E4</c>, toggled by special-settings row one.</summary>
    public bool MoonwalkEnabled { get; private set; }

    public GameOptionsPhase Phase { get; private set; }

    public bool IntroRequested { get; private set; }

    public bool FileSelectRequested { get; private set; }

    public void Step(ushort controllerInput)
    {
        controller.Latch(controllerInput);
        SnesButton pressed = controller.NewlyPressedButtons;
        StepMissile();

        switch (Phase)
        {
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
                brightness = Math.Max(0, brightness - 1);
                if (brightness == 0)
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
            int button = Input.ControllerBindings.AssignableButtonIndex(ControllerBindings[action]);
            content.GameOptions.ApplyControllerLabel(visibleTilemap, action,
                button < 0 ? 0 : button);
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

public enum GameOptionsPhase
{
    FadeIn,
    Main,
    DissolveOut,
    DissolveIn,
    ControllerSettings,
    SpecialSettings,
    ScrollControllerDown,
    ScrollControllerUp,
    FadeOutToIntro,
    FadeOutToFileSelect,
}

internal enum GameOptionsPage
{
    Primary,
    Controller,
    Special,
}
