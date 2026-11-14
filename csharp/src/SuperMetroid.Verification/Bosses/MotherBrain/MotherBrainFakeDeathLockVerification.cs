using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Checks Samus's stationary lock during Mother Brain's fake-death descent and its arena's scroll lock.</summary>
internal static class MotherBrainFakeDeathLockVerification
{
    /// <summary>
    /// Mother Brain's fake-death descent locks Samus with Samus command zero ($A9:8832), the
    /// stationary lock whose alpha handler places no bombs, and frees her with command one
    /// ($A9:8874). In the 13% movie Samus presses Shoot in morph ball during the lock: native
    /// places no bomb, while the port's plain input lock let one through.
    /// </summary>
    internal static void VerifyMotherBrainFakeDeathLock()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        MotherBrainEnemyState state = (runtime.Enemies.MotherBrainEncounter.MotherBrain ?? throw new InvalidOperationException("Verification requires runtime.Enemies.MotherBrainEncounter.MotherBrain."));
        state.Phase = MotherBrainBodyPhase.FakeDeathDescentPauseBeforeLock;
        state.PhaseTimer = 0;
        // The original's lock stores to the room's second cell: in Mother Brain's row of four screens, the second screen.
        RoomScrollGrid scrolls = runtime.Room.LoadedCamera.Scrolls;
        AssertEqual(new Extent16<RoomScreens>(4, 1), scrolls.Size, "Mother Brain's room is one row of four screens");
        for (int cell = 0; cell < 4; cell++)
            scrolls.SetCell(cell, RoomScrollState.Open);

        runtime.Enemies.MotherBrainBehavior.RunMotherBrainFakeDeath(state, samus);
        AssertTrue(samus.Control.InputLocked && samus.Control.Mode.Movement == SamusMovementMode.ScriptLocked,
            "the descent locks Samus with command zero's stationary handlers");
        for (int cell = 0; cell < 4; cell++)
            AssertEqual(cell == 1 ? RoomScrollState.Closed : RoomScrollState.Open, scrolls.ReadState(cell),
                $"the arena lock closes only the second screen, leaving cell {cell} as it was");

        state.Phase = MotherBrainBodyPhase.FakeDeathDescentPauseBeforeUnlock;
        state.PhaseTimer = 0;
        runtime.Enemies.MotherBrainBehavior.RunMotherBrainFakeDeath(state, samus);
        AssertTrue(!samus.Control.InputLocked && samus.Control.Mode.Movement != SamusMovementMode.ScriptLocked,
            "command one restores the normal handlers");
        Console.WriteLine("  Mother Brain fake-death lock: command zero locks Samus, command one frees her.");
    }

    /// <summary>
    /// The fake-death explosions step backward through their eight positions: an expiry from
    /// the first position wraps to the last, then continues to the one before it.
    /// </summary>
    internal static void VerifyMotherBrainFakeDeathExplosionOrder()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain ??
            throw new InvalidOperationException("Verification requires runtime.Enemies.MotherBrainEncounter.MotherBrain.");
        state.Phase = MotherBrainBodyPhase.FakeDeathDescentFadeToGray;
        state.PhaseTimer = 100;
        state.TubeCollapseStep = MotherBrainTubeCollapseStep.Finished;
        state.FakeDeathExplosionIndex = 0;

        int[] expectedIndices = [7, 6];
        foreach (int expected in expectedIndices)
        {
            state.FakeDeathExplosionTimer = 0;
            runtime.Enemies.MotherBrainBehavior.FakeDeath.RunMotherBrainFadeToGray(state);
            AssertEqual(expected, state.FakeDeathExplosionIndex,
                $"an expired explosion timer steps back to position {expected}");
        }
        Console.WriteLine("  Mother Brain fake-death explosions: the first position wraps back to the last.");
    }

    /// <summary>Runs the fake-death explosion position order check.</summary>
    [VerificationCommand("--mother-brain-fake-death-explosion-order")]
    internal static int MotherBrainFakeDeathExplosionOrderCommand()
    {
        Suite(nameof(VerifyMotherBrainFakeDeathExplosionOrder), () => VerifyMotherBrainFakeDeathExplosionOrder());
        return 0;
    }

    /// <summary>Runs the focused fake-death command-zero and command-one lock transition check.</summary>
    [VerificationCommand("--mother-brain-fake-death-lock")]
    internal static int MotherBrainFakeDeathLockCommand()
    {
        Suite(nameof(VerifyMotherBrainFakeDeathLock), () => VerifyMotherBrainFakeDeathLock());
        return 0;
    }
}
