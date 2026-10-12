using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;

internal static class MoonwalkRanIntoWallVerification
{
    /// <summary>
    /// $91:EADE probes a proposed run one pixel along the current pose's facing but picks the
    /// wall pose from the proposed run's direction ($91:EB4E). A right-facing moonwalk ($75)
    /// that proposes a left run therefore stops in left-facing wall pose $8A. In the 13% movie
    /// Samus moonwalks against Mother Brain's arena wall and takes exactly that route.
    /// </summary>
    internal static void VerifyMoonwalkRanIntoWall()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Pose = SamusPoseId.MoonwalkAimUpLeftPose, Position = FixedPosition<RoomPixels>.FromWhole(new(0x00eb, 0x00c3)) };
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        AssertTrue(!samus.PoseMetadata.IsFacingLeft(bus), "moonwalk pose $75 faces right");

        samus.PoseTransitions.ApplyRanIntoWallPoseChange(bus, SamusPoseId.RanIntoWallLeftPose);
        AssertEqual(SamusPoseId.RanIntoWallLeftPose, samus.Pose, "the proposed left run stops in wall pose $8A");
        Console.WriteLine("  Moonwalk ran into wall: a right-facing moonwalk can stop in the left-facing wall pose.");
    }

    [VerificationCommand("--moonwalk-ran-into-wall")]
    internal static int MoonwalkRanIntoWallCommand()
    {
        Suite(nameof(VerifyMoonwalkRanIntoWall), () => VerifyMoonwalkRanIntoWall());
        return 0;
    }
}
