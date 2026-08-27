using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
static void VerifyLoRomCrossBankCompressedData()
{
    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];

    // The four-byte literal begins at $94:FFFC. Its final byte is $94:FFFF and the $FF
    // terminator is the physically adjacent byte at $95:8000. This is the same bank-cross
    // shape used by the real title graphics stream, reduced to a six-byte fixture.
    byte[] compressed = [0x03, 0x11, 0x22, 0x33, 0x44, 0xff];
    int beforeCrossing = SuperMetroidAddressSpace.ToRomOffset(0x94fffc);
    int afterCrossing = SuperMetroidAddressSpace.ToRomOffset(0x958000);
    compressed.AsSpan(0, 4).CopyTo(rom.AsSpan(beforeCrossing));
    compressed.AsSpan(4, 2).CopyTo(rom.AsSpan(afterCrossing));

    var bus = new SuperMetroidAddressSpace(rom);
    byte[] output = RomDataReader.Decompress(bus, 0x94fffc, maximumCompressedBytes: 16);
    AssertEqual(4, output.Length, "cross-bank decompressed length");
    AssertEqual(0x11, output[0], "cross-bank first literal");
    AssertEqual(0x44, output[3], "cross-bank final literal");
}

static void VerifyMode7Rendering()
{
    var vram = new SnesVram();
    var cgram = new SnesCgram();
    cgram.SetColor(5, 0x001f); // Fully red BGR555 diagnostic color.

    // Mode 7's tilemap lives in low VRAM bytes. Select character one for map cell zero,
    // then put palette index five in every high byte of that character's 64 words.
    vram.LoadBytes(0, [1]);
    for (int pixel = 0; pixel < 64; pixel++)
    {
        ushort word = (ushort)(64 + pixel);
        vram.ExecuteWordTransfer([(ushort)(5 << 8)], word, 1);
    }

    Rgba32[] identity = SnesMode7Renderer.RenderViewport(
        vram, cgram,
        matrixA: 0x0100, matrixB: 0,
        matrixC: 0, matrixD: 0x0100,
        centerX: 0, centerY: 0,
        horizontalOffset: 0, verticalOffset: 0,
        width: 8, height: 8);
    AssertEqual(64, identity.Length, "Mode 7 identity dimensions");
    AssertTrue(identity.All(pixel => pixel.R == 255 && pixel.G == 0 && pixel.B == 0),
        "Mode 7 identity reads map low bytes and character high bytes");

    // An offset of 1024 is outside the 10-bit map. M7SEL=$80 disables wrapping and keeps
    // that overflow transparent; character-zero fill would require both bits ($C0).
    Rgba32[] outside = SnesMode7Renderer.RenderViewport(
        vram, cgram,
        matrixA: 0x0100, matrixB: 0,
        matrixC: 0, matrixD: 0x0100,
        centerX: 0, centerY: 0,
        horizontalOffset: 1024, verticalOffset: 0,
        width: 1, height: 1);
    AssertEqual(0, outside[0].A, "Mode 7 transparent outside fill");

    // A non-identity transform is essential here: applying HOFS after the matrix gives
    // the same answer under identity and hid the Ceres diagonal-boundary defect. With a
    // 90-degree matrix around (16,16), horizontal scroll +8 maps screen (16,16) to texel
    // (16,8), not (24,16). Give those two cells different characters to make the ordering
    // directly observable instead of asserting the transform against another formula.
    cgram.SetColor(6, 0x03e0); // Fully green diagnostic color.
    for (int pixel = 0; pixel < 64; pixel++)
    {
        ushort word = (ushort)(2 * 64 + pixel);
        vram.ExecuteWordTransfer([(ushort)(6 << 8)], word, 1);
    }
    // Populate map low bytes after the diagnostic character upload because Mode 7 shares
    // every physical word between those two planes; a full-word test transfer would
    // otherwise clear the map byte while writing the character's high byte.
    vram.LoadBytes((1 * 128 + 2) * 2, [1]); // Correct texel cell (16,8): red character 1.
    vram.LoadBytes((2 * 128 + 3) * 2, [2]); // Old post-matrix-scroll cell: green character 2.

    Rgba32[] rotatedScroll = SnesMode7Renderer.RenderViewport(
        vram, cgram,
        matrixA: 0, matrixB: 0x0100,
        matrixC: -0x0100, matrixD: 0,
        centerX: 16, centerY: 16,
        horizontalOffset: 8, verticalOffset: 0,
        width: 17, height: 17);
    AssertEqual(cgram.GetRgba(5), rotatedScroll[16 * 17 + 16],
        "Mode 7 transforms scroll before adding pivot");
}

