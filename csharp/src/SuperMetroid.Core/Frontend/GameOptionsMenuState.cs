using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Fresh-game route through game state $02's primary options screen.</summary>
public sealed class GameOptionsMenuState
{
    private static readonly ushort[] SelectionY = [0x38, 0x58, 0x70, 0x90, 0xb0];
    private static readonly ushort[] MissileSpritemapIds = [0x37, 0x36, 0x35, 0x34];

    private readonly ISnesAddressSpace bus;
    private readonly MenuPpuState ppu;
    private readonly OamBuffer oam = new();
    private readonly ControllerInputState controller = new();
    private readonly byte[] optionsTilemap;
    private int brightness;
    private int missileTimer = 1;
    private int missileFrame;

    public GameOptionsMenuState(ISnesAddressSpace bus)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        ppu = new MenuPpuState(bus);

        // `$82:EC77` expands five screens into adjacent WRAM pages. The primary options
        // page at `$97:8DF4` is exactly one $800-byte BG1 tilemap; retain the other source
        // addresses for their own controller/special-settings states rather than eagerly
        // decoding data that this fresh-game path has not entered.
        optionsTilemap = RomDataReader.Decompress(bus, 0x978df4, maximumOutputBytes: 0x0800);
        if (optionsTilemap.Length != 0x0800)
            throw new InvalidDataException("The primary options screen did not expand to one $800-byte tilemap.");
        ApplyLanguagePaletteBits();
        ppu.LoadBg1(optionsTilemap);
        Phase = GameOptionsPhase.FadeIn;
    }

    public int SelectedItem { get; private set; }

    public bool JapaneseText { get; private set; }

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
                brightness = Math.Min(15, brightness + 1);
                if (brightness == 15)
                    Phase = GameOptionsPhase.Main;
                break;

            case GameOptionsPhase.Main:
                if ((pressed & SnesButton.Up) != 0)
                    SelectedItem = SelectedItem == 0 ? 4 : SelectedItem - 1;
                else if ((pressed & SnesButton.Down) != 0)
                    SelectedItem = SelectedItem == 4 ? 0 : SelectedItem + 1;

                if ((pressed & SnesButton.B) != 0)
                {
                    Phase = GameOptionsPhase.FadeOutToFileSelect;
                }
                else if ((pressed & (SnesButton.Start | SnesButton.A)) != 0)
                {
                    switch (SelectedItem)
                    {
                        case 0:
                            // Fresh save leaves `loading_game_state` zero, so `$82:EEB4`
                            // selects game state $1E after the options fade reaches black.
                            Phase = GameOptionsPhase.FadeOutToIntro;
                            break;
                        case 1:
                        case 2:
                            // Both language rows call the same native toggle function, then
                            // reset the cursor to Start Game and recolor English/Japanese rows.
                            JapaneseText = !JapaneseText;
                            SelectedItem = 0;
                            ApplyLanguagePaletteBits();
                            ppu.LoadBg1(optionsTilemap);
                            break;
                        case 3:
                        case 4:
                            // Controller and special-setting submenus are not prerequisites
                            // for entering the intro. Stop on their named boundary instead of
                            // pretending a click did nothing or applying invented settings.
                            Phase = GameOptionsPhase.SubmenuTranslationBoundary;
                            break;
                    }
                }
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

            case GameOptionsPhase.SubmenuTranslationBoundary:
                // B is a useful debugger escape while those two secondary state machines
                // are still pending; it returns to the authentic primary options screen.
                if ((pressed & SnesButton.B) != 0)
                    Phase = GameOptionsPhase.Main;
                break;
        }
    }

    public Rgba32[] Render()
    {
        Rgba32[] background = SnesLayerCompositor.CreateBackdrop(ppu.Cgram, 256 * 224);
        Rgba32[] backgroundLayer = SnesBgTilemapRenderer.Render4BppViewport(
            ppu.Vram, ppu.Cgram, MenuPpuState.Bg2TilemapWord, 0, 0, 0, 256, 224, 32, 32);
        Rgba32[] foreground = SnesBgTilemapRenderer.Render4BppViewport(
            ppu.Vram, ppu.Cgram, MenuPpuState.Bg1TilemapWord, 0, 0, 0, 256, 224, 32, 32);
        SnesLayerCompositor.Composite(background, backgroundLayer);
        SnesLayerCompositor.Composite(background, foreground);

        oam.BeginFrame();
        DrawMenuSpritemap(0x4b, 0x7c, 0x10); // OPTION MODE border setup at `$82:F34B`.
        DrawMenuSpritemap(MissileSpritemapIds[missileFrame], 0x18, SelectionY[SelectedItem]);
        oam.FinalizeFrame();
        SnesLayerCompositor.Composite(
            background,
            SnesObjRenderer.Render(oam, ppu.Vram, ppu.Cgram, obsel: 0x03));
        ApplyBrightness(background);
        return background;
    }

    private void ApplyLanguagePaletteBits()
    {
        ReplacePaletteBits(0x288, 0x18, JapaneseText ? (ushort)0x0400 : (ushort)0);
        ReplacePaletteBits(0x2c8, 0x18, JapaneseText ? (ushort)0x0400 : (ushort)0);
        ReplacePaletteBits(0x348, 0x32, JapaneseText ? (ushort)0 : (ushort)0x0400);
        ReplacePaletteBits(0x388, 0x32, JapaneseText ? (ushort)0 : (ushort)0x0400);
    }

    private void ReplacePaletteBits(int byteOffset, int byteCount, ushort paletteBits)
    {
        for (int offset = byteOffset; offset < byteOffset + byteCount; offset += 2)
        {
            ushort word = (ushort)(optionsTilemap[offset] | (optionsTilemap[offset + 1] << 8));
            word = (ushort)(paletteBits | (word & 0xe3ff));
            optionsTilemap[offset] = (byte)word;
            optionsTilemap[offset + 1] = (byte)(word >> 8);
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

    private void ApplyBrightness(Span<Rgba32> pixels)
    {
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            Rgba32 color = pixels[pixel];
            if (brightness <= 0)
                pixels[pixel] = new Rgba32(0, 0, 0, color.A);
            else if (brightness < 15)
                pixels[pixel] = new Rgba32(
                    (byte)(color.R * brightness / 15),
                    (byte)(color.G * brightness / 15),
                    (byte)(color.B * brightness / 15),
                    color.A);
        }
    }
}

public enum GameOptionsPhase
{
    FadeIn,
    Main,
    FadeOutToIntro,
    FadeOutToFileSelect,
    SubmenuTranslationBoundary,
}
