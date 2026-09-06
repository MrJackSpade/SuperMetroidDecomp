using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Cartridge-backed title-sequence state owned by game state one.
/// </summary>
/// <remarks>
/// This ports the visible state chain at <c>$8B:9A22-$8B:A35A</c>. Art, palette,
/// spritemaps, and the animated baby-Metroid character data are read from the ROM. Host
/// code supplies only the controller word and consumes the composed framebuffer.
/// </remarks>
public sealed class TitleSequenceState
{
    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
    private readonly SnesVram vram = new();
    private readonly SnesCgram cgram = new();
    private RoomPaletteFxSystem consolePaletteFx = new();
    private readonly OamBuffer oam = new();
    private readonly ControllerInputState controller = new();
    private readonly byte[] babyMetroidCharacters;

    private TitleSequencePhase phase;
    private int phaseTimer;
    private int sequenceEntry;
    private int sequenceEntryTimer;
    private ushort activeSpritemap;
    private ushort activeOriginX;
    private ushort activeOriginY;
    private ushort activeCharacterOffset;
    private int mode7X;
    private int mode7Y;
    private int mode7XSubposition;
    private int mode7YSubposition;
    private int zoom;
    private int brightness;
    private int babyFrame;
    private int babyFrameTimer;
    private bool mode7BackgroundEnabled;
    private bool fadingToDemo;

    /// <summary>Creates the native initial title setup performed by <c>$8B:9B68</c>.</summary>
    public TitleSequenceState(ISnesAddressSpace bus, CartridgeAudioState? audio = null)
        : this(bus, audio, queueOpeningMusic: true)
    {
    }

    private TitleSequenceState(ISnesAddressSpace bus, CartridgeAudioState? audio, bool queueOpeningMusic)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio;
        if (queueOpeningMusic)
        {
            audio?.QueueMusicDelayed8(
                MusicCommand.LoadData(TitleSequenceRomData.Music.DataIndex));
            audio?.QueueMusicDelayed8(
                MusicCommand.SelectTrack(TitleSequenceRomData.Music.OpeningTrack));
        }

        // `$8B:9B87` expands these four independent streams to bank-$7F. Recreate the
        // subsequent DMA destinations rather than keeping an invented host texture format.
        byte[] mode7Characters = RomDataReader.Decompress(
            bus,
            TitleSequenceRomData.Assets.Mode7CharactersAddress);
        byte[] mode7Map = RomDataReader.Decompress(bus, TitleSequenceRomData.Assets.Mode7MapAddress);
        byte[] objectCharacters = RomDataReader.Decompress(
            bus,
            TitleSequenceRomData.Assets.ObjectCharactersAddress);
        babyMetroidCharacters = RomDataReader.Decompress(
            bus,
            TitleSequenceRomData.Assets.BabyMetroidCharactersAddress);

        LoadMode7InterleavedVram(mode7Characters, mode7Map);
        vram.LoadBytes(
            TitleSequenceRomData.Vram.ObjectCharacterDestinationByte,
            objectCharacters.AsSpan(
                0,
                Math.Min(
                    TitleSequenceRomData.Vram.ObjectCharacterByteCount,
                    objectCharacters.Length)));
        cgram.LoadFromBus(bus, TitleSequenceRomData.Assets.PaletteAddress);
        ResetConsolePaletteFx();

