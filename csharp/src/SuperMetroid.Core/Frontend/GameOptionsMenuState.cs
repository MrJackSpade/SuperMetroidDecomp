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
    private const int TilemapByteCount = 0x0800;
    private const int MaximumBrightness = 15;
    private const int ControllerScrollLimit = 32;

    // `$82:F307`, `$82:F31B`, and `$82:F33F` are the three cursor-coordinate tables used
    // by OptionsPreInstr_F2A9. Coordinates are screen pixels before BG1's controller-page
    // scroll is applied.
    private static readonly ushort[] PrimarySelectionY = [0x38, 0x58, 0x70, 0x90, 0xb0];
    private static readonly ushort[] ControllerSelectionY =
        [0x30, 0x48, 0x60, 0x78, 0x90, 0xa8, 0xc0, 0xb8, 0xd0];
    private static readonly ushort[] SpecialSelectionY = [0x40, 0x70, 0xa0];
    private static readonly ushort[] MissileSpritemapIds = [0x37, 0x36, 0x35, 0x34];

    // OptionsMenuFunc6 copies each selected physical-button label into one 3x2-tile box.
    // These are literal byte offsets and bank-$82 pointers from `$82:F639/$82:F647`.
    private static readonly ushort[] ControllerLabelOffsets =
        [0x016e, 0x022e, 0x02ee, 0x03ae, 0x046e, 0x052e, 0x05ee];
    private static readonly ushort[] ControllerLabelPointers =
        [0xf659, 0xf665, 0xf671, 0xf67d, 0xf689, 0xf695, 0xf6a1];

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
        primaryTilemap = DecompressOptionsPage(0x978df4, "primary");
        controllerEnglishTilemap = DecompressOptionsPage(0x978fcd, "English controller");
        controllerJapaneseTilemap = DecompressOptionsPage(0x9791c4, "Japanese controller");
        specialEnglishTilemap = DecompressOptionsPage(0x97938d, "English special-settings");
        specialJapaneseTilemap = DecompressOptionsPage(0x97953a, "Japanese special-settings");

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
                brightness = Math.Min(MaximumBrightness, brightness + 1);
                if (brightness == MaximumBrightness)
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
                brightness = Math.Min(MaximumBrightness, brightness + 1);
                if (brightness == MaximumBrightness)
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
                bg1VerticalScroll += 2;
                if (bg1VerticalScroll == ControllerScrollLimit)
                    Phase = GameOptionsPhase.ControllerSettings;
                break;

            case GameOptionsPhase.ScrollControllerUp:
                bg1VerticalScroll -= 2;
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
        Rgba32[] background = SnesLayerCompositor.CreateBackdrop(ppu.Cgram, 256 * 224);
        Rgba32[] backgroundLayer = SnesBgTilemapRenderer.Render4BppViewport(
            ppu.Vram, ppu.Cgram, MenuPpuState.Bg2TilemapWord, 0, 0, 0, 256, 224, 32, 32);
        Rgba32[] foreground = SnesBgTilemapRenderer.Render4BppViewport(
            ppu.Vram,
            ppu.Cgram,
            MenuPpuState.Bg1TilemapWord,
            0,
            0,
            unchecked((ushort)bg1VerticalScroll),
            256,
            224,
            32,
            32);
        SnesLayerCompositor.Composite(background, backgroundLayer);
        SnesLayerCompositor.Composite(background, foreground);

        oam.BeginFrame();
        DrawMenuSpritemap(0x4b, 0x7c, 0x10); // OPTION MODE border at `$82:F34B`.
        (ushort cursorX, ushort cursorY) = CursorPosition();
        DrawMenuSpritemap(MissileSpritemapIds[missileFrame], cursorX, cursorY);
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
            SelectedItem = SelectedItem == 0 ? 4 : SelectedItem - 1;
            QueueMoveSound();
        }
        else if ((pressed & SnesButton.Down) != 0)
        {
            SelectedItem = SelectedItem == 4 ? 0 : SelectedItem + 1;
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
            case 0:
                Phase = GameOptionsPhase.FadeOutToIntro;
                break;
            case 1:
            case 2:
                JapaneseText = !JapaneseText;
                SelectedItem = 0;
                ApplyLanguagePaletteBits();
                LoadVisiblePage();
                break;
            case 3:
                BeginDissolveTo(GameOptionsPage.Controller);
                break;
            case 4:
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
                SelectedItem = 8;
                Phase = GameOptionsPhase.ScrollControllerDown;
            }
            else if (SelectedItem == 6)
            {
                Phase = GameOptionsPhase.ScrollControllerUp;
            }
            return;
        }
        if ((pressed & SnesButton.Down) != 0)
        {
            QueueMoveSound();
            SelectedItem++;
            if (SelectedItem == 7)
            {
                Phase = GameOptionsPhase.ScrollControllerDown;
            }
            else if (SelectedItem == 9)
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
        if (SelectedItem < 7)
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
        if (SelectedItem == 7)
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
            SelectedItem = SelectedItem == 0 ? 2 : SelectedItem - 1;
            QueueMoveSound();
        }
        else if ((pressed & SnesButton.Down) != 0)
        {
            SelectedItem = SelectedItem == 2 ? 0 : SelectedItem + 1;
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
        if (SelectedItem == 0)
            IconCancelEnabled = !IconCancelEnabled;
        else if (SelectedItem == 1)
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
        ReplacePaletteBits(primaryTilemap, 0x288, 0x18, JapaneseText ? (ushort)0x0400 : (ushort)0);
        ReplacePaletteBits(primaryTilemap, 0x2c8, 0x18, JapaneseText ? (ushort)0x0400 : (ushort)0);
        ReplacePaletteBits(primaryTilemap, 0x348, 0x32, JapaneseText ? (ushort)0 : (ushort)0x0400);
        ReplacePaletteBits(primaryTilemap, 0x388, 0x32, JapaneseText ? (ushort)0 : (ushort)0x0400);
    }

    private void ApplySpecialPaletteBits()
    {
        ApplySpecialToggle(visibleTilemap, 0x1e0, 0x220, 0x1ee, 0x22e, IconCancelEnabled);
        ApplySpecialToggle(visibleTilemap, 0x360, 0x3a0, 0x36e, 0x3ae, MoonwalkEnabled);
    }

    private static void ApplySpecialToggle(
        byte[] tilemap,
        int enabledTop,
        int enabledBottom,
        int disabledTop,
        int disabledBottom,
        bool enabled)
    {
        ReplacePaletteBits(tilemap, enabledTop, 0x0c, enabled ? (ushort)0 : (ushort)0x0400);
        ReplacePaletteBits(tilemap, enabledBottom, 0x0c, enabled ? (ushort)0 : (ushort)0x0400);
        ReplacePaletteBits(tilemap, disabledTop, 0x0c, enabled ? (ushort)0x0400 : (ushort)0);
        ReplacePaletteBits(tilemap, disabledBottom, 0x0c, enabled ? (ushort)0x0400 : (ushort)0);
    }

    private void ApplyControllerLabels()
    {
        for (int action = 0; action < 7; action++)
        {
            int button = Input.ControllerBindings.AssignableButtons.IndexOf(ControllerBindings[action]);
            if (button < 0)
                button = 0; // Matches LoadControllerOptionsFromControllerBindings' X fallback.
            int source = 0x820000 | ControllerLabelPointers[button];
            int destination = ControllerLabelOffsets[action];

            // Each ROM label is a 3x2 tile rectangle. OptionsMenuFunc6 writes its two rows
            // 32 tilemap words apart, not as one contiguous six-word run.
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    ushort tile = RomDataReader.ReadWordFixedBank(
                        bus,
                        source + (row * 3 + column) * 2);
                    WriteWord(visibleTilemap, destination + (row * 32 + column) * 2, tile);
                }
            }
        }
    }

    private (ushort X, ushort Y) CursorPosition()
    {
        return page switch
        {
            GameOptionsPage.Primary => (0x18, PrimarySelectionY[SelectedItem]),
            GameOptionsPage.Controller =>
                (0x28, unchecked((ushort)(ControllerSelectionY[SelectedItem] - bg1VerticalScroll))),
            GameOptionsPage.Special => (0x10, SpecialSelectionY[SelectedItem]),
            _ => throw new InvalidOperationException($"Unknown options page {page}."),
        };
    }

    private byte[] DecompressOptionsPage(int address, string name)
    {
        byte[] tilemap = RomDataReader.Decompress(bus, address, maximumOutputBytes: TilemapByteCount);
        if (tilemap.Length != TilemapByteCount)
        {
            throw new InvalidDataException(
                $"The {name} options screen expanded to ${tilemap.Length:X} bytes, expected $800.");
        }
        return tilemap;
    }

    private static void ReplacePaletteBits(
        byte[] tilemap,
        int byteOffset,
        int byteCount,
        ushort paletteBits)
    {
        for (int offset = byteOffset; offset < byteOffset + byteCount; offset += 2)
        {
            ushort word = ReadWord(tilemap, offset);
            WriteWord(tilemap, offset, unchecked((ushort)(paletteBits | (word & 0xe3ff))));
        }
    }

    private void DrawMenuSpritemap(ushort id, ushort x, ushort y)
    {
        ushort pointer = RomDataReader.ReadWordFixedBank(
            bus,
            MenuPpuState.SpritemapPointerTableAddress + id * 2);
        oam.AddOnScreenSpritemap(bus, 0x820000 | pointer, x, y, MenuPpuState.ObjectPaletteBits);
    }

    private void StepMissile()
    {
        if (--missileTimer != 0)
            return;
        missileFrame = (missileFrame + 1) & 3;
        missileTimer = 8;
    }

    private void QueueMoveSound() =>
        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);

    private void QueueSelectSound() =>
        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);

    private void ApplyBrightness(Span<Rgba32> pixels)
    {
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            Rgba32 color = pixels[pixel];
            if (brightness <= 0)
                pixels[pixel] = new Rgba32(0, 0, 0, color.A);
            else if (brightness < MaximumBrightness)
            {
                pixels[pixel] = new Rgba32(
                    (byte)(color.R * brightness / MaximumBrightness),
                    (byte)(color.G * brightness / MaximumBrightness),
                    (byte)(color.B * brightness / MaximumBrightness),
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
