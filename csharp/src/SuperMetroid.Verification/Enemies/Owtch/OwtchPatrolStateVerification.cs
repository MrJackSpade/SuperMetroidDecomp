using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Verification.Enemies.Owtch;
using SuperMetroid.Verification.Harness;

/// <summary>Owtch's typed patrol speeds and burial depth keep the cartridge's carry, wraparound and slot words.</summary>
internal static class OwtchPatrolStateVerification
{
    /// <summary>
    /// Runs the production main AI. A rightward speed of one and a half pixels carries its fraction into the whole
    /// pixel, and a leftward speed of minus one and a half pixels wraps its whole word, exactly as the cartridge's paired
    /// additions do. Sinking counts the depth up to sixteen and then goes underground. Rising from depth one reaches
    /// zero and walks again, while an Owtch that starts rising unburied wraps its depth to 65535 and keeps rising.
    /// </summary>
    internal static void VerifyOwtchPatrolState()
    {
        var frame = new ClockedEnemyAiFrame(
            new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));

        (RoomEnemySystem enemies, RoomEnemySlot slot, OwtchEnemyState state) = NewOwtch(OwtchBehaviorState.MovingRight);
        state.RightVelocity = SnesSignedSixteenSixteen.FromParts(1, 32768);
        slot.Position = FixedPosition<RoomPixels>.FromWhole(new(100, 64)).WithFractionX(49152);
        enemies.OwtchBehavior.RunOwtchMainAi(slot, frame);
        AssertEqual((ushort)102, slot.Position.X.Whole, "Owtch rightward speed carries its fraction into the whole pixel");
        AssertEqual((ushort)16384, slot.Position.X.Fraction, "Owtch rightward fraction after the carry");

        (enemies, slot, state) = NewOwtch(OwtchBehaviorState.MovingLeft);
        state.LeftVelocity = SnesSignedSixteenSixteen.FromParts(-2, 32768);
        slot.Position = FixedPosition<RoomPixels>.FromWhole(new(100, 64)).WithFractionX(16384);
        enemies.OwtchBehavior.RunOwtchMainAi(slot, frame);
        AssertEqual((ushort)98, slot.Position.X.Whole, "Owtch leftward speed wraps its whole word");
        AssertEqual((ushort)49152, slot.Position.X.Fraction, "Owtch leftward fraction without a carry");

        foreach ((int depth, int expectedDepth, OwtchBehaviorState expectedBehavior) in new (int, int, OwtchBehaviorState)[]
                 {
                     (14, 15, OwtchBehaviorState.Sinking),
                     (15, 16, OwtchBehaviorState.Underground),
                 })
        {
            (enemies, slot, state) = NewOwtch(OwtchBehaviorState.Sinking);
            state.BurialDepth = depth;
            enemies.OwtchBehavior.RunOwtchMainAi(slot, frame);
            AssertEqual(expectedDepth, state.BurialDepth, $"Owtch sinking from {depth}");
            AssertEqual(expectedBehavior, state.Behavior, $"Owtch sinking from {depth}: behavior");
        }

        (enemies, slot, state) = NewOwtch(OwtchBehaviorState.Rising);
        state.BurialDepth = 1;
        enemies.OwtchBehavior.RunOwtchMainAi(slot, frame);
        AssertEqual(0, state.BurialDepth, "Owtch rising from depth one surfaces");
        AssertTrue(state.Behavior is OwtchBehaviorState.MovingLeft or OwtchBehaviorState.MovingRight, "Owtch walks again once surfaced");

        (enemies, slot, state) = NewOwtch(OwtchBehaviorState.Rising);
        AssertEqual(0, state.BurialDepth, "Owtch spawned rising starts unburied");
        enemies.OwtchBehavior.RunOwtchMainAi(slot, frame);
        AssertEqual((int)ushort.MaxValue, state.BurialDepth, "Owtch rising from depth zero wraps its depth");
        AssertEqual(OwtchBehaviorState.Rising, state.Behavior, "Owtch keeps rising after the wrap");
        AssertEqual((ushort)63, slot.Position.Y.Whole, "Owtch rose one pixel");

        Console.WriteLine("  Owtch patrol state: speeds carry and wrap, sinking stops at sixteen and rising wraps from zero.");
    }

    /// <summary>An initialized Owtch at room position (100, 64) in <paramref name="behavior"/>, with the first travel distance and speed.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot, OwtchEnemyState State) NewOwtch(OwtchBehaviorState behavior)
    {
        var enemies = new RoomEnemySystem();
        // A walk rolls the sink chance each update; a high roll keeps the Owtch walking.
        enemies.VerificationServices().NextRandom = () => 0x80;
        enemies.VerificationServices().ReadRandom = () => 0x80;
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Owtch, slot.Definition);
        slot.Parameter1 = (ushort)behavior;
        slot.Parameter2 = 0;
        slot.Position = FixedPosition<RoomPixels>.FromWhole(new(100, 64));
        OwtchBehavior.InitializeOwtch(slot);
        return (enemies, slot, OwtchBehavior.RequireOwtchState(slot));
    }

    /// <summary>Runs the Owtch patrol-state check.</summary>
    [VerificationCommand("--owtch-patrol-state")]
    internal static int OwtchPatrolStateCommand()
    {
        Suite(nameof(VerifyOwtchPatrolState), () => VerifyOwtchPatrolState());
        return 0;
    }
}
