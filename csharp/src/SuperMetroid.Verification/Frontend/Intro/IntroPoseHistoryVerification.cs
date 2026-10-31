using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Runtime.Attract.AttractCaptureTestsVerification;
using static SuperMetroid.Verification.Frontend.FrontendRenderCaptureTestsVerification;
using static SuperMetroid.Verification.Runtime.GameplayCaptureIntegrationTestsVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks Samus pose history publication during intro flashback and discovery.</summary>
internal static class IntroPoseHistoryVerification
{
    /// <summary>Checks idle, run, and same-pose history shifts after exercising the full intro scene.</summary>
    internal static void VerifyIntroPoseHistory()
    {
        Suite(nameof(VerifyIntroScenePoseHistory), () => VerifyIntroScenePoseHistory());
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Pose = SamusPoseId.FacingLeftNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 235)) };
        var entries = new RoomTerrainBlock[16 * 32];
        for (int x = 0; x < 16; x++) entries[16 * 16 + x] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
        var room = CreateRoom(new(16, 32), entries, new RoomBlockBehavior[entries.Length]);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        var history = samus.PoseHistory;
        var random = new SharedRandom();
        history.PreviousPose = samus.Pose;
        history.PreviousMovement = SamusMovementType.Standing;
        history.LastDifferentPose = SamusPoseId.SpinJumpRightPose;
        history.LastDifferentMovement = SamusMovementType.SpinJumping;
        IntroSamusDemoMovement.StepGroundedLeft(room, samus, 0, 0, 0, random);
        AssertEqual(SamusPoseId.SpinJumpRightPose, history.LastDifferentPose, "idle intro frame does not shift history");
        AssertEqual(SamusMovementType.SpinJumping, history.LastDifferentMovement, "idle intro retains older metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "idle intro retains previous pose");
        AssertEqual(SamusMovementType.Standing, history.PreviousMovement, "idle intro retains previous metadata");
        IntroSamusDemoMovement.StepGroundedLeft(room, samus, ControllerButton.Left, ControllerButton.Left, 1, random);
        AssertEqual(SamusPoseId.MovingLeftNormalPose, samus.Pose, "intro fixture starts running left");
        AssertEqual(SamusPoseId.FacingLeftNormalPose, history.LastDifferentPose, "intro run transition shifts prior pose");
        AssertEqual(SamusMovementType.Standing, history.LastDifferentMovement, "intro run transition shifts prior metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "intro run transition commits current pose");
        AssertEqual(SamusMovementType.Running, history.PreviousMovement, "intro run transition commits running metadata");
        samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(2);
        history.LastDifferentPose = SamusPoseId.SpinJumpRightPose;
        history.LastDifferentMovement = SamusMovementType.SpinJumping;
        IntroSamusDemoMovement.StepGroundedLeft(room, samus, 0, 0, 2, random);
        AssertEqual(SamusPoseId.MovingLeftNormalPose, samus.Pose, "intro release fixture retains running during deceleration");
        AssertEqual(samus.Pose, history.LastDifferentPose, "intro same-pose fallback shifts history");
        AssertEqual(SamusMovementType.Running, history.LastDifferentMovement, "intro same-pose fallback shifts running metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "intro same-pose fallback commits current pose");
        AssertEqual(SamusMovementType.Running, history.PreviousMovement, "intro same-pose fallback commits current metadata");
    }

    /// <summary>Tracks flashback transitions, collision-radius handoff, and preservation of the persistent Samus owner.</summary>
    private static void VerifyIntroScenePoseHistory()
    {
        var intro = CreateRetailIntroFixture();
        var failures = new List<string>();
        int transitions = 0;
        bool setup = false, terminal = false, handoff = false;
        bool jumpRadiusRetained = false, jumpRadiusPublished = false;
        for (int tick = 0; tick < 20000 && !handoff; tick++)
        {
            SamusState? before = intro.CurrentFlashbackSamus;
            SamusPoseId? oldPose = before?.Pose;
            SamusPoseId priorPose = before?.PoseHistory.PreviousPose ?? 0;
            SamusMovementType priorMetadata = before?.PoseHistory.PreviousMovement ?? SamusMovementType.Standing;
            SamusPoseId olderPose = before?.PoseHistory.LastDifferentPose ?? 0;
            SamusMovementType olderMetadata = before?.PoseHistory.LastDifferentMovement ?? SamusMovementType.Standing;
            bool demoEnabled = intro.CurrentFlashbackDemoInput?.Enabled == true;
            bool hurtExpiry = before is { KnockbackTimer.IsZero: true, KnockbackDirection: not 0 };
            intro.Render();
            intro.Step(tick % 47 == 0 ? ControllerButton.A : 0);
            SamusState? after = intro.CurrentFlashbackSamus;
            IntroBabyDiscoveryState? discovery = intro.CurrentBabyDiscovery;
            bool ended = demoEnabled && intro.CurrentFlashbackDemoInput?.Enabled == false;
            string? context = null;
            if (before is null && after is not null) { setup = true; context = "flashback setup"; }
            else if (before is not null && discovery is not null)
            {
                handoff = true;
                after = discovery.Samus;
                context = "discovery handoff";
                if (!ReferenceEquals(before, after)) failures.Add("discovery replaced the native persistent Samus owner");
            }
            else if (after is not null && (oldPose != after.Pose || ended))
            {
                transitions++;
                terminal |= ended;
                context = ended ? "terminal demo command" : $"flashback pose {(int?)oldPose:X2}->{(int)after.Pose:X2}";
            }
            else if (after is not null && hurtExpiry)
                context = "same-pose hurt expiry command";
            if (after is null) continue;
            if (oldPose == SamusPoseId.MovingLeftNormalPose && after.Pose == SamusPoseId.SpinJumpLeftPose)
            {
                AssertEqual(21, after.Kinematics.Radius.Y, "intro jump commit retains running collision radius");
                jumpRadiusRetained = true;
            }
            else if (jumpRadiusRetained && !jumpRadiusPublished && oldPose == SamusPoseId.SpinJumpLeftPose)
            {
                AssertEqual(12, after.Kinematics.Radius.Y, "intro next alpha publishes spin collision radius");
                jumpRadiusPublished = true;
            }
            if (context is null)
            {
                var unchanged = after.PoseHistory;
                if (unchanged.PreviousPose != priorPose || unchanged.PreviousMovement != priorMetadata ||
                    unchanged.LastDifferentPose != olderPose || unchanged.LastDifferentMovement != olderMetadata)
                    failures.Add($"tick {tick}: ordinary flashback frame unexpectedly shifted history");
                continue;
            }
            var history = after.PoseHistory;
            SamusMovementType metadata = SamusPoseMetadata.ReadMovementType(after);
            if (history.LastDifferentPose != priorPose || history.LastDifferentMovement != priorMetadata ||
                history.PreviousPose != after.Pose || history.PreviousMovement != metadata)
                failures.Add($"tick {tick}: {context} did not perform the native four-word history shift");
        }
        AssertTrue(setup && terminal && handoff && transitions == 9, $"intro history fixture covers hurt recovery, return run/jump/landing, terminal and discovery; transitions={transitions}");
        AssertTrue(jumpRadiusRetained && jumpRadiusPublished, "intro fixture covers jump radius publication across alpha/beta");
        AssertEqual(0, failures.Count, "intro scene history: " + string.Join("; ", failures));
    }

    /// <summary>Runs intro pose history and frontend rendering capture fixtures.</summary>
    [VerificationCommand("--frontend-fixtures")]
    internal static int FrontendFixturesCommand()
    {
     VerifyIntroPoseHistory(); VerifyFrontendRenderCapture(); VerifyGameplayCaptureIntegration(); VerifyAttractCapture(); return 0; 
    }
}
