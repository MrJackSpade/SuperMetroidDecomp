using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;

/// <summary>Checks the refill-station script lock that stops Samus's animation until release.</summary>
internal static class RefillStationLockVerification
{
    /// <summary>Confirms command six installs the stationary beta and holds animation during gameplay updates.</summary>
    internal static void VerifyRefillStationLock()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        var samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        for (int frame = 0; frame < 8; frame++)
            runtime.StepFrame(0);

        (ushort Frame, ushort Timer) Animation() => (samus.AnimationFrame, samus.AnimationFrameTimer.RawValue);
        var unlocked = Animation();
        runtime.StepFrame(0);
        AssertTrue(Animation() != unlocked, "the normal beta advances Samus's animation");

        samus.ScriptLocks.LockIntoRefillStation();
        AssertTrue(samus.ScriptLocks.RefillStationLocked && samus.InputLocked && !samus.ScriptLocks.StationaryScriptControlLocked,
            "command six installs the station pair, not command zero's");
        var locked = Animation();
        for (int frame = 0; frame < 4; frame++)
            runtime.StepFrame(0);
        AssertEqual(locked, Animation(), "the station beta does not animate Samus");

        samus.InputLocked = false;
        AssertTrue(!samus.ScriptLocks.RefillStationLocked, "restoring the normal handlers releases the station beta");
        Console.WriteLine("Refill-station lock: command six's RTL beta holds Samus's animation until release.");
    }

    /// <summary>Runs the focused station lock and release check.</summary>
    [VerificationCommand("--refill-station-lock")]
    internal static int RefillStationLockCommand()
    {
        VerifyRefillStationLock();
        return 0;
    }
}
