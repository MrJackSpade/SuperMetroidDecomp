using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
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
        AssertEqual(draygon.Facing == HorizontalDirection.Right ? SamusPoseId.DraygonGrabbedNeutralRightPose : SamusPoseId.DraygonGrabbedNeutralLeftPose,
            samus.Pose, "Draygon grabs Samus");
        AssertTrue(runtime.Enemies.LastEnemyFrame.Has<PoseRequestsDiscarded>(),
            "the grab publishes that alpha's pose requests are discarded");
        AssertTrue(samus.CuePose.Named is null, "the grab empties Samus's super-special pose request");
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
        AssertEqual(DraygonPhase.CarrySamus, draygon.Phase, "Draygon carries Samus");
        AssertEqual(leftoverFraction, draygon.SpiralCenter.Center.Y.Fraction, "the grab keeps the previous spiral's Y fraction");
        Console.WriteLine("Draygon grab spiral fraction: the grab keeps the leftover spiral Y fraction.");
    }

    /// <summary>
    /// #2538: a tail whip pauses Draygon's spiral for <see cref="DraygonGrabDefinitions.TailWhipUpdates"/>, 64 updates, as
    /// the game counted 64 down to zero: his tail starts its whip on the pause's first update, and the spiral resumes on
    /// its last.
    /// </summary>
    internal static void VerifyDraygonTailWhipPause()
    {
        var (runtime, samus, draygon) = GrabSamus(spiralYFraction: 0, pendingPose: null);
        DraygonGrab grab = runtime.Enemies.DraygonBehavior.Grab;
        var whip = new AnimationPlayback<DraygonSegment>(DraygonAnimations.StartingWith(draygon.Facing == HorizontalDirection.Right
            ? DraygonSegment.TailFacingRightWhip
            : DraygonSegment.TailFacingLeftWhip), AnimationPlace.StartOfAnimation);
        AssertEqual(64, DraygonGrabDefinitions.TailWhipUpdates, "the pause lasts the 64 updates the game counted");
        AssertTrue(draygon.TailPart.Playback != whip, "the tail shows no whip before the pause");

        draygon.Phase = DraygonPhase.BeginTailWhipWithSamus;
        grab.BeginDraygonTailWhip(draygon, samus);
        AssertEqual(DraygonPhase.TailWhipWithSamus, draygon.Phase, "the pause runs on after its first update");
        AssertEqual(whip, draygon.TailPart.Playback, "the tail starts its whip on the pause's first update");
        for (int update = 2; update < DraygonGrabDefinitions.TailWhipUpdates; update++)
        {
            grab.RunDraygonTailWhip(draygon, samus);
            AssertEqual(DraygonPhase.TailWhipWithSamus, draygon.Phase, $"the spiral still pauses on update {update} of the whip");
        }
        grab.RunDraygonTailWhip(draygon, samus);
        AssertEqual(DraygonPhase.FlailWithSamus, draygon.Phase, "the spiral resumes on the pause's last update");
        Console.WriteLine("Draygon tail whip pause: the whip starts on the pause's first update and the spiral resumes on its 64th.");
    }

    /// <summary>Loads Draygon's room and has Draygon grab a gooped Samus whose pose requests and spiral fraction are given.</summary>
    private static GrabSamusResult GrabSamus(
        ushort spiralYFraction, SamusPoseId? pendingPose)
    {
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Draygon);
        SamusState samus = runtime.Player.Samus;
        samus.Pose = SamusPoseId.MovingLeftNormalPose;
        samus.SlowdownLevel = 1;
        if (pendingPose is { } cuePose)
            samus.CuePose.Name(cuePose);
        else
            samus.CuePose.Forget();
        DraygonEnemyState draygon = runtime.Enemies.Draygon
            ?? throw new InvalidOperationException("Draygon's room loads the Draygon encounter.");
        draygon.Phase = DraygonPhase.TryGrabSamus;
        draygon.SpiralCenter = DraygonRegrabSpiralLift.At(draygon.SpiralCenter.Center.WithFractionY(spiralYFraction));
        // The claw sits eight pixels ahead of the body, on Samus's row.
        int clawOffset = draygon.Facing == HorizontalDirection.Right ? 8 : -8;
        draygon.Body.Position = FixedPosition<RoomPixels>.FromWhole(
            new(checked((ushort)(samus.Position.X.Whole - clawOffset)), samus.Position.Y.Whole));

        runtime.Enemies.DraygonBehavior.Grab.ChaseAndTryToGrabSamus(draygon, samus, frameCounter: 1);
        return new GrabSamusResult(runtime, samus, draygon);
    }

    /// <summary>Runs the Draygon grab pose-request check from the verification command line.</summary>
    [VerificationCommand("--draygon-grab-pose-requests")]
    internal static int DraygonGrabPoseRequestsCommand()
    {
        Suite(nameof(VerifyDraygonGrabPoseRequests), () => VerifyDraygonGrabPoseRequests());
        return 0;
    }

    /// <summary>Runs the Draygon tail-whip pause check from the verification command line.</summary>
    [VerificationCommand("--draygon-tail-whip-pause")]
    internal static int DraygonTailWhipPauseCommand()
    {
        Suite(nameof(VerifyDraygonTailWhipPause), () => VerifyDraygonTailWhipPause());
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