        // The first object is definition `$A0EF`: 1994 text at (129,112), character
        // offset `$0400`, instruction list `$A03D`. Its pre-instruction forces full
        // brightness on the first processing frame.
        brightness = TitleSequenceRomData.Timing.MaximumBrightness;
        BeginTextSequence(TitleSequenceRomData.TextSequences.Year);
        UpdateBabyMetroidCharacterFrame();
    }

    /// <summary>Current native title sub-state, exposed as a stable debugger label.</summary>
    public TitleSequencePhase Phase => phase;

    /// <summary>Current INIDISP brightness nibble; zero is black and fifteen is full.</summary>
    public byte Brightness => (byte)brightness;

    /// <summary>Current title CGRAM, including cartridge palette-animation writes.</summary>
    public ReadOnlySpan<ushort> PaletteColors => cgram.Colors;

    /// <summary>Current native Mode-7 A/D scalar, exposed for transform regression audits.</summary>
    public ushort Mode7MatrixScale => unchecked((ushort)zoom);

    /// <summary>Current signed M7HOFS word, exposed for pre-title pan regression audits.</summary>
    public short Mode7HorizontalOffset => unchecked((short)mode7X);

    /// <summary>
    /// NTSC demo countdown. Retail initializes this to $0384 (900 frames); PAL uses $02D0
    /// so both revisions hold the title for approximately fifteen seconds.
    /// </summary>
    public int TitleScreenFramesRemaining => phase == TitleSequencePhase.TitleScreen ? phaseTimer : 0;

    /// <summary>True after the title's slow fade has handed control to file select.</summary>
    public bool FileSelectRequested { get; private set; }
    /// <summary>True after the idle timeout's slow fade reaches native demo state $28.</summary>
    public bool DemoRequested { get; private set; }

    /// <summary>State $2C's player-cancelled return skips the introductory title cards.</summary>
    internal static TitleSequenceState ReturnFromDemo(ISnesAddressSpace bus, CartridgeAudioState audio)
    {
        var title = new TitleSequenceState(bus, audio, queueOpeningMusic: false);
        title.EnterImmediateTitleObjects();
        title.brightness = 0;
        title.phase = TitleSequencePhase.TitleScreenFadeIn;
        return title;
    }

    /// <summary>Runs one accepted title-sequence frame.</summary>
    public void Step(ushort controllerInput)
    {
        controller.Latch(controllerInput);
        bool rebuildConsolePaletteFxAfterStep = false;

        // `$8B:9A48` admits B, Start, or A during every pre-title cinematic function. The
        // original performs a fast fade-out, reconstructs the final title objects, then a
        // fast fade-in. Do not jump straight to a host menu: those intermediate frames and
        // their INIDISP values are observable in native traces.
        bool confirmPressed =
            controller.NewlyPressedButtons.HasAny(TitleSequenceRomData.Timing.ConfirmButtons);
        if (confirmPressed && phase < TitleSequencePhase.TitleScreenFadeIn)
        {
            phase = TitleSequencePhase.SkipFadeOut;
            phaseTimer = 0;
        }

        switch (phase)
        {
            case TitleSequencePhase.YearText:
            case TitleSequencePhase.NintendoText:
            case TitleSequencePhase.PresentsText:
            case TitleSequencePhase.MetroidThreeText:
                StepTextSequence();
                break;

            case TitleSequencePhase.SceneZeroPan:
                AddFixedPoint(
                    ref mode7X,
                    ref mode7XSubposition,
                    -TitleSequenceRomData.Scenes.PanVelocity16Point16);
                if (mode7X < TitleSequenceRomData.Scenes.SceneZeroEndX)
                    BeginTextSequence(TitleSequenceRomData.TextSequences.Nintendo);
                break;

            case TitleSequencePhase.SceneOnePan:
                AddFixedPoint(
                    ref mode7X,
                    ref mode7XSubposition,
                    -TitleSequenceRomData.Scenes.PanVelocity16Point16);
                if (mode7X < TitleSequenceRomData.Scenes.SceneOneEndX)
                    BeginTextSequence(TitleSequenceRomData.TextSequences.Presents);
                break;

            case TitleSequencePhase.SceneTwoPan:
                AddFixedPoint(
                    ref mode7Y,
                    ref mode7YSubposition,
                    TitleSequenceRomData.Scenes.PanVelocity16Point16);
                if (mode7Y >= TitleSequenceRomData.Scenes.SceneTwoEndY)
                    BeginTextSequence(TitleSequenceRomData.TextSequences.MetroidThree);
                break;

            case TitleSequencePhase.SceneThreeZoom:
                // `$8B:9E8B` increments only on even NMI frame counters.
                if ((phaseTimer++ & 1) == 0 && zoom < TitleSequenceRomData.Scenes.IdentityScale)
                    zoom++;
                if (zoom >= TitleSequenceRomData.Scenes.IdentityScale)
                {
                    activeSpritemap = ReadWord(TitleSequenceRomData.Sprites.LogoPointerAddress);
                    activeOriginX = TitleSequenceRomData.Sprites.LogoX;
                    activeOriginY = TitleSequenceRomData.Sprites.LogoY;
                    activeCharacterOffset = TitleSequenceRomData.Sprites.TitleCharacterOffset.Raw;
                    phase = TitleSequencePhase.TitleLogoFade;
                    phaseTimer = TitleSequenceRomData.Timing.LogoHoldFrames;
                }
                break;

            case TitleSequencePhase.TitleLogoFade:
                if (--phaseTimer <= 0)
                {
                    // `$8B:A0E1` holds the copyright for 32 frames before arming the
                    // 900-frame demo countdown. The palette FX itself remains future work;
                    // its final cartridge palette is already the visible CGRAM source.
                    phase = TitleSequencePhase.CopyrightFade;
                    phaseTimer = TitleSequenceRomData.Timing.CopyrightHoldFrames;
                }
                break;

            case TitleSequencePhase.CopyrightFade:
                if (--phaseTimer <= 0)
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.SkipFadeOut:
                brightness = Math.Max(
                    0,
                    brightness - TitleSequenceRomData.Timing.SkipFadeBrightnessStep);
                if (brightness == 0)
                {
                    // HandleCinematicsTransitions_1 at $8B:9A83 queues track six before
                    // rebuilding the immediate title objects.
                    audio?.QueueMusicDelayed8(
                        MusicCommand.SelectTrack(TitleSequenceRomData.Music.ImmediateTitleTrack));
                    EnterImmediateTitleObjects();
                    rebuildConsolePaletteFxAfterStep = true;
                    phase = TitleSequencePhase.TitleScreenFadeIn;
                }
                break;

            case TitleSequencePhase.TitleScreenFadeIn:
                brightness = Math.Min(
                    TitleSequenceRomData.Timing.MaximumBrightness,
                    brightness + TitleSequenceRomData.Timing.SkipFadeBrightnessStep);
                if (brightness == TitleSequenceRomData.Timing.MaximumBrightness)
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.TitleScreen:
                // The native timeout check wins over confirmation on its final frame.
                if (--phaseTimer <= 0)
                {
                    fadingToDemo = true;
                    phase = TitleSequencePhase.TitleScreenFadeOut;
                    phaseTimer = TitleSequenceRomData.Timing.TitleFadeCadenceFrames;
                }
                else if (confirmPressed)
                {
                    phase = TitleSequencePhase.TitleScreenFadeOut;
                    phaseTimer = TitleSequenceRomData.Timing.TitleFadeCadenceFrames;
                }
                break;

            case TitleSequencePhase.TitleScreenFadeOut:
                if (--phaseTimer <= 0)
                {
                    phaseTimer = TitleSequenceRomData.Timing.TitleFadeCadenceFrames;
                    brightness = Math.Max(0, brightness - 1);
                    if (brightness == 0)
                    {
                        DemoRequested = fadingToDemo;
                        FileSelectRequested = !fadingToDemo;
                    }
                }
                break;
        }

        StepBabyMetroidAnimation();
        // The title calls the same bank-$8D interpreter as rooms. Its two console
        // programs own their colors and timers; the host must not synthesize a blink.
        consolePaletteFx.Step(bus, cgram, 0, 0, false, false);
        // Native skip reconstruction follows PaletteFxHandler for this frame. Restart
        // only here, not when the subsequent fade-in arms the title idle countdown.
        if (rebuildConsolePaletteFxAfterStep)
            ResetConsolePaletteFx();
    }

    private void ResetConsolePaletteFx()
    {
        consolePaletteFx = new RoomPaletteFxSystem();
        consolePaletteFx.SpawnDefinition(bus, TitleSequenceRomData.ConsolePaletteFx.SlowLights, 0);
        consolePaletteFx.SpawnDefinition(bus, TitleSequenceRomData.ConsolePaletteFx.FastLights, 0);
    }

    /// <summary>Renders the current Mode 7 background and bank-$8C title spritemaps.</summary>
    public Rgba32[] Render()
    {
        // `$8B:9E8B` reaches identity A=D=$0100. The value being animated is already the
        // Mode 7 matrix scalar, not a camera magnification that needs to be inverted. A
        // small matrix samples less source texture across the output screen, so the early
        // title image is enlarged; increasing $43 -> $100 therefore performs the retail
        // zoom *out*. Taking its reciprocal reversed that motion and also magnified the
        // Nintendo Presents pan offsets until much of their movement wrapped off-screen.
        short matrixScale = unchecked((short)zoom);
        Rgba32[] background = SnesLayerCompositor.CreateBackdrop(
            cgram,
            FrontendFrame.Width * FrontendFrame.Height);

        // Setup_PPU_TitleSequence writes TM=$10 at `$8B:803F`: OBJ is visible but BG1 is
        // not. Each scrolling-text command leaves that state alone, so the 1994/NINTENDO/
        // PRESENTS/METROID 3 cards sit over the CGRAM-zero black backdrop. Their following
        // `$9CE1/$9D5D/$9DD6/$9E58` scene command writes TM=$11 and enables Mode 7 BG1;
        // each of the first three completed pans restores TM=$10 before spawning the next
        // text object. Rendering a zero-scale Mode 7 sample during YearText was therefore
        // not merely the wrong transform—it displayed a layer the SNES had disabled and
        // turned the whole screen into the sampled red texel.
        if (mode7BackgroundEnabled)
        {
            Rgba32[] mode7Layer = SnesMode7Renderer.RenderViewport(
                vram,
                cgram,
                matrixScale,
                TitleSequenceRomData.Scenes.Rotation.TableIndex,
                0,
                matrixScale,
                128,
                128,
                unchecked((short)mode7X),
                unchecked((short)mode7Y));
            SnesLayerCompositor.Composite(background, mode7Layer);
        }

        oam.BeginFrame();
        if (activeSpritemap != TitleSequenceRomData.Sprites.Blank)
        {
            // Cinematic drawing calls `$81:879F`, whose `chr_r22` replaces palette bits
            // after masking the ROM attributes with `$F1FF`. It does not add a base tile;
            // confusing it with the enemy loader makes the title art uniformly blue.
            oam.AddOnScreenSpritemap(
                bus,
                (int)new SnesAddress(TitleSequenceRomData.Sprites.Bank, activeSpritemap),
                activeOriginX,
                activeOriginY,
                activeCharacterOffset);
        }

        if (phase is >= TitleSequencePhase.CopyrightFade and <= TitleSequencePhase.TitleScreenFadeOut)
        {
            oam.AddOnScreenSpritemap(
                bus,
                (int)new SnesAddress(
                    TitleSequenceRomData.Sprites.Bank,
                    TitleSequenceRomData.Sprites.NintendoCopyright),
                TitleSequenceRomData.Sprites.CopyrightX,
                TitleSequenceRomData.Sprites.CopyrightY,
                TitleSequenceRomData.Sprites.CopyrightPalette.Raw);
        }

        oam.FinalizeFrame();
        Rgba32[] objects = SnesObjRenderer.Render(
            oam,
            vram,
            cgram,
            obsel: TitleSequenceRomData.Sprites.ObjectSizeAndBaseSelector);
        SnesLayerCompositor.Composite(background, objects);
        for (int pixel = 0; pixel < background.Length; pixel++)
            background[pixel] = ApplyBrightness(background[pixel], brightness);

        return background;
    }

    private void StepTextSequence()
    {
        if (--sequenceEntryTimer > 0)
            return;

        while (true)
        {
            int entryAddress = sequenceEntry;
            ushort durationOrCommand = ReadWord(entryAddress);
            if ((durationOrCommand & TitleSequenceRomData.TextSequences.CommandBit) == 0)
            {
                sequenceEntryTimer = durationOrCommand;
                activeSpritemap = ReadWord(entryAddress + 2);
                sequenceEntry = AddWithinBank(
                    entryAddress,
                    TitleSequenceRomData.TextSequences.TimedEntryByteCount);
                return;
            }

            sequenceEntry = AddWithinBank(
                entryAddress,
                TitleSequenceRomData.TextSequences.InstructionWordByteCount);
            switch (durationOrCommand)
            {
                case CinematicCodePointers.Instruction_TriggerTitleSequenceScene0:
                    phase = TitleSequencePhase.SceneZeroPan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9CE3-$9CE5`.
                    ApplyScene(TitleSequenceRomData.Scenes.SceneZero);
                    return;

                case CinematicCodePointers.Instruction_TriggerTitleSequenceScene1:
                    phase = TitleSequencePhase.SceneOnePan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9D5D-$9D61`.
                    ApplyScene(TitleSequenceRomData.Scenes.SceneOne);
                    return;

                case CinematicCodePointers.Instruction_TriggerTitleSequenceScene2:
                    phase = TitleSequencePhase.SceneTwoPan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9DD6-$9DDA`.
                    ApplyScene(TitleSequenceRomData.Scenes.SceneTwo);
                    return;

                case CinematicCodePointers.Instruction_TriggerTitleSequenceScene3:
                    phase = TitleSequencePhase.SceneThreeZoom;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9E58-$9E5C`.
                    phaseTimer = 0;
                    ApplyScene(TitleSequenceRomData.Scenes.SceneThree);
                    return;

                case CinematicCodePointers.CinematicSpriteObject_Instruction_Delete:
                    activeSpritemap = TitleSequenceRomData.Sprites.Blank;
                    return;

                default:
                    // The three title-scene lists use the complete command set above.
                    throw new InvalidDataException(
                        $"Title sequence instruction $8B:{durationOrCommand:X4} is invalid for the active retail list.");
            }
        }
    }

    private void BeginTextSequence(TitleTextSequenceDefinition definition)
    {
        phase = definition.Phase;
        sequenceEntry = definition.InstructionAddress;
        sequenceEntryTimer = 1;
        activeSpritemap = TitleSequenceRomData.Sprites.Blank;
        activeOriginX = definition.OriginX;
        activeOriginY = definition.OriginY;
        activeCharacterOffset = definition.CharacterOffset;
        mode7XSubposition = 0;
        mode7YSubposition = 0;

        // Initial PPU setup and each of the first three completed pan functions select
        // TM=$10 before the next text actor becomes visible. Keeping an explicit register
        // bit also matters when Start skips during a pan: the fade-out preserves whichever
        // TM value was live rather than inferring visibility from the host phase name.
        mode7BackgroundEnabled = false;
    }

    private void EnterImmediateTitleObjects()
    {
        // Skip transition `$8B:9A9C` explicitly overwrites the two glyph colors used by
        // the Nintendo copyright spritemap after restoring CGRAM's upper half.
        cgram.SetColor(
            TitleSequenceRomData.Palette.CopyrightWhiteIndex,
            TitleSequenceRomData.Palette.CopyrightWhite);
        cgram.SetColor(
            TitleSequenceRomData.Palette.CopyrightRedIndex,
            TitleSequenceRomData.Palette.CopyrightRed);
        activeSpritemap = TitleSequenceRomData.Sprites.SuperMetroidLogo;
        activeOriginX = TitleSequenceRomData.Sprites.LogoX;
        activeOriginY = TitleSequenceRomData.Sprites.LogoY;
        activeCharacterOffset = TitleSequenceRomData.Sprites.TitleCharacterOffset.Raw;
        mode7BackgroundEnabled = true;
        mode7X = 0;
        mode7Y = 0;
        zoom = TitleSequenceRomData.Scenes.IdentityScale;
    }

    private void EnterTitleScreen()
    {
        EnterImmediateTitleObjects();
        phase = TitleSequencePhase.TitleScreen;
        phaseTimer = TitleSequenceRomData.Timing.TitleScreenNtscFrames;
        brightness = TitleSequenceRomData.Timing.MaximumBrightness;
    }

    private void LoadMode7InterleavedVram(byte[] characterBytes, byte[] mapBytes)
    {
        if (characterBytes.Length < TitleSequenceRomData.Vram.Mode7CharacterByteCount)
            throw new InvalidDataException("Title Mode 7 character stream is shorter than its $4000-byte DMA.");
        if (mapBytes.Length < TitleSequenceRomData.Vram.Mode7MapByteCount)
            throw new InvalidDataException("Title Mode 7 map stream is shorter than its $1000-byte DMA.");

        // DMA mode zero to $2119 writes only high bytes and increments VMADD afterward.
        // The following $2118 fill and map DMA populate the corresponding low bytes.
        vram.FillMode7MapBytes(
            TitleSequenceRomData.Vram.Mode7InitialMapByte,
            TitleSequenceRomData.Vram.Mode7CharacterByteCount);
        vram.LoadMode7CharacterBytes(
            characterBytes.AsSpan(0, TitleSequenceRomData.Vram.Mode7CharacterByteCount));
        vram.LoadMode7MapBytes(
            mapBytes.AsSpan(0, TitleSequenceRomData.Vram.Mode7MapByteCount));
    }

    private void StepBabyMetroidAnimation()
    {
        if (--babyFrameTimer > 0)
            return;

        babyFrame = (babyFrame + 1) % TitleSequenceRomData.Vram.BabyAnimationSourcePages.Length;
        babyFrameTimer = TitleSequenceRomData.Timing.BabyFrameDuration;
        UpdateBabyMetroidCharacterFrame();
    }

    private void UpdateBabyMetroidCharacterFrame()
    {
        // The instruction list cycles source pages 0,1,2,1 into Mode 7 destination word
        // `$3800`, high byte only. Each page is exactly $100 bytes / four characters.
        int sourcePage = TitleSequenceRomData.Vram.BabyAnimationSourcePages[babyFrame];
        int source = sourcePage * TitleSequenceRomData.Vram.BabyCharacterPageByteCount;
        if (babyMetroidCharacters.Length <
            source + TitleSequenceRomData.Vram.BabyCharacterPageByteCount)
            throw new InvalidDataException("Title baby-Metroid stream is missing an animation page.");

        for (int byteIndex = 0;
             byteIndex < TitleSequenceRomData.Vram.BabyCharacterPageByteCount;
             byteIndex++)
        {
            int word = TitleSequenceRomData.Vram.BabyCharacterDestinationWord + byteIndex;
            ushort value = (ushort)(
                (vram.ReadWord(word) & TitleSequenceRomData.Vram.Mode7MapLowByteMask) |
                (babyMetroidCharacters[source + byteIndex] <<
                    TitleSequenceRomData.Vram.Mode7CharacterByteShift));
            vram.ExecuteWordTransfer([value], (ushort)word, 1);
        }
    }

    private ushort ReadWord(int address) => (ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8));

    private static int AddWithinBank(int address, int bytes) =>
        (int)SnesAddress.FromBusAddress(address).AddWithinBank(bytes);

    private static void AddFixedPoint(ref int integer, ref int fraction, int delta16Point16)
    {
        long combined = ((long)integer << 16) | (ushort)fraction;
        combined += delta16Point16;
        integer = unchecked((short)(combined >> 16));
        fraction = (ushort)combined;
    }

    private void ApplyScene(TitleMode7SceneDefinition definition)
    {
        phase = definition.Phase;
        mode7BackgroundEnabled = true;
        zoom = definition.Scale;
        mode7X = definition.HorizontalOffset;
        mode7Y = definition.VerticalOffset;
        activeSpritemap = TitleSequenceRomData.Sprites.Blank;
    }

    private static Rgba32 ApplyBrightness(Rgba32 color, int level)
    {
        if (color.A == 0 || level >= TitleSequenceRomData.Timing.MaximumBrightness)
            return color;
        if (level <= 0)
            return new Rgba32(0, 0, 0, color.A);

        return new Rgba32(
            (byte)(color.R * level / TitleSequenceRomData.Timing.MaximumBrightness),
            (byte)(color.G * level / TitleSequenceRomData.Timing.MaximumBrightness),
            (byte)(color.B * level / TitleSequenceRomData.Timing.MaximumBrightness),
            color.A);
    }
}

/// <summary>Debugger-facing names for the visible title-sequence functions.</summary>
public enum TitleSequencePhase
{
    YearText,
    SceneZeroPan,
    NintendoText,
    SceneOnePan,
    PresentsText,
    SceneTwoPan,
    MetroidThreeText,
    SceneThreeZoom,
    TitleLogoFade,
    CopyrightFade,
    SkipFadeOut,
    TitleScreenFadeIn,
    TitleScreen,
    TitleScreenFadeOut,
}
