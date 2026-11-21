using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.Artwork.CompiledEnemyVisualSelectorsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Boyon.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Boyon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Boyon's idle and bouncing loops, the cues each hands to main AI, and its arc against the original's.</summary>
internal static class BoyonBehaviorVerification
{
    /// <summary>The cartridge's bounce speed multipliers, by spawn selector.</summary>
    private static readonly int[] SelectorMultipliers = [1, 2, 4, 8, 10, 13, 16, 20];

    /// <summary>The spawn selectors of the jump heights a Boyon bounces, 48 through 128 pixels.</summary>
    private const int BouncedHeightSelectors = 6;

    /// <summary>
    /// The highest arc, in 256ths of a pixel, the derivation is checked against the original's word arithmetic for: up to
    /// 128 pixels the original's sign test of the arc's distance against its height is a plain comparison.
    /// </summary>
    private const int PlainComparisonHeight = 128 * 256;

    /// <summary>The largest multiplier the derivation is checked with: up to it, one entry's scaled sample is under 128 pixels.</summary>
    private const int PlainComparisonMultiplier = 128;

    /// <summary>The bounce curve's stored samples, as the original reads them before saturating.</summary>
    private const int StoredSamples = 23;

    /// <summary>
    /// The real initializer starts the idle loop, which stops off-screen processing and loops on time; Samus close by
    /// starts the bounce, which processes off screen, plays the bounce sound, permits the bounce arc and loops on time.
    /// </summary>
    internal static void VerifyBoyonBehavior()
    {
        (RoomEnemySystem idleSystem, RoomEnemyPlace idle, BoyonEnemyState idleState) = Create(parameter1: 0);
        AssertEqual(new AnimationPlayback<BoyonSegment>(BoyonAnimations.StartingWith(BoyonSegment.Idle), AnimationPlace.StartOfAnimation), idleState.Playback, "Boyon initializer starts idling");
        idle.Processing = idle.Processing.With(EnemyProcessing.OffScreen);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BoyonAnimations.Segments, BoyonSegment.IdleRest, BoyonFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(idle, EnemyInstructionRunner.LoopUpdates(BoyonAnimations.Segments, BoyonSegment.IdleRest)), "Boyon idle loop");
        AssertTrue(!idle.Processing.HasAny(EnemyProcessing.OffScreen), "Boyon idle stops off-screen processing");

