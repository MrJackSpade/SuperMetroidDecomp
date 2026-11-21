using SuperMetroid.Core.Enemies;
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
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        MotherBrainEnemyState state = (runtime.Enemies.BossEncounters.MotherBrain ?? throw new InvalidOperationException("Verification requires runtime.Enemies.BossEncounters.MotherBrain."));
        state.DescentPhase = MotherBrainDescentPhase.PauseBeforeLock;
        state.PhaseTimer.LastFor(1);
        // The original's lock stores to the room's second cell: in Mother Brain's row of four screens, the second screen.
        RoomScrollGrid scrolls = runtime.Room.LoadedCamera.Scrolls;
        AssertEqual(new Extent16<RoomScreens>(4, 1), scrolls.Size, "Mother Brain's room is one row of four screens");
        foreach (Position16<RoomScreens> screen in scrolls.Screens)
            scrolls.SetRoomScreen(screen, RoomScrollState.Open);

        runtime.Enemies.MotherBrainBehavior.RunMotherBrainFakeDeath(state, samus);
        AssertTrue(samus.Control.InputLocked && samus.Control.Mode.Movement == SamusMovementMode.ScriptLocked,
            "the descent locks Samus with command zero's stationary handlers");
        foreach (Position16<RoomScreens> screen in scrolls.Screens)
            AssertEqual(screen == new Position16<RoomScreens>(1, 0) ? RoomScrollState.Closed : RoomScrollState.Open, scrolls.ReadScreen(screen),
                $"the arena lock closes only the second screen, leaving screen {screen} as it was");

        state.DescentPhase = MotherBrainDescentPhase.PauseBeforeUnlock;
        state.PhaseTimer.LastFor(1);
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
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.BossEncounters.MotherBrain ??
            throw new InvalidOperationException("Verification requires runtime.Enemies.BossEncounters.MotherBrain.");
        state.DescentPhase = MotherBrainDescentPhase.FadeToGray;
        state.PhaseTimer.LastFor(101);
        state.TubeCollapse = new MotherBrainTubeCollapseProgress(Tube: null, MotherBrainTubeDropStage.Standing);
        IReadOnlyList<Position16<RoomPixels>> positions = MotherBrainTubeCollapseDefinitions.FakeDeathExplosionPositions;
        state.NextFakeDeathExplosion = SuperMetroid.Core.ListStep<Position16<RoomPixels>>.At(positions, positions.Count - 1);

        // The last position, which the game's place counted back from the first reaches first, is followed by the first.
        int[] expectedNext = [0, 1];
        foreach (int expected in expectedNext)
        {
            state.FakeDeathExplosionTimer.LastFor(1);
            runtime.Enemies.MotherBrainBehavior.FakeDeath.RunMotherBrainFadeToGray(state);
            AssertEqual(expected, state.NextFakeDeathExplosion.Place,
                $"an expired explosion timer moves on to position {expected}");
        }
        Console.WriteLine("  Mother Brain fake-death explosions: the last position is followed by the first.");
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
