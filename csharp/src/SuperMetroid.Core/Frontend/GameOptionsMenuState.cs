using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Complete game-state-$02 options owner, including both secondary pages.</summary>
public sealed class GameOptionsMenuState
{
    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
    private readonly MenuPpuState ppu;
    private readonly OamBuffer oam = new();
    private readonly ControllerInputState controller = new();
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
        bool japaneseText = false)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio;
        ppu = new MenuPpuState(bus);

        // `$82:EC77-$ECA3` expands these five consecutive one-screen resources. Keeping
        // each decompressed page independent mirrors their WRAM allocation and prevents a
        // language toggle from mutating the other language's source page.
        primaryTilemap = DecompressOptionsPage(GameOptionsRomData.Pages.Primary);
        controllerEnglishTilemap = DecompressOptionsPage(GameOptionsRomData.Pages.ControllerEnglish);
        controllerJapaneseTilemap = DecompressOptionsPage(GameOptionsRomData.Pages.ControllerJapanese);
        specialEnglishTilemap = DecompressOptionsPage(GameOptionsRomData.Pages.SpecialEnglish);
        specialJapaneseTilemap = DecompressOptionsPage(GameOptionsRomData.Pages.SpecialJapanese);

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
        SnesButton pressed = (SnesButton)controller.NewlyPressed;
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
        Rgba32[] background = SnesLayerCompositor.CreateBackdrop(
            ppu.Cgram,
            FrontendFrame.Width * FrontendFrame.Height);
        Rgba32[] backgroundLayer = SnesBgTilemapRenderer.Render4BppViewport(
            ppu.Vram, ppu.Cgram, MenuPpuState.Bg2TilemapWord, 0, 0, 0,
            FrontendFrame.Width, FrontendFrame.Height,
            GameOptionsRomData.MenuTilemapWidth, GameOptionsRomData.MenuTilemapHeight);
        Rgba32[] foreground = SnesBgTilemapRenderer.Render4BppViewport(
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
        SnesLayerCompositor.Composite(background, backgroundLayer);
        SnesLayerCompositor.Composite(background, foreground);

        oam.BeginFrame();
        DrawMenuSpritemap(
            GameOptionsRomData.Spritemaps.OptionModeBorder,
            GameOptionsRomData.Spritemaps.OptionModeBorderX,
            GameOptionsRomData.Spritemaps.OptionModeBorderY);
        (ushort cursorX, ushort cursorY) = CursorPosition();
        DrawMenuSpritemap(GameOptionsRomData.Spritemaps.MissileFrameIds[missileFrame], cursorX, cursorY);
        oam.FinalizeFrame();
        SnesLayerCompositor.Composite(
            background,
            SnesObjRenderer.Render(oam, ppu.Vram, ppu.Cgram, obsel: 0x03));
        ApplyBrightness(background);
        return background;
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
            ReadOnlySpan<ushort> allowed = Input.ControllerBindings.AssignableButtons;
            for (int button = allowed.Length - 1; button >= 0; button--)
            {
                if (((ushort)pressed & allowed[button]) == 0)
                    continue;
                ControllerBindings = ControllerBindings.AssignAndSwap(SelectedItem, allowed[button]);
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
        foreach (GameOptionsLanguagePaletteRegion region in GameOptionsRomData.LanguagePaletteRegions)
        {
            bool selected = JapaneseText == region.HighlightWhenJapanese;
            ReplacePaletteIndex(
                primaryTilemap,
                region.ByteOffset,
                region.ByteCount,
                selected
                    ? GameOptionsRomData.TilePalettes.Selected
                    : GameOptionsRomData.TilePalettes.Unselected);
        }
    }

    private void ApplySpecialPaletteBits()
    {
        ApplySpecialToggle(visibleTilemap, GameOptionsRomData.SpecialToggles.IconCancel, IconCancelEnabled);
        ApplySpecialToggle(visibleTilemap, GameOptionsRomData.SpecialToggles.Moonwalk, MoonwalkEnabled);
    }

    private static void ApplySpecialToggle(
        byte[] tilemap,
        GameOptionsToggleLayout layout,
        bool enabled)
    {
        int enabledPalette = enabled
            ? GameOptionsRomData.TilePalettes.Selected
            : GameOptionsRomData.TilePalettes.Unselected;
        int disabledPalette = enabled
            ? GameOptionsRomData.TilePalettes.Unselected
            : GameOptionsRomData.TilePalettes.Selected;
        ReplacePaletteIndex(tilemap, layout.EnabledTop,
            GameOptionsRomData.SpecialToggles.PaletteRegionByteCount, enabledPalette);
        ReplacePaletteIndex(tilemap, layout.EnabledBottom,
            GameOptionsRomData.SpecialToggles.PaletteRegionByteCount, enabledPalette);
        ReplacePaletteIndex(tilemap, layout.DisabledTop,
            GameOptionsRomData.SpecialToggles.PaletteRegionByteCount, disabledPalette);
        ReplacePaletteIndex(tilemap, layout.DisabledBottom,
            GameOptionsRomData.SpecialToggles.PaletteRegionByteCount, disabledPalette);
    }

    private void ApplyControllerLabels()
    {
        ReadOnlySpan<ushort> sourcePointers = GameOptionsRomData.ControllerLabels.Sources;
        ReadOnlySpan<ushort> destinationOffsets = GameOptionsRomData.ControllerLabels.Destinations;
        for (int action = 0; action < GameOptionsRomData.Rows.ControllerActionCount; action++)
        {
            int button = Input.ControllerBindings.AssignableButtons.IndexOf(ControllerBindings[action]);
            if (button < 0)
                button = 0; // Matches LoadControllerOptionsFromControllerBindings' X fallback.
            int source = GameOptionsRomData.MenuBank | sourcePointers[button];
            int destination = destinationOffsets[action];

            // Each ROM label is a 3x2 tile rectangle. OptionsMenuFunc6 writes its two rows
            // 32 tilemap words apart, not as one contiguous six-word run.
            for (int row = 0; row < GameOptionsRomData.ControllerLabels.HeightInTiles; row++)
            {
                for (int column = 0; column < GameOptionsRomData.ControllerLabels.WidthInTiles; column++)
                {
                    ushort tile = RomDataReader.ReadWordFixedBank(
                        bus,
                        source +
                        (row * GameOptionsRomData.ControllerLabels.WidthInTiles + column) * 2);
                    WriteWord(
                        visibleTilemap,
                        destination +
                        (row * GameOptionsRomData.MenuTilemapWidth + column) * 2,
                        tile);
                }
            }
        }
    }

    private (ushort X, ushort Y) CursorPosition()
    {
        return page switch
        {
            GameOptionsPage.Primary =>
                (GameOptionsRomData.Cursors.PrimaryX,
                    GameOptionsRomData.Cursors.PrimaryY[SelectedItem]),
            GameOptionsPage.Controller =>
                (GameOptionsRomData.Cursors.ControllerX,
                    unchecked((ushort)(
                        GameOptionsRomData.Cursors.ControllerY[SelectedItem] - bg1VerticalScroll))),
            GameOptionsPage.Special =>
                (GameOptionsRomData.Cursors.SpecialX,
                    GameOptionsRomData.Cursors.SpecialY[SelectedItem]),
            _ => throw new InvalidOperationException($"Unknown options page {page}."),
        };
    }

    private byte[] DecompressOptionsPage(GameOptionsPageResource resource)
    {
        byte[] tilemap = RomDataReader.Decompress(
            bus,
            resource.Address,
            maximumOutputBytes: GameOptionsRomData.TilemapByteCount);
        if (tilemap.Length != GameOptionsRomData.TilemapByteCount)
        {
            throw new InvalidDataException(
                $"The {resource.Description} options screen expanded to " +
                $"${tilemap.Length:X} bytes, expected ${GameOptionsRomData.TilemapByteCount:X}.");
        }
        return tilemap;
    }

    private static void ReplacePaletteIndex(
        byte[] tilemap,
        int byteOffset,
        int byteCount,
        int paletteIndex)
    {
        for (int offset = byteOffset; offset < byteOffset + byteCount; offset += 2)
        {
            SnesBgTilemapWord word = ReadWord(tilemap, offset);
            WriteWord(tilemap, offset, word.WithPaletteIndex(paletteIndex).Raw);
        }
    }

    private void DrawMenuSpritemap(ushort id, ushort x, ushort y)
    {
        ushort pointer = RomDataReader.ReadWordFixedBank(
            bus,
            MenuPpuState.SpritemapPointerTableAddress + id * 2);
        oam.AddOnScreenSpritemap(
            bus,
            GameOptionsRomData.MenuBank | pointer,
            x,
            y,
            MenuPpuState.ObjectPaletteBits);
    }

    private void StepMissile()
    {
        if (--missileTimer != 0)
            return;
        missileFrame = (missileFrame + 1) % GameOptionsRomData.Spritemaps.MissileFrameIds.Length;
        missileTimer = GameOptionsRomData.Spritemaps.MissileFrameDuration;
    }

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

    private static ushort ReadWord(ReadOnlySpan<byte> bytes, int offset) =>
        unchecked((ushort)(bytes[offset] | (bytes[offset + 1] << 8)));

    private static void WriteWord(Span<byte> bytes, int offset, ushort value)
    {
        bytes[offset] = unchecked((byte)value);
        bytes[offset + 1] = unchecked((byte)(value >> 8));
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
