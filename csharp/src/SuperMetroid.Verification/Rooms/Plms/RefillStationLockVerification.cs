using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Samus;

/// <summary>Checks the refill-station script lock that stops Samus's animation until release.</summary>
internal static class RefillStationLockVerification
{
    /// <summary>Confirms command six installs the stationary beta and holds animation during gameplay updates.</summary>
    internal static void VerifyRefillStationLock()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        for (int frame = 0; frame < 8; frame++)
            runtime.StepFrame(0);

        (int Frame, ushort Timer) Animation() => (samus.AnimationFrame, samus.AnimationFrameTimer.RawValue);
        var unlocked = Animation();
        runtime.StepFrame(0);
        AssertTrue(Animation() != unlocked, "the normal beta advances Samus's animation");

        samus.Control.LockIntoRefillStation();
        AssertTrue(samus.Control.Mode.Beta == SamusBetaHandler.RefillStation && samus.Control.AlphaLocked && samus.Control.Mode.Beta != SamusBetaHandler.ScriptLocked,
            "command six installs the station pair, not command zero's");
        var locked = Animation();
        for (int frame = 0; frame < 4; frame++)
            runtime.StepFrame(0);
        AssertEqual(locked, Animation(), "the station beta does not animate Samus");

        samus.Control.Unlock();
        AssertTrue(samus.Control.Mode.Beta != SamusBetaHandler.RefillStation, "restoring the normal handlers releases the station beta");
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
