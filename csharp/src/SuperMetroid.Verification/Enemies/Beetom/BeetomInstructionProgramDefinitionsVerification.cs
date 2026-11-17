using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Beetom.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Beetom;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.World.Artwork;

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
        CartridgeImportAddressSpace rom, ImportEnemyArtwork? artwork = null)
    {
        Suite(nameof(VerifyBeetomNativeMaps), () => VerifyBeetomNativeMaps(rom));
        Suite(nameof(VerifyBeetomDistantActionMapping), () => VerifyBeetomDistantActionMapping(rom));
        Suite(nameof(VerifyBeetomDistantDirectionMapping), () => VerifyBeetomDistantDirectionMapping());

        var enemies = EnemySystemFixtures.New((artwork is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(artwork.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null), installedBosses: artwork?.Bosses);
        enemies.VerificationServices().SeedRandom = _ => { };
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Beetom, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.Position = place.Position.WithWholeX(0x0200);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0100, 0)) };
        enemies.BeetomBehavior.InitializeBeetom(place, samus, 0);
        BeetomEnemyState state = enemies.BeetomStates[0] ?? throw new InvalidOperationException("Verification requires enemies.BeetomStates[0].");
        object?[] processArguments = [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];

        AssertEqual(new AnimationPlayback<BeetomSegment>(BeetomAnimations.StartingWith(BeetomSegment.CrawlingLeft), AnimationPlace.StartOfAnimation), state.Playback,
            "real Beetom initializer installs the left crawl");
        RunBeetomLoop(processArguments, place, state, BeetomSegment.CrawlingLeft, callsThroughGoto: 5);
        AssertTrue(!place.Processing.HasAny(EnemyProcessing.OffScreen), "left crawl stops off-screen processing");

        BeetomBehavior.StartBeetomCrawling(place, state, HorizontalDirection.Right);
        AssertEqual(new AnimationPlayback<BeetomSegment>(BeetomAnimations.StartingWith(BeetomSegment.CrawlingRight), AnimationPlace.StartOfAnimation), state.Playback, "right crawl is installed");
        RunBeetomLoop(processArguments, place, state, BeetomSegment.CrawlingRight, callsThroughGoto: 5);
        AssertTrue(!place.Processing.HasAny(EnemyProcessing.OffScreen), "right crawl stops off-screen processing");

        foreach ((bool left, BeetomSegment hop) in new[] { new BeetomInstructionProgramDefinitionsVerifyBeetomInstructionProgramDefinitionsCase(true, BeetomSegment.HopLeft), new BeetomInstructionProgramDefinitionsVerifyBeetomInstructionProgramDefinitionsCase(false, BeetomSegment.HopRight) })
        {
            BeetomBehavior.StartBeetomHop(place, state, QuadraticSpeedCurvePlace.Start, left ? HorizontalDirection.Left : HorizontalDirection.Right, BeetomHopLength.Short);
            AssertEqual(new AnimationPlayback<BeetomSegment>(BeetomAnimations.StartingWith(hop), AnimationPlace.StartOfAnimation), state.Playback, $"{hop} is installed");
            RunBeetomCalls(processArguments, place, 5);
            AnimationPlayback<BeetomSegment> held = state.Playback;
            RunBeetomCalls(processArguments, place, 2);
            AssertEqual(held, state.Playback, $"{hop} holds its last pose");
            AssertEqual(BeetomAnimationsConstants.Spritemaps(hop).Last(), place.SpritemapPointer, $"{hop} ends on its last pose");
            AssertTrue(place.Processing.HasAny(EnemyProcessing.OffScreen), $"{hop} starts off-screen processing");
        }

        foreach ((bool left, BeetomSegment drain) in new[]
                 { new VerifyBeetomInstructionProgramDefinitionsCase(true, BeetomSegment.DrainingLeft), new VerifyBeetomInstructionProgramDefinitionsCase(false, BeetomSegment.DrainingRight) })
        {
            BeetomBehavior.StartBeetomDrain(place, state, left ? HorizontalDirection.Left : HorizontalDirection.Right);
            AssertEqual(new AnimationPlayback<BeetomSegment>(BeetomAnimations.StartingWith(drain), AnimationPlace.StartOfAnimation), state.Playback, $"{drain} is installed");
            RunBeetomLoop(processArguments, place, state, drain, callsThroughGoto: 9);
        }

        AssertTrue(BeetomAnimationsConstants.Spritemaps(Enum.GetValues<BeetomSegment>()).ToHashSet().SetEquals(
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
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = _ => { };
        enemies.VerificationServices().NextRandom = nextRandom;
        Action<RoomEnemyPlace, SamusState, GameplayAction> initialize = enemies.BeetomBehavior.InitializeBeetom;
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Beetom, place.Definition);
        initialize(place, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), 0);
        Action<BeetomEnemyState> choose = enemies.BeetomBehavior.ChooseDistantBeetomAction;
        return new CreateBeetomDistantSelectionFixtureResult(enemies, (enemies.BeetomStates[0] ?? throw new InvalidOperationException("Verification requires the initialized Beetom state.")), choose);
    }

    /// <summary>Checks every RNG word against the native eight-pointer action table and verifies one random consumption per choice.</summary>
    private static void VerifyBeetomDistantActionMapping(CartridgeImportAddressSpace rom)
    {
        ushort random = 0;
        int randomCalls = 0;
        var fixture = CreateBeetomDistantSelectionFixture(() => { randomCalls++; return random; });
        // The original eight pointer words at A8:B74E; high RNG bits are masked by B839.
        // Native short-hop labels are swapped; compare pointer identities, not label spelling.
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            random = (ushort)value;
            fixture.State.Action = BeetomAction.DecideActionSamusNotInProximity;
            fixture.State.ActionTimer = 0x1234;
            AnimationPlayback<BeetomSegment> program = fixture.State.Playback;
            fixture.Choose(fixture.State);
            ushort expected = ReadBeetomInstructionWord(rom, (ushort)(0xb74e + 2 * (value & 7)));
            AssertEqual(expected, BeetomNativeWords.ActionWord(fixture.State), "Beetom distant action matches native pointer for every RNG word");
            AssertEqual(0x1234, fixture.State.ActionTimer, "Beetom choice preserves timer until setup runs");
            AssertEqual(program, fixture.State.Playback, "Beetom choice preserves program until setup runs");
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
            HorizontalDirection expectedFacing = value % 2 == 1 ? HorizontalDirection.Right : HorizontalDirection.Left;
            fixture.State.Facing = expectedFacing == HorizontalDirection.Right ? HorizontalDirection.Left : HorizontalDirection.Right;
            fixture.Choose(fixture.State);
            AssertEqual(expectedFacing, fixture.State.Facing, "Beetom facing preserves native low-bit mapping independently of selected action");
        }
    }
    /// <summary>Checks the native entry counts of the Beetom's five- and six-entry maps.</summary>
    private static void VerifyBeetomNativeMaps(CartridgeImportAddressSpace rom)
    {
        foreach (ushort pointer in new ushort[] {0xbed3,0xbeee,0xbf09,0xbf24,0xbf3f,0xbf5a,0xbf75,0xbf90,
            0xc00b,0xc026,0xc041,0xc05c,0xc077,0xc092,0xc0ad,0xc0c8})
            AssertEqual((ushort)5, ReadBeetomInstructionWord(rom, pointer), "Beetom five-entry native map");
        foreach (ushort pointer in new ushort[] {0xbfab,0xbfcb,0xbfeb,0xc0e3,0xc103,0xc123})
            AssertEqual((ushort)6, ReadBeetomInstructionWord(rom, pointer), "Beetom six-entry native map");
    }

    /// <summary>Forces the requested number of animation updates.</summary>
    private static void RunBeetomCalls(object?[] arguments, RoomEnemyPlace place, int calls)
    {
        for (int call = 0; call < calls; call++)
        {
            place.AnimationClock.StepOnNextUpdate();
            EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Advances a crawl or drain program through its loop's restart and checks it shows the loop's first pose again.</summary>
    private static void RunBeetomLoop(object?[] arguments,
        RoomEnemyPlace place,
        BeetomEnemyState state,
        BeetomSegment animation,
        int callsThroughGoto)
    {
        RunBeetomCalls(arguments, place, callsThroughGoto);
        AnimationPlayback<BeetomSegment> playback = state.Playback;
        int cycle = playback.Animation.CyclePhase;
        AnimationPhase<BeetomSegment> loop = playback.Animation.Phases[cycle];
        AssertTrue(playback.Place.Phase == cycle && playback.Place.Beat == 1 && ReferenceEquals(playback.Animation, BeetomAnimations.StartingWith(animation)),
            $"Beetom {animation} loops back and shows its loop's first pose");
        var first = AssertIs<AnimationBeat<EnemyFrameId, BeetomCue>.Frame>(BeetomAnimations.Segments.Beats(loop.Segment)[loop.FirstBeat], $"Beetom {animation} loop's first beat");
        AssertEqual(BeetomFrameToolingDefinitions.Spritemap(first.Shown), place.SpritemapPointer, $"Beetom {animation} loop's first pose");
    }

    /// <summary>Reads a native Beetom instruction word from bank A8 solely for comparison with compiled definitions.</summary>
    internal static ushort ReadBeetomInstructionWord(
        CartridgeImportAddressSpace source,
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
