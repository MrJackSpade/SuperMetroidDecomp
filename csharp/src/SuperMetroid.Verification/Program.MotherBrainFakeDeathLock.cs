using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Mother Brain's fake-death descent locks Samus with Samus command zero ($A9:8832), the
    /// stationary lock whose alpha handler places no bombs, and frees her with command one
    /// ($A9:8874). In the 13% movie Samus presses Shoot in morph ball during the lock: native
    /// places no bomb, while the port's plain input lock let one through.
    /// </summary>
    private static void VerifyMotherBrainFakeDeathLock()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain!;
        state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock;
        state.FunctionTimer = 0;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo run = typeof(RoomEnemySystem).GetMethod("RunMotherBrainFakeDeath", flags)!;
        run.Invoke(runtime.Enemies, [state, samus]);
        AssertTrue(samus.InputLocked && samus.StationaryScriptControlLocked,
            "the descent locks Samus with command zero's stationary handlers");

        state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock;
        state.FunctionTimer = 0;
        run.Invoke(runtime.Enemies, [state, samus]);
        AssertTrue(!samus.InputLocked && !samus.StationaryScriptControlLocked,
            "command one restores the normal handlers");
        Console.WriteLine("  Mother Brain fake-death lock: command zero locks Samus, command one frees her.");
    }
}
