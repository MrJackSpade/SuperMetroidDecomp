using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using System.Reflection;

internal static partial class Program
{
    private static void VerifyIntroPoseHistory()
    {
        VerifyIntroScenePoseHistory();
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Pose = SamusPoseIds.FacingLeftNormalPose, XPosition = 128, YPosition = 235 };
        var entries = new ushort[16 * 32];
        for (int x = 0; x < 16; x++) entries[16 * 16 + x] = 0x8000;
        var room = CreateRoom(16, 32, entries, new byte[entries.Length]);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var history = samus.PoseHistory;
        history.PreviousPose = samus.Pose;
        history.PreviousDirectionAndMovement = 4;
        history.LastDifferentPose = SamusPoseIds.SpinJumpRightPose;
        history.LastDifferentDirectionAndMovement = 0x0308;
        IntroSamusDemoMovement.StepGroundedLeft(bus, room, samus, 0, 0, 0);
        AssertEqual(SamusPoseIds.SpinJumpRightPose, history.LastDifferentPose, "idle intro frame does not shift history");
        AssertEqual(0x0308, history.LastDifferentDirectionAndMovement, "idle intro retains older metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "idle intro retains previous pose");
        AssertEqual(4, history.PreviousDirectionAndMovement, "idle intro retains previous metadata");
        IntroSamusDemoMovement.StepGroundedLeft(bus, room, samus, (ushort)SnesButton.Left, (ushort)SnesButton.Left, 1);
        AssertEqual(SamusPoseIds.MovingLeftNormalPose, samus.Pose, "intro fixture starts running left");
        AssertEqual(SamusPoseIds.FacingLeftNormalPose, history.LastDifferentPose, "intro run transition shifts prior pose");
        AssertEqual(4, history.LastDifferentDirectionAndMovement, "intro run transition shifts prior metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "intro run transition commits current pose");
        AssertEqual(0x0104, history.PreviousDirectionAndMovement, "intro run transition commits running metadata");
        samus.HorizontalSpeed.BaseSpeed = 2;
        history.LastDifferentPose = SamusPoseIds.SpinJumpRightPose;
        history.LastDifferentDirectionAndMovement = 0x0308;
        IntroSamusDemoMovement.StepGroundedLeft(bus, room, samus, 0, 0, 2);
        AssertEqual(SamusPoseIds.MovingLeftNormalPose, samus.Pose, "intro release fixture retains running during deceleration");
        AssertEqual(samus.Pose, history.LastDifferentPose, "intro same-pose fallback shifts history");
        AssertEqual(0x0104, history.LastDifferentDirectionAndMovement, "intro same-pose fallback shifts running metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "intro same-pose fallback commits current pose");
        AssertEqual(0x0104, history.PreviousDirectionAndMovement, "intro same-pose fallback commits current metadata");
    }

    private static void VerifyIntroScenePoseHistory()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var intro = new IntroCinematicState(bus);
        T? ReadOwner<T>(string name) where T : class =>
            (T?)typeof(IntroCinematicState).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(intro);
        var failures = new List<string>();
        int transitions = 0;
        bool setup = false, terminal = false, handoff = false;
        for (int tick = 0; tick < 20000 && !handoff; tick++)
        {
            SamusState? before = ReadOwner<SamusState>("flashbackSamus");
            byte? oldPose = before?.Pose;
            ushort priorPose = before?.PoseHistory.PreviousPose ?? 0;
            ushort priorMetadata = before?.PoseHistory.PreviousDirectionAndMovement ?? 0;
            ushort olderPose = before?.PoseHistory.LastDifferentPose ?? 0;
            ushort olderMetadata = before?.PoseHistory.LastDifferentDirectionAndMovement ?? 0;
            bool demoEnabled = ReadOwner<DemoInputState>("flashbackDemoInput")?.Enabled == true;
            bool hurtExpiry = before is { KnockbackTimer: 0, KnockbackDirection: not 0 };
            intro.Render();
            intro.Step(tick % 47 == 0 ? (ushort)SnesButton.A : (ushort)0);
            SamusState? after = ReadOwner<SamusState>("flashbackSamus");
            IntroBabyDiscoveryState? discovery = ReadOwner<IntroBabyDiscoveryState>("babyDiscovery");
            bool ended = demoEnabled && ReadOwner<DemoInputState>("flashbackDemoInput")?.Enabled == false;
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
                context = ended ? "terminal demo command" : $"flashback pose {oldPose:X2}->{after.Pose:X2}";
            }
            else if (after is not null && hurtExpiry)
                context = "same-pose hurt expiry command";
            if (after is null) continue;
            if (context is null)
            {
                var unchanged = after.PoseHistory;
                if (unchanged.PreviousPose != priorPose || unchanged.PreviousDirectionAndMovement != priorMetadata ||
                    unchanged.LastDifferentPose != olderPose || unchanged.LastDifferentDirectionAndMovement != olderMetadata)
                    failures.Add($"tick {tick}: ordinary flashback frame unexpectedly shifted history");
                continue;
            }
            var history = after.PoseHistory;
            ushort metadata = (ushort)(after.ReadPoseXDirection(bus) | ((byte)after.ReadMovementType(bus) << 8));
            if (history.LastDifferentPose != priorPose || history.LastDifferentDirectionAndMovement != priorMetadata ||
                history.PreviousPose != after.Pose || history.PreviousDirectionAndMovement != metadata)
                failures.Add($"tick {tick}: {context} did not perform the native four-word history shift");
        }
        AssertTrue(setup && terminal && handoff && transitions == 9, $"intro history fixture covers hurt recovery, return run/jump/landing, terminal and discovery; transitions={transitions}");
        AssertEqual(0, failures.Count, "intro scene history: " + string.Join("; ", failures));
    }
}