static void VerifyLayerCompositorBackdrop()
{
    var cgram = new SnesCgram();
    cgram.SetColor(0, 0x0421);
    cgram.SetColor(1, 0x001f);

    // A decoded layer deliberately uses alpha zero as its palette-index-zero key. Final
    // SNES scanout never has transparency, so an untouched pixel must retain opaque CGRAM
    // color zero while a non-keyed layer pixel replaces it.
    Rgba32[] frame = SnesLayerCompositor.CreateBackdrop(cgram, pixelCount: 2);
    Rgba32[] layer =
    [
        new Rgba32(255, 0, 255, 0),
        cgram.GetRgba(1),
    ];
    SnesLayerCompositor.Composite(frame, layer);

    AssertEqual(255, frame[0].A, "keyed backdrop remains opaque");
    AssertEqual(cgram.GetRgba(0), frame[0], "keyed pixel reveals CGRAM color zero");
    AssertEqual(cgram.GetRgba(1), frame[1], "non-keyed pixel replaces backdrop");
}

static void VerifyBgPriorityPlaneRendering()
{
    var vram = new SnesVram();
    var cgram = new SnesCgram();
    cgram.SetColor(5, 0x001f);

    // Two adjacent map cells use the same visible 2-bpp character and palette. Only bit
    // $2000 differs, so priority-plane filtering must place one red pixel in each result.
    vram.ExecuteWordTransfer([0x0401, 0x2401], destinationWord: 0, wordIncrement: 1);
    vram.LoadBytes(0x0210, [0x80, 0x00]); // Character 1, first row, leftmost color-1 pixel.

    Rgba32[] low = SnesBgTilemapRenderer.Render2Bpp(
        vram, cgram, tilemapBaseWord: 0, characterBaseWord: 0x0100, rowCount: 1,
        transparentColorZero: true, priority: false);
    Rgba32[] high = SnesBgTilemapRenderer.Render2Bpp(
        vram, cgram, tilemapBaseWord: 0, characterBaseWord: 0x0100, rowCount: 1,
        transparentColorZero: true, priority: true);

    AssertEqual(255, low[0].A, "low-priority BG cell selected");
    AssertEqual(0, low[8].A, "high-priority BG cell omitted from low plane");
    AssertEqual(0, high[0].A, "low-priority BG cell omitted from high plane");
    AssertEqual(255, high[8].A, "high-priority BG cell selected");
}

static void VerifyFileSelectFreshSaveTilemap()
{
    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];

    // FileSelectMenuState also loads the five labels surrounding NO DATA. Empty streams are
    // sufficient for this focused fixture, but each one still needs the native $FFFF
    // terminator so the cartridge-backed loader cannot wander into zero-filled ROM.
    int[] unusedLabelAddresses =
    [
        0x81b40a, // SAMUS DATA
        0x81b436, // SAMUS A
        0x81b456, // SAMUS B
        0x81b476, // SAMUS C
        0x81b4ee, // EXIT
    ];
    foreach (int address in unusedLabelAddresses)
        WriteRomWord(rom, address, 0xffff);

    // This is the literal retail structure at $81:B4AC: one leading blank, "NO DATA",
    // and three trailing blanks. Keeping the exact words makes the test cover both the
    // destination coordinates and the fact that the source itself intentionally starts
    // one character before the visible N.
    ushort[] noDataWords =
    [
        0x000f,
        0x2077, 0x2078,
        0x200f,
        0x206d, 0x206a, 0x207d, 0x206a,
        0x200f, 0x200f, 0x200f,
        0xffff,
    ];
    for (int index = 0; index < noDataWords.Length; index++)
        WriteRomWord(rom, 0x81b4ac + index * 2, noDataWords[index]);

    var menu = new FileSelectMenuState(new SuperMetroidAddressSpace(rom));
    ReadOnlySpan<ushort> tilemap = menu.BackgroundTilemap;

    // Native empty-slot origins are energy-field X plus one $40-byte row: rows 6, 11,
    // and 16 at column 14. Consequently every visible N begins at column 15 and the whole
    // seven-character label remains on that row. A next-row column-zero check catches the
    // exact wraparound regression that placed N above "O DATA".
    int[] labelStarts = [0x019c / 2, 0x02dc / 2, 0x041c / 2];
    foreach (int start in labelStarts)
    {
        AssertEqual(0x000f, tilemap[start], "file-select NO DATA leading blank");
        AssertEqual(0x2077, tilemap[start + 1], "file-select NO DATA N column");
        AssertEqual(0x2078, tilemap[start + 2], "file-select NO DATA O column");
        AssertEqual(0x206d, tilemap[start + 4], "file-select NO DATA D column");
        AssertEqual(0x206a, tilemap[start + 5], "file-select NO DATA A column");
        AssertEqual(0x207d, tilemap[start + 6], "file-select NO DATA T column");
        AssertEqual(0x206a, tilemap[start + 7], "file-select NO DATA final A column");
        int followingRowStart = ((start / 32) + 1) * 32;
        AssertEqual(0x000f, tilemap[followingRowStart],
            "file-select NO DATA does not wrap to next row");
    }

    Console.WriteLine("  File select: all three fresh-save NO DATA labels stay on their native rows.");
}