        (RoomEnemySystem bounceSystem, RoomEnemyPlace bounce, BoyonEnemyState bounceState) = Create(parameter1: 0);
        SamusState samus = SamusAt(bounce);
        BoyonBehavior.RunBoyonMain(bounce.HeldOccupant().SpawnSettings<BoyonSpawnSettings>(), bounce, bounceState, samus);
        BoyonBehavior.RunBoyonMain(bounce.HeldOccupant().SpawnSettings<BoyonSpawnSettings>(), bounce, bounceState, samus);
        AssertEqual(new AnimationPlayback<BoyonSegment>(BoyonAnimations.StartingWith(BoyonSegment.Bouncing), AnimationPlace.StartOfAnimation), bounceState.Playback, "Samus nearby starts the bounce");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BoyonAnimations.Segments, BoyonSegment.BouncingStar, BoyonFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(bounce, EnemyInstructionRunner.LoopUpdates(BoyonAnimations.Segments, BoyonSegment.BouncingStar)), "Boyon bounce loop");
        AssertTrue(bounce.Processing.HasAny(EnemyProcessing.OffScreen), "Boyon bounce processes off screen");
        AssertEqual((SoundEffectId?)BoyonNativeValues.BounceSound,
            bounceSystem.Audio.LatestSound, "Boyon bounce plays its library-two bounce sound");
        AssertTrue(bounceState.IsBouncing, "Boyon bounce permits the movement arc");
        Console.WriteLine("  Boyon behavior: the idle and bounce loops with their off-screen, sound and arc cues pass.");
    }

    /// <summary>
    /// The first update derives the arc's baseline (#1684): for every height of 0 through 128 pixels with each of the
    /// cartridge's eight multipliers, and every multiplier of 1 through 128 with each bounced selector height up to 128
    /// pixels, the production derivation ends at the entry, and with the arc distance, the original's word arithmetic
    /// reaches: the index and distance kept in sixteen bits and the distance compared with the height by the sign of their
    /// difference, which is a plain comparison over that range, past every retail Boyon's arc (#2282). A negative height and
    /// a multiplier of nothing are refused.
    /// </summary>
    internal static void VerifyBoyonArcBaseline(CartridgeImportAddressSpace rom)
    {
        int[] samples = StoredCurve(rom);
        int derivations = 0;
        foreach (int multiplier in SelectorMultipliers)
        for (int height = 0; height <= PlainComparisonHeight; height++)
            derivations += CheckBaseline(samples, multiplier, height);
        for (int multiplier = 1; multiplier <= PlainComparisonMultiplier; multiplier++)
        for (int selector = 0; selector < BouncedHeightSelectors; selector++)
            derivations += CheckBaseline(samples, multiplier, rom.ReadCartridgeWord(0xa286ef + selector * 2));

        AssertThrows<ArgumentOutOfRangeException>(() => BoyonBehavior.DeriveBoyonArcBaseline(new BoyonEnemyState(), 1, SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-1)),
            "a negative arc is refused");
        AssertThrows<ArgumentOutOfRangeException>(() => BoyonBounceCurve.Multiply(BoyonCurvePlace.Top, 0), "a Boyon with multiplier 0 is refused");
        Console.WriteLine($"  Boyon arc baseline: {derivations} derivations end at the original's entry and distance.");
    }

    /// <summary>
    /// Every arc a Boyon bounces with the cartridge's multipliers and its bounced selector heights runs through the
    /// production main update with Samus close by for two whole bounces (#1684): on every update its height, its rise or
    /// fall and its place along the curve match the original's, whose rise leaves the sixteen-bit curve index at its
    /// largest value past the top entry, selects the fall by that value's sign, and whose fall advances it back to the top
    /// entry and ends once the index's signed difference from the baseline is no longer negative.
    /// </summary>
    internal static void VerifyBoyonArcFollowsTheOriginal(CartridgeImportAddressSpace rom)
    {
        int[] samples = StoredCurve(rom);
        int updates = 0;
        for (int multiplierSelector = 0; multiplierSelector < SelectorMultipliers.Length; multiplierSelector++)
        for (int heightSelector = 0; heightSelector < BouncedHeightSelectors; heightSelector++)
        {
            (_, RoomEnemyPlace place, BoyonEnemyState state) = Create(parameter1: (heightSelector << 8) | multiplierSelector);
            SamusState samus = SamusAt(place);
            BoyonBehavior.RunBoyonMain(place.HeldOccupant().SpawnSettings<BoyonSpawnSettings>(), place, state, samus);
            int multiplier = SelectorMultipliers[multiplierSelector];
            (int baseline, _) = OriginalBaseline(samples, multiplier, rom.ReadCartridgeWord(0xa286ef + heightSelector * 2));
            string context = $"Boyon with multiplier {multiplier} and height selector {heightSelector}";
            AssertEqual(BoyonCurvePlaceConstants.AtEntry(baseline), state.BaselineCurvePlace, $"{context}: baseline");

            int index = baseline;
            bool rising = true;
            int y = place.Position.Y.Whole;
            for (int update = 0; update < 4 * (baseline + 1); update++)
            {
                // The original's rise or fall for the next update; a fall that selects the rise has landed, and the next
                // update, Samus still close by, bounces again (#2531).
                BoyonPhase expected;
                if (rising)
                {
                    y = (y - (Sample(samples, index) * multiplier >> 8)) & 0xffff;
                    index = (index - 1) & 0xffff;
                    rising = (index & 0x8000) == 0;
                    expected = rising ? BoyonPhase.Rising : BoyonPhase.Falling;
                }
                else
                {
                    index = (index + 1) & 0xffff;
                    y = (y + (Sample(samples, index) * multiplier >> 8)) & 0xffff;
                    rising = unchecked((short)(index - baseline)) >= 0;
                    expected = rising ? BoyonPhase.Landed : BoyonPhase.Falling;
                }

                BoyonBehavior.RunBoyonMain(place.HeldOccupant().SpawnSettings<BoyonSpawnSettings>(), place, state, samus);
                updates++;
                string at = $"{context}, update {update}";
                AssertEqual(y, (int)place.Position.Y.Whole, $"{at}: height");
                AssertEqual(expected, state.Phase, $"{at}: rise, fall or landing");
                AssertEqual(index == ushort.MaxValue ? BoyonCurvePlaceConstants.PastTheTop : BoyonCurvePlaceConstants.AtEntry(index), state.CurvePlace, $"{at}: place on the curve");
            }
        }
        Console.WriteLine($"  Boyon arc: {updates} bouncing updates follow the original's heights, rises, falls and curve places.");
    }

    /// <summary>Checks one derivation against the original's; returns one.</summary>
    private static int CheckBaseline(int[] samples, int multiplier, int height)
    {
        var state = new BoyonEnemyState();
        BoyonBehavior.DeriveBoyonArcBaseline(state, multiplier, SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(height));
        (int entry, int distance) = OriginalBaseline(samples, multiplier, height);
        // The arc's distance is the derivation's working sum; the entry it ends at is what the Boyon keeps (#2432).
        if (state.BaselineCurvePlace != BoyonCurvePlaceConstants.AtEntry(entry) || state.CurvePlace != state.BaselineCurvePlace ||
            state.Phase != BoyonPhase.Resting)
            throw new InvalidOperationException($"A Boyon with multiplier {multiplier} and height {height} derives {state.BaselineCurvePlace} ({state.Phase}), not entry {entry}, reached at {distance} 256ths.");
        return 1;
    }

    /// <summary>The original derivation: the entry it ends at and the distance it has accumulated, both sixteen-bit.</summary>
    private static (int Entry, int Distance) OriginalBaseline(int[] samples, int multiplier, int height)
    {
        int index = 0;
        int distance = 0;
        do
        {
            distance = (distance + Sample(samples, index) * multiplier) & 0xffff;
            index = (index + 1) & 0xffff;
        }
        while (unchecked((short)(distance - height)) < 0);
        return (index, distance);
    }

    /// <summary>The original's curve sample at <paramref name="index"/>: a stored byte, or 255 from the first index past them.</summary>
    private static int Sample(int[] samples, int index) => index < StoredSamples ? samples[index] : byte.MaxValue;

    /// <summary>The bounce curve's stored samples from the cartridge.</summary>
    private static int[] StoredCurve(CartridgeImportAddressSpace rom) =>
        [.. Enumerable.Range(0, StoredSamples).Select(index => (int)rom.ReadCartridgeByte(0xa28701 + index))];

    /// <summary>Samus standing on the Boyon, within its trigger range.</summary>
    private static SamusState SamusAt(RoomEnemyPlace place) =>
        new(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), artwork: global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(place.Position.Whole),
        };

    /// <summary>A Boyon with a 64-pixel trigger range and the given first parameter after its real initializer.</summary>
    private static BoyonBehaviorCreateResult Create(int parameter1)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Boyon, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        place.SetSpawnParameter1(EnemyDefinitionId.Boyon, parameter1);
        // Parameter two is the horizontal trigger range Samus must be within to start a bounce.
        place.SetSpawnParameter2(EnemyDefinitionId.Boyon, 64);
        place.Position = place.Position.WithWhole(new(256, 512));
        BoyonBehavior.InitializeBoyon(place);
        return new BoyonBehaviorCreateResult(enemies, place, enemies.BoyonStates[0] ?? throw new InvalidOperationException("Boyon has no state."));
    }

    /// <summary>Runs the Boyon behavior and arc checks, its program oracle and the compiled selector check.</summary>
    [VerificationCommand("--lookup-boyon-programs")]
    internal static int LookupBoyonProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBoyonBehavior), () => VerifyBoyonBehavior());
        Suite(nameof(VerifyBoyonArcBaseline), () => VerifyBoyonArcBaseline(rom));
        Suite(nameof(VerifyBoyonArcFollowsTheOriginal), () => VerifyBoyonArcFollowsTheOriginal(rom));
        Suite(nameof(BoyonAnimationsVerification.VerifyBoyonAnimations), () => BoyonAnimationsVerification.VerifyBoyonAnimations(rom));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }
}
