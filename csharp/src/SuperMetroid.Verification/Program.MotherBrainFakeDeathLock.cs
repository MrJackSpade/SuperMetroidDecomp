using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

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
        SamusState samus = runtime.Player.Samus!;
        samus.InputLocked = false;
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain!;
        state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock;
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

    [VerificationCommand("--mother-brain-fake-death-lock")]
    internal static int MotherBrainFakeDeathLockCommand()
    {
        Suite(nameof(VerifyMotherBrainFakeDeathLock), () => VerifyMotherBrainFakeDeathLock());
        return 0;
    }
}