static void VerifyIntroGameplayFlashbackVerticalScroll()
{
    // `$8B:A66F` installs BG1VOFS eight for the illustrated page. The two following
    // gameplay-style setup functions replace BG1SC but leave that scroll word untouched.
    // Lock the inherited value independently of any host-authored actor coordinates.
    AssertEqual(8, IntroCinematicState.GameplayFlashbackBg1VerticalScroll,
        "intro gameplay flashback inherited BG1 vertical scroll");

    var motherBrain = new IntroMotherBrainSpriteState();
    AssertEqual(8, motherBrain.BackgroundVerticalScroll,
        "intro Mother Brain starts from inherited BG1 vertical scroll");

    // Four impacts select `$8B:B80F`. Its `$8B:B877` shake adds four on an even frame and
    // removes four on an odd frame, oscillating around eight rather than around zero.
    for (int hit = 0; hit < 4; hit++)
        motherBrain.RegisterMissileHit();
    var cgram = new SnesCgram();
    var introPalette = new ushort[SnesCgram.ColorCount];
    motherBrain.RunPreInstruction(cgram, introPalette, cinematicFrameCounter: 2, introCrossfadeTimer: 0x7f);
    AssertEqual(12, motherBrain.BackgroundVerticalScroll,
        "intro Mother Brain even-frame shake adds four to inherited scroll");
    motherBrain.RunPreInstruction(cgram, introPalette, cinematicFrameCounter: 3, introCrossfadeTimer: 0x7f);
    AssertEqual(8, motherBrain.BackgroundVerticalScroll,
        "intro Mother Brain odd-frame shake returns to inherited scroll");

    Console.WriteLine("  Intro: Mother Brain and SR388 BG1 retain the native eight-pixel vertical scroll.");
}

static void VerifyCinematicPaletteFader()
{
    var target = new ushort[SnesCgram.ColorCount];
    // Component four produces step $0020, making the fixed-point rounding boundary easy
    // to see: seven calls compose to zero and the eighth composes to component one.
    target[20] = 0x1084;
    var cgram = new SnesCgram();
    var fader = new CinematicPaletteFader(target);

    // $8B:B018 clears incoming range $28/$03. Because X is a byte offset, the first
    // affected entry is color $14 rather than color $28.
    fader.Clear(0x0028, 3);
    fader.ComposeInto(cgram);
    AssertEqual(0x0000, cgram.Colors[20], "cinematic clear uses byte-offset color index");

    // One retail step adds component four << 3 = $0020. Compose takes the high byte,
    // so the first seven calls remain black and the eighth produces component one.
    for (int step = 0; step < 7; step++)
        fader.FadeIn(0x0028, 1);
    fader.ComposeInto(cgram);
    AssertEqual(0x0000, cgram.Colors[20], "8.8 cinematic fade preserves early rounding");
    fader.FadeIn(0x0028, 1);
    fader.ComposeInto(cgram);
    AssertEqual(0x0421, cgram.Colors[20], "eighth fade step reaches BGR component one");

    for (int step = 8; step < 32; step++)
        fader.FadeIn(0x0028, 1);
    fader.ComposeInto(cgram);
    AssertEqual(0x1084, cgram.Colors[20], "32 fade-in calls reach exact target");

    for (int step = 0; step < 32; step++)
        fader.FadeOut(0x0028, 1);
    fader.ComposeInto(cgram);
    AssertEqual(0x0000, cgram.Colors[20], "32 fade-out calls reach exact black");
}

