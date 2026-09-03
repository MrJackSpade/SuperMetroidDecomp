using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Retail game-over prompt at <c>$81:90AE-$81:93F7</c>.</summary>
public sealed class GameOverMenuState
{
    private static readonly ushort[] MissileSpritemapIds = [0x37, 0x36, 0x35, 0x34];

    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState audio;
    private readonly MenuPpuState ppu;
    private readonly OamBuffer oam = new();
    private readonly ControllerInputState controller = new();
    private readonly ushort[] tilemap = new ushort[32 * 32];
    private int brightness;
    private int missileTimer = 1;
    private int missileFrame;
    private ushort babyInstructionPointer;
    private ushort babyInstructionTimer;
    private ushort babySpritemap = 0x65;

    public GameOverMenuState(ISnesAddressSpace bus, CartridgeAudioState audio)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
        ppu = new MenuPpuState(bus);
        Array.Fill(tilemap, (ushort)0x000f);

        // GameOverMenu_1_Init uses the general bank-$81 command-stream loader. These five
        // pointers are the cartridge's localized text and line breaks, not host strings.
        LoadMenuTilemap(0x0156, 0x92dc); // GAME OVER.
        LoadMenuTilemap(0x038a, 0x9304); // FIND THE METROID LARVA.
        LoadMenuTilemap(0x0414, 0x9334); // TRY AGAIN?
        LoadMenuTilemap(0x04ce, 0x934c); // YES - RETURN TO GAME.
        LoadMenuTilemap(0x05ce, 0x93a0); // NO - GO TO TITLE.
        ppu.Vram.ExecuteWordTransfer(tilemap, MenuPpuState.Bg1TilemapWord, 1);
        Phase = GameOverMenuPhase.Initialize;
    }

    /// <summary>Zero selects Yes; one selects No, matching <c>file_select_map_area_index</c>.</summary>
    public int SelectedItem { get; private set; }

    public GameOverMenuPhase Phase { get; private set; }

    public bool ContinueRequested { get; private set; }

    public bool TitleRequested { get; private set; }

    public void Step(ushort controllerInput)
    {
        controller.Latch(controllerInput);
        SnesButton pressed = (SnesButton)controller.NewlyPressed;
        StepMissileAnimation();

        switch (Phase)
        {
            case GameOverMenuPhase.Initialize:
                audio.QueueMusicDelayed8(0);
                audio.QueueMusicDelayed8(0xff03);
                babyInstructionPointer = 0;
                babyInstructionTimer = 0;
                StepBabyMetroid();
                SelectedItem = 0;
                brightness = 0;
                Phase = GameOverMenuPhase.WaitForInitialMusic;
                break;

            case GameOverMenuPhase.WaitForInitialMusic:
                StepBabyMetroid();
                if (!audio.HasQueuedMusic)
                {
                    audio.QueueMusicDelayed8(4);
                    Phase = GameOverMenuPhase.FadeIn;
                }
                break;

            case GameOverMenuPhase.FadeIn:
                StepBabyMetroid();
                brightness = Math.Min(15, brightness + 1);
                if (brightness == 15)
                    Phase = GameOverMenuPhase.Main;
                break;

            case GameOverMenuPhase.Main:
                StepBabyMetroid();
                if ((pressed & (SnesButton.Select | SnesButton.Up | SnesButton.Down)) != 0)
                {
                    audio.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
                    SelectedItem ^= 1;
                }
                else if ((pressed & SnesButton.A) != 0)
                {
                    // `$81:914B` holds the current Baby frame for 180 ticks once either
                    // answer is accepted, while the selected fade owner continues drawing.
                    babyInstructionTimer = 180;
                    Phase = SelectedItem == 0
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

    public Rgba32[] Render()
    {
        // `TM=$11` enables BG1 and OBJ only. Game-over does not retain the menu starfield.
        Rgba32[] output = SnesLayerCompositor.CreateBackdrop(ppu.Cgram, 256 * 224);
        Rgba32[] foreground = SnesBgTilemapRenderer.Render4BppViewport(
            ppu.Vram, ppu.Cgram, MenuPpuState.Bg1TilemapWord, 0, 0, 0, 256, 224, 32, 32);
        SnesLayerCompositor.Composite(output, foreground);

        oam.BeginFrame();
        DrawMenuSpritemap(babySpritemap, 0x7c, 0x50, paletteBits: 0x0800);
        DrawMenuSpritemap(0x64, 0x7c, 0x50, paletteBits: 0x0a00);
        ushort missileY = SelectedItem == 0 ? (ushort)160 : (ushort)192;
        DrawMenuSpritemap(MissileSpritemapIds[missileFrame], 40, missileY, MenuPpuState.ObjectPaletteBits);
        oam.FinalizeFrame();
        SnesLayerCompositor.Composite(
            output,
            SnesObjRenderer.Render(oam, ppu.Vram, ppu.Cgram, obsel: 0x03));
        MasterBrightnessFilter.Apply(output, (byte)brightness);
        return output;
    }

    /// <summary>Ports <c>HandleGameOverBabyMetroid</c>'s six/eight-byte instruction stream.</summary>
    private void StepBabyMetroid()
    {
        if (babyInstructionTimer == 0)
        {
            babyInstructionPointer = 0xbc27;
            babyInstructionTimer = 10;
        }

        babyInstructionTimer = unchecked((ushort)(babyInstructionTimer - 1));
        if (babyInstructionTimer == 0)
            AdvanceBabyInstruction();
        LoadCurrentBabyFrame();
    }

    private void AdvanceBabyInstruction()
    {
        ushort next = ReadBank82Word(unchecked((ushort)(babyInstructionPointer + 6)));
        if (next == 0xffff)
        {
            babyInstructionPointer = 0xbc27;
            babyInstructionTimer = 10;
            return;
        }

        if ((next & 0x8000) != 0)
        {
            byte cry = next switch
            {
                0xbc0c => 0x23,
                0xbc15 => 0x26,
                0xbc1e => 0x27,
                _ => throw new InvalidDataException(
                    $"Unknown game-over Baby instruction $82:{next:X4}."),
            };
            audio.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, cry), maximumQueued: 6);
            babyInstructionPointer = unchecked((ushort)(babyInstructionPointer + 8));
            babyInstructionTimer = ReadBank82Word(babyInstructionPointer);
        }
        else
        {
            babyInstructionPointer = unchecked((ushort)(babyInstructionPointer + 6));
            babyInstructionTimer = next;
        }
    }

    private void LoadCurrentBabyFrame()
    {
        babySpritemap = ReadBank82Word(unchecked((ushort)(babyInstructionPointer + 2)));
        ushort palettePointer = ReadBank82Word(unchecked((ushort)(babyInstructionPointer + 4)));
        for (int color = 0; color < 16; color++)
        {
            ppu.Cgram.SetColor(
                0xc0 + color,
                ReadBank82Word(unchecked((ushort)(palettePointer + color * 2))));
        }
    }

    private void LoadMenuTilemap(int destinationByteOffset, ushort sourcePointer)
    {
        int initialColumn = destinationByteOffset;
        int sourceAddress = 0x810000 | sourcePointer;
        while (true)
        {
            ushort word = RomDataReader.ReadWordFixedBank(bus, sourceAddress);
            sourceAddress = 0x810000 | ((sourceAddress + 2) & 0xffff);
            if (word == 0xffff)
                return;
            if (word == 0xfffe)
            {
                initialColumn += 64;
                destinationByteOffset = initialColumn;
                continue;
            }

            int wordIndex = destinationByteOffset >> 1;
            if ((uint)wordIndex >= tilemap.Length)
                throw new InvalidDataException("A game-over text stream escaped BG1.");
            tilemap[wordIndex] = word;
            destinationByteOffset += 2;
        }
    }

    private void DrawMenuSpritemap(
        ushort spritemapId,
        ushort x,
        ushort y,
        ushort paletteBits)
    {
        ushort pointer = RomDataReader.ReadWordFixedBank(
            bus,
            MenuPpuState.SpritemapPointerTableAddress + spritemapId * 2);
        oam.AddOnScreenSpritemap(bus, 0x820000 | pointer, x, y, paletteBits);
    }

    private ushort ReadBank82Word(ushort pointer) =>
        RomDataReader.ReadWordFixedBank(bus, 0x820000 | pointer);

    private void StepMissileAnimation()
    {
        if (--missileTimer != 0)
            return;
        missileFrame = (missileFrame + 1) & 3;
        missileTimer = 8;
    }
}

public enum GameOverMenuPhase
{
    Initialize,
    WaitForInitialMusic,
    FadeIn,
    Main,
    FadeOutToContinue,
    FadeOutToTitle,
}
