using System.Reflection;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that X-ray suspends Tourian statue animation timers until the scope closes.</summary>
internal static class TourianStatueXrayFreezeVerification
{
    /// <summary>
    /// Confirms the bank-$87 animated-tile objects stop advancing during X-ray and resume
    /// afterward; their paused timers account for the recorded delayed statue unlock.
    /// </summary>
    internal static void VerifyTourianStatueXrayFreeze()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            runtime.System.SetBossBits(area, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(0xa66a);
        AssertTrue(runtime.TourianStatues.Enabled, "the statue room runs the statue sequence");
        runtime.Player.IntroducedSamus.InputLocked = true;

        var objects = runtime.TourianStatues.objects;
        string Snapshot() => string.Join(" ", objects.Select(o => $"{o.Cursor}/{o.Timer.RawValue}"));

        runtime.StepFrame(0);
        runtime.Player.IntroducedSamus.Xray.SuspendedSubsystems = XraySuspendedSubsystems.All;
        string frozen = Snapshot();
        for (int frame = 0; frame < 20; frame++)
            runtime.StepFrame(0);
        AssertEqual(frozen, Snapshot(), "the X-ray scope freezes every statue program");

        runtime.Player.IntroducedSamus.Xray.SuspendedSubsystems = XraySuspendedSubsystems.None;
        runtime.StepFrame(0);
        AssertTrue(frozen != Snapshot(), "the statues resume once the scope re-enables animated tiles");
        Console.WriteLine("Tourian statue X-ray freeze: the scope suspends the statues' animated-tile programs.");
    }

    /// <summary>Runs the focused statue timer suspension check.</summary>
    [VerificationCommand("--tourian-statue-xray-freeze")]
    internal static int TourianStatueXrayFreezeCommand()
    {
        VerifyTourianStatueXrayFreeze();
        return 0;
    }
}
