using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
static void VerifyCeresDestructionCinematic()
{
    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];

    AssertEqual(SnesAngle.FromTableIndex(0x20), CeresDestructionRomData.Motion.ApproachAngle,
        "Ceres destruction typed approach angle");
    AssertTrue(
        CeresDestructionRomData.Vram.ObjectCharacterDestinationByte +
            CeresDestructionRomData.Vram.Mode7CharacterBytes <= SnesVram.ByteCount,
        "Ceres destruction OBJ transfer fits VRAM");

    // Each cinematic decompression target is intentionally all zero in this timeline
    // fixture. Long command-one runs encode $4000 bytes in only 49 stored bytes, while the
    // distinct output lengths still exercise every minimum-size assertion and map slice.
    WriteRepeatedCompressedStream(
        rom,
        CeresDestructionRomData.Assets.Mode7Characters,
        CeresDestructionRomData.Vram.Mode7CharacterBytes,
        0);
    WriteRepeatedCompressedChunks(
        rom,
        CeresDestructionRomData.Assets.CeresTilemaps,
        [0x11, 0x22, 0x33, 0x44]);
    WriteRepeatedCompressedStream(
        rom,
        CeresDestructionRomData.Assets.CeresObjectCharacters,
        CeresDestructionRomData.Vram.Mode7CharacterBytes,
        0);
    WriteRepeatedCompressedStream(
        rom,
        CeresDestructionRomData.Assets.ZebesTilemap,
        CeresDestructionRomData.Vram.ZebesTilemapMinimumBytes,
        0);
    WriteRepeatedCompressedStream(
        rom,
        CeresDestructionRomData.Assets.ZebesCharacters,
        CeresDestructionRomData.Vram.Mode7CharacterBytes,
        0);

    // Stable invisible frame lists let this test isolate the state machine from spritemap
    // decoding. The real-ROM audit exercises the same pointers with their retail maps.
    ushort[] stableLists = [0xcc3f, 0xcc4f, 0xcc57, 0xccab, 0xcd83, 0xcd8b, 0xcd93, 0xcd9b];
    foreach (ushort pointer in stableLists)
    {
        WriteRomWord(rom, 0x8b0000 | pointer, 10);
        WriteRomWord(rom, 0x8b0000 | (ushort)(pointer + 2), 0);
        WriteRomWord(rom, 0x8b0000 | (ushort)(pointer + 4), 0x94bc);
        WriteRomWord(rom, 0x8b0000 | (ushort)(pointer + 6), pointer);
    }

    // Explosion actors need only remain alive beyond the first scene in this control-flow
    // test. A long invisible frame preserves the generic interpreter's ordinary data path.
    foreach (ushort pointer in new ushort[] { 0xccdb, 0xccf5, 0xcd1b, 0xce1b })
    {
        WriteRomWord(rom, 0x8b0000 | pointer, 0x7fff);
        WriteRomWord(rom, 0x8b0000 | (ushort)(pointer + 2), 0);
        WriteRomWord(rom, 0x8b0000 | (ushort)(pointer + 4), 0x9438);
    }

    // Ceres' five initial explosions share $CCDB but seed instruction timers 1/16/32/48/64.
    // Prove the reusable object keeps that timer distinct from GeneralTimer, which belongs
    // to the list's decrement-and-goto opcode and must remain untouched.
    var delayedExplosion = new IntroDiscoverySprite(0, 0, 0, 0xccdb);
    delayedExplosion.DelayFirstInstruction(3);
    delayedExplosion.Step(new SuperMetroidAddressSpace(rom));
    delayedExplosion.Step(new SuperMetroidAddressSpace(rom));
    AssertEqual(0, delayedExplosion.SpriteMapPointer,
        "Ceres staggered explosion remains invisible before instruction delay");
    AssertEqual(0, delayedExplosion.GeneralTimer,
        "instruction delay does not overwrite cinematic goto timer");
    delayedExplosion.Step(new SuperMetroidAddressSpace(rom));
    AssertEqual(0, delayedExplosion.SpriteMapPointer,
        "fixture's first delayed explosion frame uses invisible map zero");
    AssertEqual(0xccdf, delayedExplosion.InstructionPointer,
        "third handler call consumes the first delayed explosion frame");

    // Retail US PLANET ZEBES list. These private opcodes must drive the transition into
    // camera flight; replacing the list with a host timer would make this fixture fail.
    ushort[] titleList =
    [
        0x0040, 0,
        0xc9a5,
        0x0020, 0,
        0xc9af,
        0x00c0, 0,
        0xc9bd,
        0x0060, 0,
        0xc9c7,
        0x9438,
    ];
    for (int index = 0; index < titleList.Length; index++)
        WriteRomWord(rom, 0x8bccbb + index * 2, titleList[index]);

    var state = new CeresDestructionCinematicState(new SuperMetroidAddressSpace(rom));
    AssertEqual(CeresDestructionPhase.WaitForMusicQueue, state.Phase,
        "Ceres destruction initial music-queue phase");
    AssertEqual((byte)0x22, state.ReadMode7MapByte(0x0000),
        "Ceres initial map begins at decompressed +$600");
    AssertEqual((byte)0x33, state.ReadMode7MapByte(0x0200),
        "Ceres initial map crosses into decompressed +$800 block");

    int frame = 0;
    var firstFrameByPhase = new Dictionary<CeresDestructionPhase, int>();
    bool checkedGunshipTransfer = false;
    bool observedZebesActorDeletionBeforeHandoff = false;
    while (!state.Finished && frame < 5000)
    {
        state.Step();
        frame++;
        firstFrameByPhase.TryAdd(state.Phase, frame);
        observedZebesActorDeletionBeforeHandoff |=
            state.Phase == CeresDestructionPhase.SlideZebesSceneAway &&
            state.ActiveActorCount < 5;
        if (!checkedGunshipTransfer &&
            state.Phase == CeresDestructionPhase.FlyingAwayFromExplosion)
        {
            AssertEqual((byte)0x11, state.ReadMode7MapByte(0x0000),
                "$8B:C345 installs front-gunship map in upper half");
            AssertEqual((byte)0x11, state.ReadMode7MapByte(0x02ff),
                "front-gunship transfer covers exactly $300 map bytes");
            AssertEqual((byte)0x44, state.ReadMode7MapByte(0x0300),
                "$8B:C345 clears lower Mode-7 half from +$C00 source");
            AssertEqual((byte)0x44, state.ReadMode7MapByte(0x05ff),
                "clear transfer covers exactly $300 map bytes");
            checkedGunshipTransfer = true;
        }
    }

    AssertTrue(state.Finished, "Ceres destruction reaches the state-six handoff");
    AssertTrue(firstFrameByPhase.ContainsKey(CeresDestructionPhase.FlyingAwayFromExplosion),
        "Ceres final explosion enters flying-away phase");
    AssertTrue(firstFrameByPhase.ContainsKey(CeresDestructionPhase.FadeInZebes),
        "Ceres fade selects Zebes Mode-1 reveal");
    AssertTrue(firstFrameByPhase.ContainsKey(CeresDestructionPhase.PlanetZebesTitle),
        "Zebes mosaic installs cartridge sprite actors");
    AssertTrue(firstFrameByPhase.ContainsKey(CeresDestructionPhase.FlyingTowardZebesA),
        "PLANET ZEBES instruction C9C7 starts camera flight");
    AssertTrue(firstFrameByPhase.ContainsKey(CeresDestructionPhase.SlideZebesSceneAway),
        "close-Zebes hold hands motion to actor pre-instructions");
    AssertTrue(checkedGunshipTransfer,
        "Ceres explosion performed the native gunship/clear transfers");
    AssertTrue(observedZebesActorDeletionBeforeHandoff,
        "Zebes actors delete at signed Y=-$80 instead of wrapping through OAM");

    Console.WriteLine(
        $"  Ceres destruction: ROM phases, C9C7 flight, and state-six handoff agree ({frame} calls).");
}

