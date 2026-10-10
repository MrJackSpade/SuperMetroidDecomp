using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// $91:EADE probes a proposed run one pixel along the current pose's facing but picks the
    /// wall pose from the proposed run's direction ($91:EB4E). A right-facing moonwalk ($75)
    /// that proposes a left run therefore stops in left-facing wall pose $8A. In the 13% movie
    /// Samus moonwalks against Mother Brain's arena wall and takes exactly that route.
    /// </summary>
    private static void VerifyMoonwalkRanIntoWall()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Pose = SamusPoseId.MoonwalkAimUpLeftPose, XPosition = 0x00eb, YPosition = 0x00c3 };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        AssertTrue(!samus.IsFacingLeft(bus), "moonwalk pose $75 faces right");

        samus.ApplyRanIntoWallPoseChange(bus, SamusPoseId.RanIntoWallLeftPose);
        AssertEqual(SamusPoseId.RanIntoWallLeftPose, samus.Pose, "the proposed left run stops in wall pose $8A");
        Console.WriteLine("  Moonwalk ran into wall: a right-facing moonwalk can stop in the left-facing wall pose.");
    }
}
