using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Reproduces a shinespark crash continuing through Baby Metroid's
/// drain pose until its timer finishes and stands Samus up.</summary>
internal static class DrainedSparkCrashVerification
{
    /// <summary>
    /// Drained-Samus controller zero ($91:E4F8) changes only the pose; the movement-handler word
    /// keeps whatever owns it. A spark crash ($90:D3F3) therefore keeps counting and finishes
    /// ($90:D40D), standing Samus up. In the 13% movie Baby Metroid finishes draining Samus
    /// during her crash and native releases her when the crash timer runs out, sending its echoes
    /// at angles read past the end of the crash-pose table.
    /// </summary>
    internal static void VerifyDrainedSparkCrash()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.FacingLeftNormalPose;
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        // Two calls left in the echo hold after the echoes finish circling Samus.
        samus.Shinespark.Phase = ShinesparkPhase.CrashEchoCircle;
        samus.Shinespark.StartStopTimer = NativeCountdown.Start((ushort)2);
        samus.Drained.LetFall(samus);
        AssertEqual(SamusPoseId.DrainedCrouchingLeftPose, samus.Pose, "controller zero installs the drained pose");

        for (int frame = 0; frame < 3; frame++)
            runtime.StepFrame(0);

        AssertEqual(ShinesparkPhase.Inactive, samus.Shinespark.Phase, "the crash runs out and finishes");
        AssertEqual(SamusPoseId.FacingLeftNormalPose, samus.Pose, "the finished crash stands Samus up");
        AssertEqual(DrainedSamusPhase.Inactive, samus.Drained.Phase, "normal movement replaces the drained pose's handler");
        // $90:D482 indexes its angle table with the drained pose, reading $90:D506/$D507.
        AssertEqual((ushort)0xb6, runtime.Projectiles.Slots[3].Variable, "the first echo takes the overread angle $B6");
        AssertEqual((ushort)0x0a, runtime.Projectiles.Slots[4].Variable, "the second echo takes the overread angle $0A");
        Console.WriteLine("  Drained spark crash: the crash keeps its handler through the drain and stands Samus up.");
    }

    /// <summary>Runs the focused drained-shinespark handoff from the CLI.</summary>
    [VerificationCommand("--drained-spark-crash")]
    internal static int DrainedSparkCrashCommand()
    {
        Suite(nameof(VerifyDrainedSparkCrash), () => VerifyDrainedSparkCrash());
        return 0;
    }
}
