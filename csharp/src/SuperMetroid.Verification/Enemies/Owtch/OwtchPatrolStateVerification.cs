using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Owtch;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Owtch's typed patrol speeds and burial depth keep the cartridge's carry, wraparound and slot words.</summary>
internal static class OwtchPatrolStateVerification
{
    /// <summary>
    /// Runs the production main AI. A rightward speed of one and a half pixels carries its fraction into the whole
    /// pixel, and a leftward speed of minus one and a half pixels wraps its whole word, exactly as the cartridge's paired
    /// additions do. Sinking counts the depth up to sixteen and then goes underground. Rising from depth one reaches
    /// zero and walks again.
    /// </summary>
    internal static void VerifyOwtchPatrolState()
    {
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));
        (RoomEnemySystem enemies, RoomEnemyPlace place, _) = NewOwtch(OwtchBehaviorState.MovingRight);
        // The Owtch walks at its placement's speed (#2439).
        place.SetSpawnSettings(place.SpawnSettings<OwtchSpawnSettings>() with { Speed = SignedSixteenSixteen.FromParts(1, 32768) });
        place.Position = FixedPosition<RoomPixels>.FromWhole(new(100, 64)).WithFractionX(49152);
        enemies.OwtchBehavior.RunOwtchMainAi(place, frame);
        AssertEqual((ushort)102, place.Position.X.Whole, "Owtch rightward speed carries its fraction into the whole pixel");
        AssertEqual((ushort)16384, place.Position.X.Fraction, "Owtch rightward fraction after the carry");

        (enemies, place, _) = NewOwtch(OwtchBehaviorState.MovingLeft);
        place.SetSpawnSettings(place.SpawnSettings<OwtchSpawnSettings>() with { Speed = SignedSixteenSixteen.FromParts(1, 32768) });
        place.Position = FixedPosition<RoomPixels>.FromWhole(new(100, 64)).WithFractionX(16384);
        enemies.OwtchBehavior.RunOwtchMainAi(place, frame);
        AssertEqual((ushort)98, place.Position.X.Whole, "Owtch leftward speed wraps its whole word");
        AssertEqual((ushort)49152, place.Position.X.Fraction, "Owtch leftward fraction without a carry");
        OwtchEnemyState state;
        foreach ((int depth, int expectedDepth, OwtchBehaviorState expectedBehavior) in new VerifyOwtchPatrolStateCase[]
                         {
                     new VerifyOwtchPatrolStateCase(14, 15, OwtchBehaviorState.Sinking),
                     new VerifyOwtchPatrolStateCase(15, 16, OwtchBehaviorState.Underground),
                         })
        {
            (enemies, place, state) = NewOwtch(OwtchBehaviorState.Sinking);
            state.BurialDepth = depth;
            enemies.OwtchBehavior.RunOwtchMainAi(place, frame);
            AssertEqual(expectedDepth, state.BurialDepth, $"Owtch sinking from {depth}");
            AssertEqual(expectedBehavior, state.Behavior, $"Owtch sinking from {depth}: behavior");
        }

        (enemies, place, state) = NewOwtch(OwtchBehaviorState.Rising);
        state.BurialDepth = 1;
        enemies.OwtchBehavior.RunOwtchMainAi(place, frame);
        AssertEqual(0, state.BurialDepth, "Owtch rising from depth one surfaces");
        AssertTrue(state.Behavior is OwtchBehaviorState.MovingLeft or OwtchBehaviorState.MovingRight, "Owtch walks again once surfaced");

        Console.WriteLine("  Owtch patrol state: speeds carry and wrap, sinking stops at sixteen and rising surfaces at zero.");
    }

    /// <summary>An initialized Owtch at room position (100, 64) in <paramref name="behavior"/>, with the first travel distance and speed.</summary>
    private static NewOwtchResult NewOwtch(OwtchBehaviorState behavior)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        // A walk rolls the sink chance each update; a high roll keeps the Owtch walking.
        enemies.VerificationServices().NextRandom = () => 0x80;
        enemies.VerificationServices().ReadRandom = () => 0x80;
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Owtch, place.Definition);
        place.SetSpawnParameter1(EnemyDefinitionId.Owtch, OwtchNativeWords.Behaviors.Word(behavior));
        place.SetSpawnParameter2(EnemyDefinitionId.Owtch, 0);
        place.Position = FixedPosition<RoomPixels>.FromWhole(new(100, 64));
        OwtchBehavior.InitializeOwtch(place);
        return new NewOwtchResult(enemies, place, place.RequireSpeciesState<OwtchEnemyState>());
    }

    /// <summary>Runs the Owtch patrol-state check.</summary>
    [VerificationCommand("--owtch-patrol-state")]
    internal static int OwtchPatrolStateCommand()
    {
        Suite(nameof(VerifyOwtchPatrolState), () => VerifyOwtchPatrolState());
        return 0;
    }
}
