using SuperMetroid.Core.Programs;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Verification.Frontend.CeresCinematics;
using SuperMetroid.Verification.Harness;

/// <summary>Synthetic-map verification of the complete Ceres destruction cinematic and its native transition order.</summary>
internal static class CeresDestructionVerification
{

/// <summary>Advances the Ceres escape aftermath through the state-six handoff while checking map transfers, actor deletion, and phase entry.</summary>
[PpuLayoutBoundary("exact hardware test: checks native PPU geometry, native VRAM words")]
internal static void VerifyCeresDestructionCinematic()
{
    var rom = new byte[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.RetailRomByteCount];

    AssertEqual(SnesAngle.FromTableIndex(0x20), CeresDestructionRomData.Motion.ApproachAngle,
        "Ceres destruction typed approach angle");
    AssertTrue(
        (CeresVramLayout.ObjectCharacters.Offset * 2) +
            CeresDestructionRomData.Vram.Mode7CharacterBytes <= SnesVram.ByteCount,
        "Ceres destruction OBJ transfer fits VRAM");

    // Ceres' five initial explosions share one program but seed instruction timers 1/16/32/48/64.
    // Prove the reusable object keeps that timer distinct from GeneralTimer, which belongs
    // to the program's counted loops and must remain untouched.
    var delayedExplosion = new IntroDiscoverySprite(new(0, 0), 0, CinematicSpriteProgram.CeresSmallExplosion);
    delayedExplosion.DelayFirstInstruction(3);
    delayedExplosion.Step();
    delayedExplosion.Step();
    AssertEqual<CinematicSpritePose?>(null, delayedExplosion.Pose,
        "Ceres staggered explosion remains invisible before instruction delay");
    AssertEqual(0, delayedExplosion.GeneralTimer,
        "instruction delay does not overwrite cinematic goto timer");
    delayedExplosion.Step();
    AssertEqual<CinematicSpritePose?>(AssertIs<AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.ShowStep>(CinematicSpriteAnimations.Programs.Steps(CinematicSpriteProgram.CeresSmallExplosion)[0], "first explosion step").Frame,
        delayedExplosion.Pose, "third handler call shows the delayed explosion's first frame");
    AssertEqual(new AnimationProgramCursor<CinematicSpriteProgram>(CinematicSpriteProgram.CeresSmallExplosion, 1), delayedExplosion.Program,
        "third handler call consumes the first delayed explosion frame");

    var fixtureBus = new CartridgeImportAddressSpace(rom);

    // Preserve the distinctive native map slices through the installed-artwork
    // boundary. Scene programs and all other art retain their retail definitions.
    WriteRepeatedCompressedChunks(rom, CeresDestructionRomData.Assets.CeresTilemaps,
        [0x11, 0x22, 0x33, 0x44]);
    byte[] maps = DecompressSyntheticCeresTilemaps(rom);
    using var overrideRootScratch = new TestScratchDirectory("ceres-timeline");
    string overrideRoot = overrideRootScratch.Root;
    Directory.CreateDirectory(overrideRoot);
    using (var stream = File.Create(Path.Combine(overrideRoot, CeresFlightArtworkFormat.MapFileName)))
        CeresFlightArtworkCatalog.WriteMap(stream, new CeresFlightMapDocument
        {
            Version = CeresFlightArtworkFormat.Version,
            Width = CeresFlightArtworkFormat.MapWidth,
            Height = CeresFlightArtworkFormat.MapHeight,
            FrontTiles = maps.AsSpan(0, 0x300).SelectToArray(value => (int)value),
            RearTiles = maps.AsSpan(0x300, 0x300).SelectToArray(value => (int)value),
        });
    using (var stream = File.Create(Path.Combine(overrideRoot, CeresDestructionArtworkFormat.CeresMapFileName)))
        CeresDestructionArtworkCatalog.WriteMap(stream, new CeresDestructionMapDocument
        {
            Version = CeresDestructionArtworkFormat.Version,
            Width = CeresDestructionArtworkFormat.MapWidth,
            Height = CeresDestructionArtworkFormat.MapHeight,
            Views = Enumerable.Range(0, 3).Select(index => maps.AsSpan(0x600 + index * 0x300, 0x300)
                .SelectToArray(value => (int)value)).ToArray(),
        });
    var artwork = IntroCinematicArtworkFiles.Load(
        RepositoryInstallation.Installation.IntroCinematicDirectory, overrideRoot);
    var presentation = RetailPresentationFixture();
    var state = new CeresDestructionCinematicState(fixedColors: presentation.PowerBombFixedColors, artwork: artwork,
        paletteFxColors: presentation.RoomPaletteFx);
    // $8B:C11B's first dispatch waits for nine NMIs; the ninth resume sets the scene up.
    for (int wait = 0; wait < CeresDestructionRomData.InitialNmiWaits; wait++)
    {
        state.Step();
        AssertTrue(state.ResumesAfterNmiWait, "Ceres initializer resumes after each of its NMI waits");
    }
    state.Step();
    AssertTrue(!state.ResumesAfterNmiWait, "Ceres initializer returns to the main loop after setup");
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

/// <summary>Writes distinct repeated blocks into a synthetic cartridge stream so native work-RAM map slices remain observable.</summary>
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

/// <summary>Encodes a fixed-size repeated-byte compressed stream for synthetic Ceres artwork.</summary>
internal static void WriteRepeatedCompressedStream(
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

/// <summary>Writes one cartridge byte and advances across LoROM bank boundaries.</summary>
static void WriteSequentialRomByte(byte[] rom, ref int snesAddress, byte value)
{
    rom[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.ToRomOffset(snesAddress)] = value;
    int offset = SnesAddressMath.OffsetInBank(snesAddress);
    snesAddress = offset == 0xffff
        ? (int)new SnesAddress(unchecked((byte)(SnesAddressMath.BankNumber(snesAddress) + 1)), 0x8000)
        : snesAddress + 1;
}

/// <summary>Expands the fixture's Ceres tilemap stream for comparison with installed scene map slices.</summary>
[CpuBusBoundary("exact bus test: decompresses the synthetic cartridge's Ceres tilemap stream as the oracle for installed map slices")]
static byte[] DecompressSyntheticCeresTilemaps(byte[] rom) =>
    RomDataReader.Decompress(new CartridgeImportAddressSpace(rom),
        CeresDestructionRomData.Assets.CeresTilemaps, maximumOutputBytes: 0x1000);

    /// <summary>Runs the Ceres destruction cinematic check from the verification host.</summary>
    [VerificationCommand("--ceres-destruction")]
    internal static int CeresDestructionCommand()
    {
        Suite(nameof(VerifyCeresDestructionCinematic), () => VerifyCeresDestructionCinematic());
        return 0;
    }
}
