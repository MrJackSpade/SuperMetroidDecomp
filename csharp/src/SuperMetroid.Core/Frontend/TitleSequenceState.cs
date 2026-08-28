using SuperMetroid.Core.Assets;
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
    private const int PaletteAddress = 0x8ce1e9;
    private const int Mode7CharactersAddress = 0x94e000;
    private const int Mode7MapAddress = 0x96fc04;
    private const int ObjectCharactersAddress = 0x9580d8;
    private const int BabyMetroidCharactersAddress = 0x95a5e1;

    private const ushort BlankSpritemap = 0x0000;
    private const ushort SuperMetroidLogoSpritemap = 0x879d;
    private const ushort NintendoCopyrightSpritemap = 0x8103;

    private readonly ISnesAddressSpace bus;
    private readonly SnesVram vram = new();
    private readonly SnesCgram cgram = new();
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

    /// <summary>Creates the native initial title setup performed by <c>$8B:9B68</c>.</summary>
    public TitleSequenceState(ISnesAddressSpace bus)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));

        // `$8B:9B87` expands these four independent streams to bank-$7F. Recreate the
        // subsequent DMA destinations rather than keeping an invented host texture format.
        byte[] mode7Characters = RomDataReader.Decompress(bus, Mode7CharactersAddress);
        byte[] mode7Map = RomDataReader.Decompress(bus, Mode7MapAddress);
        byte[] objectCharacters = RomDataReader.Decompress(bus, ObjectCharactersAddress);
        babyMetroidCharacters = RomDataReader.Decompress(bus, BabyMetroidCharactersAddress);

        LoadMode7InterleavedVram(mode7Characters, mode7Map);
        vram.LoadBytes(0xc000, objectCharacters.AsSpan(0, Math.Min(0x4000, objectCharacters.Length)));
        cgram.LoadFromBus(bus, PaletteAddress);

        // The first object is definition `$A0EF`: 1994 text at (129,112), character
        // offset `$0400`, instruction list `$A03D`. Its pre-instruction forces full
        // brightness on the first processing frame.
        brightness = 15;
        BeginTextSequence(TitleSequencePhase.YearText, 0x8ba03d, 129, 112, 0x0400);
        UpdateBabyMetroidCharacterFrame();
    }

    /// <summary>Current native title sub-state, exposed as a stable debugger label.</summary>
    public TitleSequencePhase Phase => phase;

    /// <summary>Current INIDISP brightness nibble; zero is black and fifteen is full.</summary>
    public byte Brightness => (byte)brightness;

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

    /// <summary>Runs one accepted title-sequence frame.</summary>
    public void Step(ushort controllerInput)
    {
        controller.Latch(controllerInput);

        // `$8B:9A48` admits B, Start, or A during every pre-title cinematic function. The
        // original performs a fast fade-out, reconstructs the final title objects, then a
        // fast fade-in. Do not jump straight to a host menu: those intermediate frames and
        // their INIDISP values are observable in native traces.
        const SnesButton confirmButtons = SnesButton.B | SnesButton.Start | SnesButton.A;
        bool confirmPressed = ((SnesButton)controller.NewlyPressed & confirmButtons) != 0;
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
                AddFixedPoint(ref mode7X, ref mode7XSubposition, -0x0001_8000);
                if (mode7X < -7)
                    BeginTextSequence(TitleSequencePhase.NintendoText, 0x8ba055, 129, 112, 0x0200);
                break;

            case TitleSequencePhase.SceneOnePan:
                AddFixedPoint(ref mode7X, ref mode7XSubposition, -0x0001_8000);
                if (mode7X < -176)
                    BeginTextSequence(TitleSequencePhase.PresentsText, 0x8ba079, 129, 112, 0x0200);
                break;

            case TitleSequencePhase.SceneTwoPan:
                AddFixedPoint(ref mode7Y, ref mode7YSubposition, 0x0001_8000);
                if (mode7Y >= 163)
                    BeginTextSequence(TitleSequencePhase.MetroidThreeText, 0x8ba09d, 129, 112, 0x0200);
                break;

            case TitleSequencePhase.SceneThreeZoom:
                // `$8B:9E8B` increments only on even NMI frame counters.
                if ((phaseTimer++ & 1) == 0 && zoom < 0x0100)
                    zoom++;
                if (zoom >= 0x0100)
                {
                    activeSpritemap = ReadWord(0x8ba0c7);
                    activeOriginX = 128;
                    activeOriginY = 48;
                    activeCharacterOffset = 0x0400;
                    phase = TitleSequencePhase.TitleLogoFade;
                    phaseTimer = 32;
                }
                break;

            case TitleSequencePhase.TitleLogoFade:
                if (--phaseTimer <= 0)
                {
                    // `$8B:A0E1` holds the copyright for 32 frames before arming the
                    // 900-frame demo countdown. The palette FX itself remains future work;
                    // its final cartridge palette is already the visible CGRAM source.
                    phase = TitleSequencePhase.CopyrightFade;
                    phaseTimer = 32;
                }
                break;

            case TitleSequencePhase.CopyrightFade:
                if (--phaseTimer <= 0)
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.SkipFadeOut:
                brightness = Math.Max(0, brightness - 2);
                if (brightness == 0)
                {
                    EnterImmediateTitleObjects();
                    phase = TitleSequencePhase.TitleScreenFadeIn;
                }
                break;

            case TitleSequencePhase.TitleScreenFadeIn:
                brightness = Math.Min(15, brightness + 2);
                if (brightness == 15)
                    EnterTitleScreen();
                break;

            case TitleSequencePhase.TitleScreen:
                if (confirmPressed)
                {
                    phase = TitleSequencePhase.TitleScreenFadeOut;
                    phaseTimer = 2;
                }
                else if (--phaseTimer <= 0)
                {
                    // The native timeout enters the attract-mode dispatcher. The playable
                    // C# milestone keeps the title resident until demo playback is ported;
                    // resetting the exact 900-frame counter avoids an invented auto-start.
                    phaseTimer = 900;
                }
                break;

            case TitleSequencePhase.TitleScreenFadeOut:
                if (--phaseTimer <= 0)
                {
                    phaseTimer = 2;
                    brightness = Math.Max(0, brightness - 1);
                    if (brightness == 0)
                        FileSelectRequested = true;
                }
                break;
        }

        StepBabyMetroidAnimation();
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
        Rgba32[] background = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);

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
                0,
                0,
                matrixScale,
                128,
                128,
                unchecked((short)mode7X),
                unchecked((short)mode7Y));
            SnesLayerCompositor.Composite(background, mode7Layer);
        }

        oam.BeginFrame();
        if (activeSpritemap != BlankSpritemap)
        {
            // Cinematic drawing calls `$81:879F`, whose `chr_r22` replaces palette bits
            // after masking the ROM attributes with `$F1FF`. It does not add a base tile;
            // confusing it with the enemy loader makes the title art uniformly blue.
            oam.AddOnScreenSpritemap(
                bus,
                0x8c0000 | activeSpritemap,
                activeOriginX,
                activeOriginY,
                activeCharacterOffset);
        }

        if (phase is >= TitleSequencePhase.CopyrightFade and <= TitleSequencePhase.TitleScreenFadeOut)
        {
            oam.AddOnScreenSpritemap(
                bus,
                0x8c0000 | NintendoCopyrightSpritemap,
                128,
                196,
                0x0800);
        }

        oam.FinalizeFrame();
        Rgba32[] objects = SnesObjRenderer.Render(oam, vram, cgram, obsel: 0x03);
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
            if ((durationOrCommand & 0x8000) == 0)
            {
                sequenceEntryTimer = durationOrCommand;
                activeSpritemap = ReadWord(entryAddress + 2);
                sequenceEntry = AddWithinBank(entryAddress, 4);
                return;
            }

            sequenceEntry = AddWithinBank(entryAddress, 2);
            switch (durationOrCommand)
            {
                case 0x9ce1: // Trigger title scene zero.
                    phase = TitleSequencePhase.SceneZeroPan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9CE3-$9CE5`.
                    zoom = 0x48;
                    mode7X = 0x013b;
                    mode7Y = 0x00e1;
                    activeSpritemap = BlankSpritemap;
                    return;

                case 0x9d5d: // Trigger title scene one.
                    phase = TitleSequencePhase.SceneOnePan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9D5D-$9D61`.
                    zoom = 0x60;
                    mode7X = 0x002c;
                    mode7Y = unchecked((short)0xff65);
                    activeSpritemap = BlankSpritemap;
                    return;

                case 0x9dd6: // Trigger title scene two.
                    phase = TitleSequencePhase.SceneTwoPan;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9DD6-$9DDA`.
                    zoom = 0x60;
                    mode7X = unchecked((short)0xff4f);
                    mode7Y = unchecked((short)0xff60);
                    activeSpritemap = BlankSpritemap;
                    return;

                case 0x9e58: // Trigger title scene three.
                    phase = TitleSequencePhase.SceneThreeZoom;
                    mode7BackgroundEnabled = true; // TM=$11 at `$8B:9E58-$9E5C`.
                    phaseTimer = 0;
                    zoom = 0x43;
                    mode7X = 0;
                    mode7Y = 0;
                    activeSpritemap = BlankSpritemap;
                    return;

                case 0x9438: // Cinematic sprite delete.
                    activeSpritemap = BlankSpritemap;
                    return;

                default:
                    throw new NotSupportedException(
                        $"Title sequence instruction $8B:{durationOrCommand:X4} is not translated.");
            }
        }
    }

    private void BeginTextSequence(
        TitleSequencePhase nextPhase,
        int instructionAddress,
        ushort x,
        ushort y,
        ushort characterOffset)
    {
        phase = nextPhase;
        sequenceEntry = instructionAddress;
        sequenceEntryTimer = 1;
        activeSpritemap = BlankSpritemap;
        activeOriginX = x;
        activeOriginY = y;
        activeCharacterOffset = characterOffset;
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
        cgram.SetColor(201, 0x7fff);
        cgram.SetColor(202, 0x7d80);
        activeSpritemap = SuperMetroidLogoSpritemap;
        activeOriginX = 128;
        activeOriginY = 48;
        activeCharacterOffset = 0x0400;
        mode7BackgroundEnabled = true;
        mode7X = 0;
        mode7Y = 0;
        zoom = 0x0100;
    }

    private void EnterTitleScreen()
    {
        EnterImmediateTitleObjects();
        phase = TitleSequencePhase.TitleScreen;
        phaseTimer = 900;
        brightness = 15;
    }

    private void LoadMode7InterleavedVram(byte[] characterBytes, byte[] mapBytes)
    {
        if (characterBytes.Length < 0x4000)
            throw new InvalidDataException("Title Mode 7 character stream is shorter than its $4000-byte DMA.");
        if (mapBytes.Length < 0x1000)
            throw new InvalidDataException("Title Mode 7 map stream is shorter than its $1000-byte DMA.");

        // DMA mode zero to $2119 writes only high bytes and increments VMADD afterward.
        // The following $2118 fill and map DMA populate the corresponding low bytes.
        vram.FillMode7MapBytes(0xff, 0x4000);
        vram.LoadMode7CharacterBytes(characterBytes.AsSpan(0, 0x4000));
        vram.LoadMode7MapBytes(mapBytes.AsSpan(0, 0x1000));
    }

    private void StepBabyMetroidAnimation()
    {
        if (--babyFrameTimer > 0)
            return;

        babyFrame = (babyFrame + 1) % 4;
        babyFrameTimer = 10;
        UpdateBabyMetroidCharacterFrame();
    }

    private void UpdateBabyMetroidCharacterFrame()
    {
        // The instruction list cycles source pages 0,1,2,1 into Mode 7 destination word
        // `$3800`, high byte only. Each page is exactly $100 bytes / four characters.
        int sourcePage = babyFrame is 0 ? 0 : babyFrame is 2 ? 2 : 1;
        int source = sourcePage * 0x0100;
        if (babyMetroidCharacters.Length < source + 0x0100)
            throw new InvalidDataException("Title baby-Metroid stream is missing an animation page.");

        for (int byteIndex = 0; byteIndex < 0x0100; byteIndex++)
        {
            int word = 0x3800 + byteIndex;
            ushort value = (ushort)((vram.ReadWord(word) & 0x00ff) | (babyMetroidCharacters[source + byteIndex] << 8));
            vram.ExecuteWordTransfer([value], (ushort)word, 1);
        }
    }

    private ushort ReadWord(int address) => (ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8));

    private static int AddWithinBank(int address, int bytes) =>
        (address & 0x00ff_0000) | ((address + bytes) & 0xffff);

    private static void AddFixedPoint(ref int integer, ref int fraction, int delta16Point16)
    {
        long combined = ((long)integer << 16) | (ushort)fraction;
        combined += delta16Point16;
        integer = unchecked((short)(combined >> 16));
        fraction = (ushort)combined;
    }

    private static Rgba32 ApplyBrightness(Rgba32 color, int level)
    {
        if (color.A == 0 || level >= 15)
            return color;
        if (level <= 0)
            return new Rgba32(0, 0, 0, color.A);

        return new Rgba32(
            (byte)(color.R * level / 15),
            (byte)(color.G * level / 15),
            (byte)(color.B * level / 15),
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
