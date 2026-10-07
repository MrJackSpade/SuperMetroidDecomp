using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
static void VerifyCeresDestructionCinematic()
{
    var rom = new byte[SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.RetailRomByteCount];

    AssertEqual(SnesAngle.FromTableIndex(0x20), CeresDestructionRomData.Motion.ApproachAngle,
        "Ceres destruction typed approach angle");
    AssertTrue(
        CeresDestructionRomData.Vram.ObjectCharacterDestinationByte +
            CeresDestructionRomData.Vram.Mode7CharacterBytes <= SnesVram.ByteCount,
        "Ceres destruction OBJ transfer fits VRAM");

    // Keep the delayed-instruction interpreter case synthetic and independent of
    // the compiled cinematic programs used by the complete scene below.
    WriteRomWord(rom, 0x8bccdb, 0x7fff);
    WriteRomWord(rom, 0x8bccdd, 0);
    var fixtureBus = new CartridgeImportAddressSpace(rom);
    ushort ReadFixtureWord(ushort pointer) => (ushort)(
        fixtureBus.ReadByte(0x8b0000 | pointer) |
        fixtureBus.ReadByte(0x8b0000 | unchecked((ushort)(pointer + 1))) << 8);
    // Ceres' five initial explosions share $CCDB but seed instruction timers 1/16/32/48/64.
    // Prove the reusable object keeps that timer distinct from GeneralTimer, which belongs
    // to the list's decrement-and-goto opcode and must remain untouched.
    var delayedExplosion = new IntroDiscoverySprite(0, 0, 0, 0xccdb);
    delayedExplosion.DelayFirstInstruction(3);
    delayedExplosion.Step(fixtureBus, instructionWord: ReadFixtureWord);
    delayedExplosion.Step(fixtureBus, instructionWord: ReadFixtureWord);
    AssertEqual(0, delayedExplosion.SpriteMapPointer,
        "Ceres staggered explosion remains invisible before instruction delay");
    AssertEqual(0, delayedExplosion.GeneralTimer,
        "instruction delay does not overwrite cinematic goto timer");
    delayedExplosion.Step(fixtureBus, instructionWord: ReadFixtureWord);
    AssertEqual(0, delayedExplosion.SpriteMapPointer,
        "fixture's first delayed explosion frame uses invisible map zero");
    AssertEqual(0xccdf, delayedExplosion.InstructionPointer,
        "third handler call consumes the first delayed explosion frame");

    // Preserve the distinctive native map slices through the installed-artwork
    // boundary. Scene programs and all other art retain their retail definitions.
    WriteRepeatedCompressedChunks(rom, CeresDestructionRomData.Assets.CeresTilemaps,
        [0x11, 0x22, 0x33, 0x44]);
    byte[] maps = RomDataReader.Decompress(new CartridgeImportAddressSpace(rom),
        CeresDestructionRomData.Assets.CeresTilemaps, maximumOutputBytes: 0x1000);
    using var overrideRootScratch = new TestTempDirectory("ceres-timeline");
    string overrideRoot = overrideRootScratch.Root;
    Directory.CreateDirectory(overrideRoot);
    using (var stream = File.Create(Path.Combine(overrideRoot, CeresFlightArtworkFormat.MapFileName)))
        CeresFlightArtworkCatalog.WriteMap(stream, new CeresFlightMapDocument
        {
            Version = CeresFlightArtworkFormat.Version,
            Width = CeresFlightArtworkFormat.MapWidth,
            Height = CeresFlightArtworkFormat.MapHeight,
            FrontTiles = maps.AsSpan(0, 0x300).ToArray().Select(value => (int)value).ToArray(),
            RearTiles = maps.AsSpan(0x300, 0x300).ToArray().Select(value => (int)value).ToArray(),
        });
    using (var stream = File.Create(Path.Combine(overrideRoot, CeresDestructionArtworkFormat.CeresMapFileName)))
        CeresDestructionArtworkCatalog.WriteMap(stream, new CeresDestructionMapDocument
        {
            Version = CeresDestructionArtworkFormat.Version,
            Width = CeresDestructionArtworkFormat.MapWidth,
            Height = CeresDestructionArtworkFormat.MapHeight,
            Views = Enumerable.Range(0, 3).Select(index => maps.AsSpan(0x600 + index * 0x300, 0x300)
                .ToArray().Select(value => (int)value).ToArray()).ToArray(),
        });
    var artwork = IntroCinematicArtworkFiles.Load(
        RepositoryInstallation.Installation.IntroCinematicDirectory, overrideRoot);
    var presentation = RetailPresentationFixture();
    var state = new CeresDestructionCinematicState(fixtureBus,
        fixedColors: presentation.PowerBombFixedColors, artwork: artwork,
        paletteFxColors: presentation.RoomPaletteFx);
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
    rom[SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.ToRomOffset(snesAddress)] = value;
    int offset = snesAddress & 0xffff;
    snesAddress = offset == 0xffff
        ? ((((snesAddress >> 16) + 1) & 0xff) << 16) | 0x8000
        : snesAddress + 1;
}
}