static void WriteRepeatedCompressedChunks(
    byte[] rom,
    int snesAddress,
    ReadOnlySpan<byte> chunkValues)
{
    if (chunkValues.IsEmpty)
        throw new ArgumentException("At least one compressed chunk is required.", nameof(chunkValues));

    int address = snesAddress;
    foreach (byte value in chunkValues)
    {
        // Long command one encodes one repeated $400-byte block. Distinct values make
        // native work-RAM slices observable without embedding any retail asset bytes.
        WriteSequentialRomByte(rom, ref address, 0xe7);
        WriteSequentialRomByte(rom, ref address, 0xff);
        WriteSequentialRomByte(rom, ref address, value);
    }
    WriteSequentialRomByte(rom, ref address, 0xff);
}

static void WriteRepeatedCompressedStream(
    byte[] rom,
    int snesAddress,
    int outputLength,
    byte value)
{
    if (outputLength <= 0 || (outputLength & 0x03ff) != 0)
        throw new ArgumentOutOfRangeException(nameof(outputLength));

    int address = snesAddress;
    for (int remaining = outputLength; remaining > 0; remaining -= 0x0400)
    {
        // Long header E7/FF selects command one and length $400.
        WriteSequentialRomByte(rom, ref address, 0xe7);
        WriteSequentialRomByte(rom, ref address, 0xff);
        WriteSequentialRomByte(rom, ref address, value);
    }
    WriteSequentialRomByte(rom, ref address, 0xff);
}

static void WriteSequentialRomByte(byte[] rom, ref int snesAddress, byte value)
{
    rom[SuperMetroidAddressSpace.ToRomOffset(snesAddress)] = value;
    int offset = snesAddress & 0xffff;
    snesAddress = offset == 0xffff
        ? ((((snesAddress >> 16) + 1) & 0xff) << 16) | 0x8000
        : snesAddress + 1;
}
}
