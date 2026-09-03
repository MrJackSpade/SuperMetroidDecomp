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
    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState audio;
    private readonly MenuPpuState ppu;
    private readonly OamBuffer oam = new();
    private readonly ControllerInputState controller = new();
    private readonly ushort[] tilemap =
        new ushort[GameOverRomData.TilemapWidth * GameOverRomData.TilemapHeight];
    private int brightness;
    private int missileTimer = 1;
    private int missileFrame;
    private ushort babyInstructionPointer;
    private ushort babyInstructionTimer;
    private ushort babySpritemap = GameOverRomData.BabyAnimation.InitialSpritemap;

    public GameOverMenuState(ISnesAddressSpace bus, CartridgeAudioState audio)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
        ppu = new MenuPpuState(bus);
        Array.Fill(tilemap, GameOverRomData.BlankTile.Raw);

        // GameOverMenu_1_Init uses the general bank-$81 command-stream loader. These five
        // pointers are the cartridge's localized text and line breaks, not host strings.
        foreach (GameOverTextStream stream in GameOverRomData.Text.All)
            LoadMenuTilemap(stream);
        ppu.Vram.ExecuteWordTransfer(tilemap, MenuPpuState.Bg1TilemapWord, 1);
        Phase = GameOverMenuPhase.Initialize;
    }

    /// <summary>Zero selects Yes; one selects No, matching <c>file_select_map_area_index</c>.</summary>
    public int SelectedItem { get; private set; }

    public GameOverMenuPhase Phase { get; private set; }

    public bool ContinueRequested { get; private set; }

    public bool TitleRequested { get; private set; }

    /// <summary>Currently displayed Baby Metroid frame, exposed for deterministic replay.</summary>
    public ushort BabySpritemap => babySpritemap;

    /// <summary>Current bank-$82 Baby instruction pointer, exposed for debugger inspection.</summary>
    public ushort BabyInstructionPointer => babyInstructionPointer;

    public void Step(ushort controllerInput)
    {
        controller.Latch(controllerInput);
        SnesButton pressed = controller.NewlyPressedButtons;
        StepMissileAnimation();

        switch (Phase)
        {
            case GameOverMenuPhase.Initialize:
                audio.QueueMusicDelayed8(MusicCommand.Stop);
                audio.QueueMusicDelayed8(MusicCommand.LoadData(GameOverRomData.Music.DataIndex));
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
                    audio.QueueMusicDelayed8(
                        MusicCommand.SelectTrack(GameOverRomData.Music.TrackIndex));
                    Phase = GameOverMenuPhase.FadeIn;
                }
                break;

            case GameOverMenuPhase.FadeIn:
                StepBabyMetroid();
                brightness = Math.Min(GameOverRomData.MaximumBrightness, brightness + 1);
                if (brightness == GameOverRomData.MaximumBrightness)
                    Phase = GameOverMenuPhase.Main;
                break;

            case GameOverMenuPhase.Main:
                StepBabyMetroid();
                if ((pressed & (SnesButton.Select | SnesButton.Up | SnesButton.Down)) != 0)
                {
                    audio.QueueSound(
                        SoundEffectLibrary1Sounds.MenuCursor,
                        maximumQueued: GameOverRomData.MaximumQueuedSounds);
                    SelectedItem ^= 1;
                }
                else if ((pressed & SnesButton.A) != 0)
                {
                    // `$81:914B` holds the current Baby frame for 180 ticks once either
                    // answer is accepted, while the selected fade owner continues drawing.
                    babyInstructionTimer =
                        GameOverRomData.BabyAnimation.AcceptedAnswerHoldDuration;
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
        Rgba32[] output = SnesLayerCompositor.CreateBackdrop(
            ppu.Cgram,
            FrontendFrame.Width * FrontendFrame.Height);
        Rgba32[] foreground = SnesBgTilemapRenderer.Render4BppViewport(
            ppu.Vram, ppu.Cgram, MenuPpuState.Bg1TilemapWord, 0, 0, 0,
            FrontendFrame.Width, FrontendFrame.Height,
            GameOverRomData.TilemapWidth, GameOverRomData.TilemapHeight);
        SnesLayerCompositor.Composite(output, foreground);

        oam.BeginFrame();
        DrawMenuSpritemap(
            babySpritemap,
            GameOverRomData.Sprites.BabyX,
            GameOverRomData.Sprites.BabyY,
            GameOverRomData.Sprites.BabyPalette.Raw);
        DrawMenuSpritemap(
            GameOverRomData.Sprites.EggSpritemap,
            GameOverRomData.Sprites.BabyX,
            GameOverRomData.Sprites.BabyY,
            GameOverRomData.Sprites.EggPalette.Raw);
        ushort missileY = SelectedItem == 0
            ? GameOverRomData.Sprites.YesMissileY
            : GameOverRomData.Sprites.NoMissileY;
        DrawMenuSpritemap(
            GameOverRomData.Sprites.MissileFrameIds[missileFrame],
            GameOverRomData.Sprites.MissileX,
            missileY,
            MenuPpuState.ObjectPaletteBits);
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
            babyInstructionPointer = GameOverRomData.BabyAnimation.FirstInstruction;
            babyInstructionTimer = GameOverRomData.BabyAnimation.InitialFrameDuration;
        }

        babyInstructionTimer = unchecked((ushort)(babyInstructionTimer - 1));
        if (babyInstructionTimer == 0)
            AdvanceBabyInstruction();
        LoadCurrentBabyFrame();
    }

    private void AdvanceBabyInstruction()
    {
        ushort next = ReadBank82Word(unchecked((ushort)(
            babyInstructionPointer + GameOverRomData.BabyAnimation.NextInstructionOffset)));
        if (next == GameOverRomData.BabyAnimation.End)
        {
            babyInstructionPointer = GameOverRomData.BabyAnimation.FirstInstruction;
            babyInstructionTimer = GameOverRomData.BabyAnimation.InitialFrameDuration;
            return;
        }

        if ((next & 0x8000) != 0)
        {
            audio.QueueSound(
                GameOverRomData.BabyAnimation.ResolveCry(next),
                maximumQueued: GameOverRomData.MaximumQueuedSounds);
            babyInstructionPointer = unchecked((ushort)(
                babyInstructionPointer + GameOverRomData.BabyAnimation.SoundInstructionByteCount));
            babyInstructionTimer = ReadBank82Word(babyInstructionPointer);
        }
        else
        {
            babyInstructionPointer = unchecked((ushort)(
                babyInstructionPointer + GameOverRomData.BabyAnimation.FrameByteCount));
            babyInstructionTimer = next;
        }
    }

    private void LoadCurrentBabyFrame()
    {
        babySpritemap = ReadBank82Word(unchecked((ushort)(
            babyInstructionPointer + GameOverRomData.BabyAnimation.SpritemapOffset)));
        ushort palettePointer = ReadBank82Word(unchecked((ushort)(
            babyInstructionPointer + GameOverRomData.BabyAnimation.PalettePointerOffset)));
        for (int color = 0; color < GameOverRomData.BabyAnimation.PaletteColorCount; color++)
        {
            ppu.Cgram.SetColor(
                GameOverRomData.BabyAnimation.PaletteDestinationIndex + color,
                ReadBank82Word(unchecked((ushort)(palettePointer + color * 2))));
        }
    }

    private void LoadMenuTilemap(GameOverTextStream stream)
    {
        int destinationByteOffset = stream.DestinationByteOffset;
        int initialColumn = destinationByteOffset;
        int sourceAddress = GameOverRomData.TextBank | stream.SourcePointer;
        while (true)
        {
            ushort word = RomDataReader.ReadWordFixedBank(bus, sourceAddress);
            sourceAddress = GameOverRomData.TextBank | ((sourceAddress + 2) & 0xffff);
            if (word == GameOverRomData.TextEnd)
                return;
            if (word == GameOverRomData.TextNextLine)
            {
                initialColumn += GameOverRomData.TilemapRowByteCount;
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
        oam.AddOnScreenSpritemap(bus, GameOverRomData.SpriteBank | pointer, x, y, paletteBits);
    }

    private ushort ReadBank82Word(ushort pointer) =>
        RomDataReader.ReadWordFixedBank(bus, GameOverRomData.SpriteBank | pointer);

    private void StepMissileAnimation()
    {
        if (--missileTimer != 0)
            return;
        missileFrame = (missileFrame + 1) % GameOverRomData.Sprites.MissileFrameIds.Length;
        missileTimer = GameOverRomData.Sprites.MissileFrameDuration;
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
