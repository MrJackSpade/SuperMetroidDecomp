using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Confirms <c>MakeSamusFaceForward</c> ($91:E3F6) leaves the live collision radius
    /// to the next alpha's SetSamusRadius and lifts both Y words by three when that radius
    /// is not the front view's 24, as when Samus boards an elevator aiming up.
    /// </summary>
    private static void VerifyMakeSamusFaceForward()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Pose = SamusPoseIds.StandingAimUpRightPose, XPosition = 0x7b, YPosition = 0x8b };
        samus.Kinematics.YSubposition = 0xffff;
        samus.RefreshCollisionRadii(bus);
        ushort boardingRadius = samus.Kinematics.YRadius;
        AssertTrue(boardingRadius != 0x18, "aim-up boarding pose has a non-front radius");

        samus.ApplyForwardFacingPoseSetup(bus);
        AssertEqual(SamusPoseIds.ForwardFacingPowerSuitPose, samus.Pose, "power suit faces forward");
        AssertEqual(boardingRadius, samus.Kinematics.YRadius, "face-forward does not write SamusYRadius");
        AssertEqual((ushort)0x88, samus.YPosition, "non-front radius lifts Samus three pixels");
        var previous = new SamusCameraPoint(0x7b, 0, 0x8b, 0xffff);
        AssertEqual(previous with { YPosition = 0x88 }, samus.ApplyPreviousPositionWrites(previous),
            "the lift also writes SamusPreviousYPosition");

        samus.RefreshCollisionRadii(bus);
        samus.ApplyForwardFacingPoseSetup(bus);
        AssertEqual((ushort)0x88, samus.YPosition, "front-view radius 24 skips the lift");
        AssertEqual(previous, samus.ApplyPreviousPositionWrites(previous), "no lift, no previous-Y write");
        Console.WriteLine("  MakeSamusFaceForward: live radius retained; lift and previous-Y write agree.");
    }
}
