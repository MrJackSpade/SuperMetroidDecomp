using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks Samus's stationary lock during Mother Brain's fake-death descent.</summary>
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
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        MotherBrainEnemyState state = (runtime.Enemies.MotherBrainEncounter.MotherBrain ?? throw new InvalidOperationException("Verification requires runtime.Enemies.MotherBrainEncounter.MotherBrain."));
        (state ?? throw new InvalidOperationException("Verification requires state.")).Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock;
        state.FunctionTimer = NativeCountdown.Start(0);

        runtime.Enemies.MotherBrainBehavior.RunMotherBrainFakeDeath(state, samus);
        AssertTrue(samus.InputLocked && samus.ScriptLocks.StationaryScriptControlLocked,
            "the descent locks Samus with command zero's stationary handlers");

        state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock;
        state.FunctionTimer = NativeCountdown.Start(0);
        runtime.Enemies.MotherBrainBehavior.RunMotherBrainFakeDeath(state, samus);
        AssertTrue(!samus.InputLocked && !samus.ScriptLocks.StationaryScriptControlLocked,
            "command one restores the normal handlers");
        Console.WriteLine("  Mother Brain fake-death lock: command zero locks Samus, command one frees her.");
    }

    /// <summary>
    /// The fake-death explosions step backward through their eight positions: an expiry from
    /// the first position wraps to the last, then continues to the one before it.
    /// </summary>
    internal static void VerifyMotherBrainFakeDeathExplosionOrder()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain ??
            throw new InvalidOperationException("Verification requires runtime.Enemies.MotherBrainEncounter.MotherBrain.");
        state.Function = MotherBrainBodyFunction.FakeDeathDescentFadeToGray;
        state.FunctionTimer = NativeCountdown.Start(100);
        state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.Finished;
        state.FakeDeathExplosionIndex = 0;

        int[] expectedIndices = [7, 6];
        foreach (int expected in expectedIndices)
        {
            state.FakeDeathExplosionTimer = NativeCountdown.Start(0);
            runtime.Enemies.MotherBrainBehavior.RunMotherBrainFadeToGray(state);
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
