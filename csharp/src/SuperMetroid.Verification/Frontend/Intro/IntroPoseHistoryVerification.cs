using static SuperMetroid.Verification.Runtime.Attract.AttractCaptureTestsVerification;
using static SuperMetroid.Verification.Frontend.FrontendRenderCaptureTestsVerification;
using static SuperMetroid.Verification.Runtime.GameplayCaptureIntegrationTestsVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Samus pose history publication during intro flashback and discovery.</summary>
internal static class IntroPoseHistoryVerification
{
    /// <summary>Checks idle, run, and same-pose history shifts after exercising the full intro scene.</summary>
    internal static void VerifyIntroPoseHistory()
    {
        Suite(nameof(VerifyIntroScenePoseHistory), () => VerifyIntroScenePoseHistory());
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Pose = SamusPoseId.FacingLeftNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 235)) };
        var entries = new ushort[16 * 32];
        for (int x = 0; x < 16; x++) entries[16 * 16 + x] = 0x8000;
        var room = CreateRoom(new(16, 32), entries, new byte[entries.Length]);
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        var history = samus.PoseHistory;
        history.PreviousPose = samus.Pose;
        history.PreviousDirectionAndMovement = 4;
        history.LastDifferentPose = SamusPoseId.SpinJumpRightPose;
        history.LastDifferentDirectionAndMovement = 0x0308;
        IntroSamusDemoMovement.StepGroundedLeft(bus, room, samus, 0, 0, 0);
        AssertEqual(SamusPoseId.SpinJumpRightPose, history.LastDifferentPose, "idle intro frame does not shift history");
        AssertEqual(0x0308, history.LastDifferentDirectionAndMovement, "idle intro retains older metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "idle intro retains previous pose");
        AssertEqual(4, history.PreviousDirectionAndMovement, "idle intro retains previous metadata");
        IntroSamusDemoMovement.StepGroundedLeft(bus, room, samus, SnesButton.Left, SnesButton.Left, 1);
        AssertEqual(SamusPoseId.MovingLeftNormalPose, samus.Pose, "intro fixture starts running left");
        AssertEqual(SamusPoseId.FacingLeftNormalPose, history.LastDifferentPose, "intro run transition shifts prior pose");
        AssertEqual(4, history.LastDifferentDirectionAndMovement, "intro run transition shifts prior metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "intro run transition commits current pose");
        AssertEqual(0x0104, history.PreviousDirectionAndMovement, "intro run transition commits running metadata");
        samus.HorizontalSpeed.BaseSpeed = 2;
        history.LastDifferentPose = SamusPoseId.SpinJumpRightPose;
        history.LastDifferentDirectionAndMovement = 0x0308;
        IntroSamusDemoMovement.StepGroundedLeft(bus, room, samus, 0, 0, 2);
        AssertEqual(SamusPoseId.MovingLeftNormalPose, samus.Pose, "intro release fixture retains running during deceleration");
        AssertEqual(samus.Pose, history.LastDifferentPose, "intro same-pose fallback shifts history");
        AssertEqual(0x0104, history.LastDifferentDirectionAndMovement, "intro same-pose fallback shifts running metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "intro same-pose fallback commits current pose");
        AssertEqual(0x0104, history.PreviousDirectionAndMovement, "intro same-pose fallback commits current metadata");
    }

    /// <summary>Tracks flashback transitions, collision-radius handoff, and preservation of the persistent Samus owner.</summary>
    private static void VerifyIntroScenePoseHistory()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var intro = CreateRetailIntroFixture(bus);
        var failures = new List<string>();
        int transitions = 0;
        bool setup = false, terminal = false, handoff = false;
        bool jumpRadiusRetained = false, jumpRadiusPublished = false;
        for (int tick = 0; tick < 20000 && !handoff; tick++)
        {
            SamusState? before = intro.flashbackSamus;
            SamusPoseId? oldPose = before?.Pose;
            SamusPoseId priorPose = before?.PoseHistory.PreviousPose ?? 0;
            ushort priorMetadata = before?.PoseHistory.PreviousDirectionAndMovement ?? 0;
            SamusPoseId olderPose = before?.PoseHistory.LastDifferentPose ?? 0;
            ushort olderMetadata = before?.PoseHistory.LastDifferentDirectionAndMovement ?? 0;
            bool demoEnabled = intro.flashbackDemoInput?.Enabled == true;
            bool hurtExpiry = before is { KnockbackTimer.IsZero: true, KnockbackDirection: not 0 };
            intro.Render();
            intro.Step(tick % 47 == 0 ? SnesButton.A : (ushort)0);
            SamusState? after = intro.flashbackSamus;
            IntroBabyDiscoveryState? discovery = intro.babyDiscovery;
            bool ended = demoEnabled && intro.flashbackDemoInput?.Enabled == false;
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
                if (unchanged.PreviousPose != priorPose || unchanged.PreviousDirectionAndMovement != priorMetadata ||
                    unchanged.LastDifferentPose != olderPose || unchanged.LastDifferentDirectionAndMovement != olderMetadata)
                    failures.Add($"tick {tick}: ordinary flashback frame unexpectedly shifted history");
                continue;
            }
            var history = after.PoseHistory;
            ushort metadata = (ushort)(SnesLittleEndian.Word(after.PoseMetadata.ReadPoseXDirection(bus), (byte)after.PoseMetadata.ReadMovementType(bus)));
            if (history.LastDifferentPose != priorPose || history.LastDifferentDirectionAndMovement != priorMetadata ||
                history.PreviousPose != after.Pose || history.PreviousDirectionAndMovement != metadata)
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
