using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that Draygon's grab discards the pose requests Samus chose earlier in the frame.</summary>
internal static class DraygonGrabPoseRequestsVerification
{
    /// <summary>
    /// `SetSamusIntoTheGrabbedByDraygonPose` ($90:E23B) empties every prospective-pose slot. In the
    /// contributed lowc movie Draygon grabs Samus while she runs left holding up: alpha has already
    /// chosen the running aim-up-left pose, and the port then applied it to the grabbed Samus.
    /// </summary>
    internal static void VerifyDraygonGrabPoseRequests()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Draygon);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.MovingLeftNormalPose;
        samus.XSpeedDivisor = 1;
        samus.PendingTransitionalPose = SamusPoseId.RunningAimDiagonalUpLeftPose;
        DraygonEnemyState draygon = runtime.Enemies.DraygonBehavior.Draygon
            ?? throw new InvalidOperationException("Draygon's room loads the Draygon encounter.");
        draygon.Function = DraygonAiFunction.TryGrabSamus;
        // The claw sits eight pixels ahead of the body, on Samus's row.
        int clawOffset = draygon.FacingRight ? 8 : -8;
        draygon.Body.Position = FixedPosition<RoomPixels>.FromWhole(
            new(checked((ushort)(samus.Position.X.Whole - clawOffset)), samus.Position.Y.Whole));

        runtime.Enemies.DraygonBehavior.ChaseAndTryToGrabSamus(draygon, samus, frameCounterLow: 1);

        AssertEqual(draygon.FacingRight ? SamusPoseId.DraygonGrabbedNeutralRightPose : SamusPoseId.DraygonGrabbedNeutralLeftPose,
            samus.Pose, "Draygon grabs Samus");
        AssertTrue(runtime.Enemies.RoomStatus.DraygonDiscardedSamusPoseRequestsThisFrame,
            "the grab publishes that alpha's pose requests are discarded");
        AssertTrue(samus.PendingTransitionalPose is null, "the grab empties Samus's super-special pose request");
        Console.WriteLine("Draygon grab pose requests: the grab discards the pose requests alpha chose.");
    }

    /// <summary>Runs the Draygon grab pose-request check from the verification command line.</summary>
    [VerificationCommand("--draygon-grab-pose-requests")]
    internal static int DraygonGrabPoseRequestsCommand()
    {
        Suite(nameof(VerifyDraygonGrabPoseRequests), () => VerifyDraygonGrabPoseRequests());
        return 0;
    }
}
