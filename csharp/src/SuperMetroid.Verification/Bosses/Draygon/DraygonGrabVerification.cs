using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks what Draygon's grab (`$A5:8EA9`, `$90:E23B`) resets and what it leaves alone.</summary>
internal static class DraygonGrabVerification
{
    /// <summary>
    /// `SetSamusIntoTheGrabbedByDraygonPose` ($90:E23B) empties every prospective-pose slot. In the
    /// contributed lowc movie Draygon grabs Samus while she runs left holding up: alpha has already
    /// chosen the running aim-up-left pose, and the port then applied it to the grabbed Samus.
    /// </summary>
    internal static void VerifyDraygonGrabPoseRequests()
    {
        var (runtime, samus, draygon) = GrabSamus(spiralYFraction: 0, pendingPose: SamusPoseId.RunningAimDiagonalUpLeftPose);
        AssertEqual(draygon.FacingRight ? SamusPoseId.DraygonGrabbedNeutralRightPose : SamusPoseId.DraygonGrabbedNeutralLeftPose,
            samus.Pose, "Draygon grabs Samus");
        AssertTrue(runtime.Enemies.LastEnemyFrame.DraygonDiscardedSamusPoseRequests,
            "the grab publishes that alpha's pose requests are discarded");
        AssertTrue(samus.PendingTransitionalPose is null, "the grab empties Samus's super-special pose request");
        Console.WriteLine("Draygon grab pose requests: the grab discards the pose requests alpha chose.");
    }

    /// <summary>
    /// The grab sets the whole spiral center (`$A5:8EDB-$8EE5`) but not its Y fraction (`$7E:7814`),
    /// which the previous spiral of the fight left behind. In the lowc movie a second grab starts with
    /// a fraction of $4000, so Draygon's first lift waits a frame; the port reset it to zero and rose
    /// a frame early (update 76,989).
    /// </summary>
    internal static void VerifyDraygonGrabSpiralFraction()
    {
        const ushort leftoverFraction = 16384;
        var (_, _, draygon) = GrabSamus(spiralYFraction: leftoverFraction, pendingPose: null);
        AssertEqual(DraygonAiFunction.CarrySamus, draygon.Function, "Draygon carries Samus");
        AssertEqual(leftoverFraction, draygon.SpiralCenter.Y.Fraction, "the grab keeps the previous spiral's Y fraction");
        Console.WriteLine("Draygon grab spiral fraction: the grab keeps the leftover spiral Y fraction.");
    }

    /// <summary>Loads Draygon's room and has Draygon grab a gooped Samus whose pose requests and spiral fraction are given.</summary>
    private static (SuperMetroidRuntime Runtime, SamusState Samus, DraygonEnemyState Draygon) GrabSamus(
        ushort spiralYFraction, SamusPoseId? pendingPose)
    {
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Draygon);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.MovingLeftNormalPose;
        samus.XSpeedDivisor = 1;
        samus.PendingTransitionalPose = pendingPose;
        DraygonEnemyState draygon = runtime.Enemies.DraygonBehavior.Draygon
            ?? throw new InvalidOperationException("Draygon's room loads the Draygon encounter.");
        draygon.Function = DraygonAiFunction.TryGrabSamus;
        draygon.SpiralCenter = draygon.SpiralCenter.WithFractionY(spiralYFraction);
        // The claw sits eight pixels ahead of the body, on Samus's row.
        int clawOffset = draygon.FacingRight ? 8 : -8;
        draygon.Body.Position = FixedPosition<RoomPixels>.FromWhole(
            new(checked((ushort)(samus.Position.X.Whole - clawOffset)), samus.Position.Y.Whole));

        runtime.Enemies.DraygonBehavior.ChaseAndTryToGrabSamus(draygon, samus, frameCounterLow: 1);
        return (runtime, samus, draygon);
    }

    /// <summary>Runs the Draygon grab pose-request check from the verification command line.</summary>
    [VerificationCommand("--draygon-grab-pose-requests")]
    internal static int DraygonGrabPoseRequestsCommand()
    {
        Suite(nameof(VerifyDraygonGrabPoseRequests), () => VerifyDraygonGrabPoseRequests());
        return 0;
    }

    /// <summary>Runs the Draygon grab spiral-fraction check from the verification command line.</summary>
    [VerificationCommand("--draygon-grab-spiral-fraction")]
    internal static int DraygonGrabSpiralFractionCommand()
    {
        Suite(nameof(VerifyDraygonGrabSpiralFraction), () => VerifyDraygonGrabSpiralFraction());
        return 0;
    }
}
