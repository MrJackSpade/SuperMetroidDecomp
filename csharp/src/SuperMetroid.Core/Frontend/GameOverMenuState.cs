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
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;

    public GameOverMenuState(
        ISnesAddressSpace bus,
        CartridgeAudioState audio,
        AreaMapPresentationCatalog? mapPresentation = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
        this.mapPresentation = mapPresentation;
        ppu = mapPresentation is null
            ? new MenuPpuState(bus)
            : new MenuPpuState(bus, mapPresentation.Tiles, mapPresentation.Palettes,
                mapPresentation.WorldArtwork, mapPresentation.Sprites,
                loadInitialBackground: false);
        if (mapPresentation is null)
        {
            Array.Fill(tilemap, GameOverRomData.BlankTile.Raw);

            // GameOverMenu_1_Init uses the general bank-$81 command-stream loader. These five
            // pointers are the cartridge's localized text and line breaks, not host strings.
            foreach (GameOverTextStream stream in GameOverRomData.Text.All)
                LoadMenuTilemap(stream);
            ppu.Vram.ExecuteWordTransfer(tilemap, MenuPpuState.Bg1TilemapWord, 1);
        }
        else
        {
            mapPresentation.GameOver.LoadTilemapTo(ppu.Vram, MenuPpuState.Bg1TilemapWord);
        }
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

    /// <summary>
    /// Reattaches host-owned presentation after debugger restoration and refreshes only
    /// presentation-owned PPU state. Animation phase, answer selection and fade remain live.
    /// </summary>
    public void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        string? previousIdentity = mapPresentation?.ContentIdentity;
        mapPresentation = catalog;
        if (catalog is null || string.Equals(
                previousIdentity, catalog.ContentIdentity, StringComparison.Ordinal))
            return;

        ppu.BindMapTiles(bus, catalog.Tiles);
        ppu.BindMapSprites(bus, catalog.Sprites);
        ppu.BindMapPalettes(bus, catalog.Palettes);
        catalog.GameOver.LoadTilemapTo(ppu.Vram, MenuPpuState.Bg1TilemapWord);
        if (babyInstructionPointer != 0)
        {
            GameOverBabyInstruction instruction =
                GameOverBabyAnimationDefinitions.Get(babyInstructionPointer);
            catalog.GameOver.ApplyBabyPalette(ppu.Cgram, instruction.Palette);
        }
    }

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

        PrepareRenderOam();
        SnesLayerCompositor.Composite(output,
            SnesObjRenderer.Render(oam, ppu.Vram, ppu.Cgram, obsel: MenuRenderDefinitions.ObjectSelection));
        MasterBrightnessFilter.Apply(output, (byte)brightness);
        return output;
    }

    /// <summary>Captures BG1/OBJ only; the game-over PPU does not enable BG2.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderOam();
        RenderLayer[] layers = [new Bg4BppRenderLayer(MenuPpuState.Bg1TilemapWord, 0,
            0, 0, GameOverRomData.TilemapWidth, GameOverRomData.TilemapHeight, null), new ObjRenderLayer()];
        return new(PpuMemorySnapshot.Capture(ppu.Vram, ppu.Cgram, oam), layers,
            MenuRenderDefinitions.ObjectSelection, checked((byte)brightness));
    }

    private void PrepareRenderOam()
    {
        oam.BeginFrame();
        if (mapPresentation is not null)
        {
            GameOverBabyInstruction instruction = babyInstructionPointer == 0
                ? GameOverBabyAnimationDefinitions.Get(GameOverBabyAnimationDefinitions.FirstPointer)
                : GameOverBabyAnimationDefinitions.Get(babyInstructionPointer);
            mapPresentation.GameOver.DrawBaby(oam, instruction.Frame);
            mapPresentation.GameOver.DrawEgg(oam);
            mapPresentation.GameOver.DrawCursor(oam, missileFrame, SelectedItem != 0);
            oam.FinalizeFrame();
            return;
        }
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
    }

    /// <summary>Ports <c>HandleGameOverBabyMetroid</c>'s six/eight-byte instruction stream.</summary>
    private void StepBabyMetroid()
    {
        if (mapPresentation is not null)
        {
            StepCompiledBabyMetroid();
            return;
        }
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

    private void StepCompiledBabyMetroid()
    {
        if (babyInstructionTimer == 0)
        {
            babyInstructionPointer = GameOverBabyAnimationDefinitions.FirstPointer;
            babyInstructionTimer =
                GameOverBabyAnimationDefinitions.Get(babyInstructionPointer).Duration;
        }

        babyInstructionTimer = unchecked((ushort)(babyInstructionTimer - 1));
        if (babyInstructionTimer == 0)
        {
            GameOverBabyInstruction completed =
                GameOverBabyAnimationDefinitions.Get(babyInstructionPointer);
            if (completed.SoundAfter != GameOverBabySound.None)
                QueueCompiledBabySound(completed.SoundAfter);
            babyInstructionPointer = completed.NextPointer;
            babyInstructionTimer =
                GameOverBabyAnimationDefinitions.Get(completed.NextPointer).Duration;
        }

        GameOverBabyInstruction current =
            GameOverBabyAnimationDefinitions.Get(babyInstructionPointer);
        babySpritemap = GameOverBabyAnimationDefinitions.NativeSpritemap(current.Frame);
        mapPresentation!.GameOver.ApplyBabyPalette(ppu.Cgram, current.Palette);
    }

    private void QueueCompiledBabySound(GameOverBabySound sound)
    {
        SoundEffectId effect = sound switch
        {
            GameOverBabySound.Cry23 => GameOverRomData.BabyAnimation.Cry23,
            GameOverBabySound.Cry26 => GameOverRomData.BabyAnimation.Cry26,
            GameOverBabySound.Cry27 => GameOverRomData.BabyAnimation.Cry27,
            _ => throw new InvalidDataException($"Unknown compiled game-over Baby sound {sound}."),
        };
        audio.QueueSound(effect, maximumQueued: GameOverRomData.MaximumQueuedSounds);
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
        missileTimer = mapPresentation?.GameOver.CursorFrameDuration ??
            GameOverRomData.Sprites.MissileFrameDuration;
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
