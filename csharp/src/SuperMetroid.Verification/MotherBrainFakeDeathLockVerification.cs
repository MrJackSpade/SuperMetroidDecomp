using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;

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
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        MotherBrainEnemyState state = (runtime.Enemies.MotherBrainEncounter.MotherBrain ?? throw new InvalidOperationException("Verification requires runtime.Enemies.MotherBrainEncounter.MotherBrain."));
        (state ?? throw new InvalidOperationException("Verification requires state.")).Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock;
        state.FunctionTimer = NativeCountdown.Start(0);

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
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

    /// <summary>Runs the focused fake-death command-zero and command-one lock transition check.</summary>
    [VerificationCommand("--mother-brain-fake-death-lock")]
    internal static int MotherBrainFakeDeathLockCommand()
    {
        Suite(nameof(VerifyMotherBrainFakeDeathLock), () => VerifyMotherBrainFakeDeathLock());
        return 0;
    }
}
