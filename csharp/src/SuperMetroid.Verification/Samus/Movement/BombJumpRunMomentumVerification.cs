using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks how a bomb jump carries Samus's existing dash speed into the forward-jump pose.</summary>
internal static class BombJumpRunMomentumVerification
{
    /// <summary>
    /// A bomb jump's special target pose installs through $91:F433, which runs the normal-jump
    /// initializer $91:F543: retained extra run speed selects deceleration mode two. In the 13%
    /// movie a bomb lifts a dashing Samus, and her run speed then decays rather than grows.
    /// </summary>
    internal static void VerifyBombJumpRunMomentum()
    {
        const int width = 8, height = 8;
        var floorBlocks = new RoomTerrainBlock[width * height];
        for (int x = 0; x < width; x++)
            floorBlocks[4 * width + x] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
        var floor = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(floorBlocks, new RoomBlockBehavior[floorBlocks.Length], new RoomBlockAppearance[floorBlocks.Length]));

        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = SamusPoseId.MovingLeftNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(48, 43)) };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(2, 0xc000);
        SamusPoseMetadata.PublishBombJumpDirection(samus, SamusBombJumpDirection.Left);
        AssertTrue(SamusPoseMetadata.TrySetupPublishedBombJump(samus, floor, timeIsFrozen: false, frameCounter: 0, controllerNewInput: 0),
            "a running Samus accepts the bomb jump");
        AssertEqual(SamusPoseId.NormalJumpForwardLeftPose, samus.Pose, "the bomb jump installs pose $52");
        AssertEqual(SamusAccelerationMode.Decelerating, samus.HorizontalSpeed.AccelerationMode,
            "retained extra run speed selects deceleration");
        Console.WriteLine("  Bomb-jump run momentum: the forward-jump pose decelerates retained dash speed.");
    }

    /// <summary>Runs the focused 13% movie dash-to-bomb-jump momentum check from the verification command line.</summary>
    [VerificationCommand("--bomb-jump-run-momentum")]
    internal static int BombJumpRunMomentumCommand()
    {
        Suite(nameof(VerifyBombJumpRunMomentum), () => VerifyBombJumpRunMomentum());
        return 0;
    }
}
