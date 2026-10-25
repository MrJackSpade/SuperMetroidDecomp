using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Enemies.Beetom;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.Artwork;

/// <summary>Checks the Beetom's typed crawl, hop and drain animations and its distant-action choice.</summary>
internal static class BeetomInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge oracle and runs the standalone Beetom instruction suite.</summary>
    internal static void VerifyBeetomInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// The real initializer starts the left crawl; each crawl and drain program runs into its loop and repeats; each hop
    /// shows its poses and holds the last; the crawls stop and the hops start off-screen processing; the programs show
    /// exactly the installed Beetom frames, with optional installed artwork; the program oracle matches bank $A8.
    /// </summary>
    internal static void VerifyBeetomInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom, InstalledEnemyArtwork? artwork = null)
    {
        Suite(nameof(VerifyBeetomNativeMaps), () => VerifyBeetomNativeMaps(rom));
        Suite(nameof(VerifyBeetomDistantActionMapping), () => VerifyBeetomDistantActionMapping(rom));
        Suite(nameof(VerifyBeetomDistantDirectionMapping), () => VerifyBeetomDistantDirectionMapping());

        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { TileArtwork = artwork?.Enemies, BossArtwork = artwork?.Bosses, AreaArtwork = artwork?.Areas };
        enemies.VerificationServices().SeedRandom = _ => { };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Beetom, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.Position = slot.Position.WithWholeX(0x0200);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0100, 0)) };
        enemies.BeetomBehavior.InitializeBeetom(slot, samus, 0);
        BeetomEnemyState state = enemies.BeetomStates[0] ?? throw new InvalidOperationException("Verification requires enemies.BeetomStates[0].");
        object?[] processArguments = [slot, null, null, Position16<RoomPixels>.Zero, SnesButton.None];

        AssertEqual(AnimationProgramCursor<BeetomProgram>.Start(BeetomProgram.CrawlingLeft), state.Program,
            "real Beetom initializer installs the left crawl");
        RunBeetomLoop(processArguments, slot, state, BeetomProgram.CrawlingLeftLoop, callsThroughGoto: 5);
        AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen), "left crawl stops off-screen processing");

        BeetomBehavior.StartBeetomCrawling(slot, state, HorizontalDirection.Right);
        AssertEqual(AnimationProgramCursor<BeetomProgram>.Start(BeetomProgram.CrawlingRight), state.Program, "right crawl is installed");
        RunBeetomLoop(processArguments, slot, state, BeetomProgram.CrawlingRightLoop, callsThroughGoto: 5);
        AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen), "right crawl stops off-screen processing");

        foreach ((bool left, BeetomProgram hop) in new[] { new BeetomInstructionProgramDefinitionsVerifyBeetomInstructionProgramDefinitionsCase(true, BeetomProgram.HopLeft), new BeetomInstructionProgramDefinitionsVerifyBeetomInstructionProgramDefinitionsCase(false, BeetomProgram.HopRight) })
        {
            BeetomBehavior.StartBeetomHop(slot, state, 0, left ? HorizontalDirection.Left : HorizontalDirection.Right, BeetomHopLength.Short);
            AssertEqual(AnimationProgramCursor<BeetomProgram>.Start(hop), state.Program, $"{hop} is installed");
            RunBeetomCalls(processArguments, slot, 5);
            AnimationProgramCursor<BeetomProgram> held = state.Program;
            RunBeetomCalls(processArguments, slot, 2);
            AssertEqual(held, state.Program, $"{hop} holds its last pose");
            AssertEqual(BeetomAnimationsConstants.Spritemaps(hop).Last(), slot.SpritemapPointer, $"{hop} ends on its last pose");
            AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen), $"{hop} starts off-screen processing");
        }

        foreach ((bool left, BeetomProgram drain, BeetomProgram loop) in new[]
                 { new VerifyBeetomInstructionProgramDefinitionsCase(true, BeetomProgram.DrainingLeft, BeetomProgram.DrainingLeftLoop), new VerifyBeetomInstructionProgramDefinitionsCase(false, BeetomProgram.DrainingRight, BeetomProgram.DrainingRightLoop) })
        {
            BeetomBehavior.StartBeetomDrain(slot, state, left ? HorizontalDirection.Left : HorizontalDirection.Right);
            AssertEqual(AnimationProgramCursor<BeetomProgram>.Start(drain), state.Program, $"{drain} is installed");
            RunBeetomLoop(processArguments, slot, state, loop, callsThroughGoto: 9);
        }

        AssertTrue(BeetomAnimationsConstants.Spritemaps(Enum.GetValues<BeetomProgram>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.Name.StartsWith("beetom_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Beetom's programs show exactly its installed frames");
        Suite(nameof(BeetomAnimationsVerification.VerifyBeetomAnimations), () => BeetomAnimationsVerification.VerifyBeetomAnimations(rom));

        Console.WriteLine(artwork is null
            ? "Beetom: the real initializer, both crawl loops, both held hops and both drain loops with their off-screen " +
              "cues pass."
            : "Installed Beetom: all six crawl, hop, and drain programs run with the installed artwork.");
    }

    /// <summary>Initializes a Beetom with an injected RNG so distant-action selection can be checked for each seed.</summary>
    private static CreateBeetomDistantSelectionFixtureResult
        CreateBeetomDistantSelectionFixture(Func<ushort> nextRandom)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = _ => { };
        enemies.VerificationServices().NextRandom = nextRandom;
        Action<RoomEnemySlot, SamusState, SnesButton> initialize = enemies.BeetomBehavior.InitializeBeetom;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Beetom, slot.Definition);
        initialize(slot, new SamusState(), 0);
        Action<BeetomEnemyState> choose = enemies.BeetomBehavior.ChooseDistantBeetomAction;
        return new CreateBeetomDistantSelectionFixtureResult(enemies, (enemies.BeetomStates[0] ?? throw new InvalidOperationException("Verification requires the initialized Beetom state.")), choose);
    }

    /// <summary>Checks every RNG word against the native eight-pointer action table and verifies one random consumption per choice.</summary>
    private static void VerifyBeetomDistantActionMapping(SuperMetroidAddressSpace rom)
    {
        ushort random = 0;
        int randomCalls = 0;
        var fixture = CreateBeetomDistantSelectionFixture(() => { randomCalls++; return random; });
        // The original eight pointer words at A8:B74E; high RNG bits are masked by B839.
        // Native short-hop labels are swapped; compare pointer identities, not label spelling.
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            random = (ushort)value;
            fixture.State.Function = BeetomEnemyFunction.DecideActionSamusNotInProximity;
            fixture.State.FunctionTimer = WrappingCountdown.Start(0x1234);
            AnimationProgramCursor<BeetomProgram> program = fixture.State.Program;
            fixture.Choose(fixture.State);
            ushort expected = ReadBeetomInstructionWord(rom, (ushort)(0xb74e + 2 * (value & 7)));
            AssertEqual(expected, BeetomNativeWords.BeetomEnemyFunctions.Word(fixture.State.Function ?? throw new InvalidOperationException("Beetom chose no action.")), "Beetom distant action matches native pointer for every RNG word");
            AssertEqual((ushort)0x1234, fixture.State.FunctionTimer.RawValue, "Beetom choice preserves timer until setup runs");
            AssertEqual(program, fixture.State.Program, "Beetom choice preserves program until setup runs");
            AssertEqual(value + 1, randomCalls, "Beetom consumes exactly one random value per choice");
        }
    }

    /// <summary>Confirms the selected travel direction uses the seed's low bit independently of the chosen action.</summary>
    private static void VerifyBeetomDistantDirectionMapping()
    {
        ushort random = 0;
        var fixture = CreateBeetomDistantSelectionFixture(() => random);
        // A8:B844-B84A reloads that same seed, masks bit zero and stores direction.
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            random = (ushort)value;
            bool expectedFacingRight = value % 2 == 1;
            fixture.State.FacingRight = !expectedFacingRight;
            fixture.Choose(fixture.State);
            AssertEqual(expectedFacingRight, fixture.State.FacingRight, "Beetom facing preserves native low-bit mapping independently of selected action");
        }
    }
    /// <summary>Checks the native entry counts of the Beetom's five- and six-entry maps.</summary>
    private static void VerifyBeetomNativeMaps(SuperMetroidAddressSpace rom)
    {
        foreach (ushort pointer in new ushort[] {0xbed3,0xbeee,0xbf09,0xbf24,0xbf3f,0xbf5a,0xbf75,0xbf90,
            0xc00b,0xc026,0xc041,0xc05c,0xc077,0xc092,0xc0ad,0xc0c8})
            AssertEqual((ushort)5, ReadBeetomInstructionWord(rom, pointer), "Beetom five-entry native map");
        foreach (ushort pointer in new ushort[] {0xbfab,0xbfcb,0xbfeb,0xc0e3,0xc103,0xc123})
            AssertEqual((ushort)6, ReadBeetomInstructionWord(rom, pointer), "Beetom six-entry native map");
    }

    /// <summary>Forces the requested number of animation updates.</summary>
    private static void RunBeetomCalls(object?[] arguments, RoomEnemySlot slot, int calls)
    {
        for (int call = 0; call < calls; call++)
        {
            slot.InstructionTimer = WrappingCountdown.Start(1);
            EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Advances a crawl or drain program through its loop's restart and checks it shows the loop's first pose again.</summary>
    private static void RunBeetomLoop(object?[] arguments,
        RoomEnemySlot slot,
        BeetomEnemyState state,
        BeetomProgram loop,
        int callsThroughGoto)
    {
        RunBeetomCalls(arguments, slot, callsThroughGoto);
        AssertEqual(new AnimationProgramCursor<BeetomProgram>(loop, 1), state.Program,
            $"Beetom {loop} restarts and shows its first pose");
        AssertEqual(BeetomAnimationsConstants.Spritemaps(loop).First(), slot.SpritemapPointer, $"Beetom {loop} first pose");
    }

    /// <summary>Reads a native Beetom instruction word from bank A8 solely for comparison with compiled definitions.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Beetom instruction words")]
    internal static ushort ReadBeetomInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa80000 | address);

    /// <summary>Runs the retail Beetom instruction suite from the verification command line.</summary>
    [VerificationCommand("--beetom-instruction-mechanics")]
    internal static int BeetomInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions());
        return 0;
    }
}