static void VerifyDemoInputObject()
{
    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];
    WriteRomWord(rom, 0x918784, 0x83bf); // Initializer: RTS.
    WriteRomWord(rom, 0x918786, 0x83bf); // Pre-instruction: RTS.
    WriteRomWord(rom, 0x918788, 0x8694); // Retail old-Mother-Brain input list.

    // Exact six records at $91:8694. The two one-frame X edges are separated by held-X
    // records; normal projectile cooldown/motion code—not this fixture—decides each shot.
    ushort[] words =
    [
        0x005a, 0x0000, 0x0000,
        0x0001, 0x0040, 0x0040,
        0x0028, 0x0040, 0x0000,
        0x0001, 0x0040, 0x0040,
        0x001d, 0x0040, 0x0000,
        0x0046, 0x0000, 0x0000,
    ];
    for (int index = 0; index < words.Length; index++)
        WriteRomWord(rom, 0x918694 + index * 2, words[index]);

    var demo = new DemoInputState();
    var bus = new SuperMetroidAddressSpace(rom);
    demo.Clear();
    demo.Enable();
    demo.LoadObject(bus, 0x8784);

    int[] boundaries = [90, 1, 40, 1, 29, 70];
    ushort[] held = [0, 0x0040, 0x0040, 0x0040, 0x0040, 0];
    ushort[] newlyPressed = [0, 0x0040, 0, 0x0040, 0, 0];
    int elapsed = 0;
    for (int record = 0; record < boundaries.Length; record++)
    {
        for (int frame = 0; frame < boundaries[record]; frame++)
        {
            demo.Step(bus);
            AssertEqual(held[record], demo.Held, $"demo record {record} held frame {frame}");
            AssertEqual(newlyPressed[record], demo.NewlyPressed,
                $"demo record {record} new frame {frame}");
            elapsed++;
        }
    }

    AssertEqual(231, elapsed, "old Mother Brain demo-input duration");
    AssertEqual(0x86b8, demo.InstructionPointer, "old Mother Brain next record pointer");
    // Loading a record and publishing its first visible frame happen in the same handler
    // call. Consequently the last of its 70 visible frames leaves timer one; the following
    // call decrements to zero and fetches the next record.
    AssertEqual(1, demo.InstructionTimer, "final visible record frame retains timer one");

    // Exercise the generic control instructions separately: set a loop count, consume the
    // decrement/goto twice, then delete. This verifies that the reusable interpreter is not
    // accidentally hard-coded to the intro's all-record list.
    WriteRomWord(rom, 0x918700, 0x8459);
    WriteRomWord(rom, 0x918702, 0x0002);
    WriteRomWord(rom, 0x918704, 0x0001);
    WriteRomWord(rom, 0x918706, 0x1234);
    WriteRomWord(rom, 0x918708, 0x0020);
    WriteRomWord(rom, 0x91870a, 0x844f);
    WriteRomWord(rom, 0x91870c, 0x8704);
    WriteRomWord(rom, 0x91870e, 0x8427);
    WriteRomWord(rom, 0x918720, 0x83bf);
    WriteRomWord(rom, 0x918722, 0x83bf);
    WriteRomWord(rom, 0x918724, 0x8700);

    bus = new SuperMetroidAddressSpace(rom);
    demo.Clear();
    demo.Enable();
    demo.LoadObject(bus, 0x8720);
    demo.Step(bus);
    AssertEqual(0x1234, demo.Held, "demo opcode fixture first held word");
    AssertEqual(2, demo.Timer, "demo set-timer opcode");
    demo.Step(bus);
    AssertEqual(1, demo.Timer, "demo decrement/goto loops while nonzero");
    AssertEqual(0x1234, demo.Held, "demo loop replays input record");
    demo.Step(bus);
    AssertEqual(0, demo.Timer, "demo decrement/goto falls through at zero");
    AssertEqual(0, demo.InstructionPointer, "demo delete clears list pointer");
    AssertEqual(0, demo.Held, "demo delete clears held input");
    AssertEqual(0, demo.NewlyPressed, "demo delete clears new input");

    // Object-specific routine pointers must be acknowledged explicitly and return the exact
    // bytecode cursor after their operands. Model $8739's no-argument disable followed by
    // the shared delete opcode, just as the physical Mother Brain list ends in the ROM.
    WriteRomWord(rom, 0x918730, 0x83bf);
    WriteRomWord(rom, 0x918732, 0x83bf);
    WriteRomWord(rom, 0x918734, 0x8750);
    WriteRomWord(rom, 0x918750, 0x8739);
    WriteRomWord(rom, 0x918752, 0x8427);
    bus = new SuperMetroidAddressSpace(rom);
    demo.Clear();
    demo.Enable();
    demo.LoadObject(bus, 0x8730);
    int specialInstructionCalls = 0;
    demo.Step(bus, specialInstruction: (state, instruction, argumentPointer) =>
    {
        AssertEqual(0x8739, instruction, "demo special instruction pointer");
        specialInstructionCalls++;
        state.Disable();
        return DemoInputInstructionResult.ContinueAt(argumentPointer);
    });
    AssertEqual(1, specialInstructionCalls, "demo special instruction callback count");
    AssertEqual(false, demo.Enabled, "demo special instruction disable");
    AssertEqual(0, demo.InstructionPointer, "demo special instruction continues into delete");

    Console.WriteLine("  Demo input: records, edges, shared opcodes, special dispatch, and deletion agree.");
}

static void WriteRomWord(byte[] rom, int snesAddress, ushort value)
{
    int offset = SuperMetroidAddressSpace.ToRomOffset(snesAddress);
    rom[offset] = unchecked((byte)value);
    rom[offset + 1] = unchecked((byte)(value >> 8));
}
}
