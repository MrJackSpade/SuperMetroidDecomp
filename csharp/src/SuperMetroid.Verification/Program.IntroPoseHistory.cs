using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyIntroPoseHistory()
    {
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
}
