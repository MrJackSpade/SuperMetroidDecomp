using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Rooms.LevelData;
using SuperMetroid.Verification.Harness;

/// <summary>Checks screw-attack wall probing against bombable block collision behavior.</summary>
internal static class WallProbeBombBlockVerification
{
    /// <summary>Confirms the wall-jump probe breaks a bomb block during screw attack and preserves X fraction.</summary>
    /// <remarks>#1269: native probe $94:967F uses the horizontal bomb-block reaction; treating the block as solid wrote $FFFF in the 100% movie.</remarks>
    internal static void VerifyWallProbeBombBlock()
    {
        const int width = 8;
        const int height = 8;
        const ushort fraction = 0x2fff;

        foreach (var (pose, breaks) in new VerifyWallProbeBombBlockCase[]
            { new VerifyWallProbeBombBlockCase(SamusPoseId.ScrewAttackRightPose, true), new VerifyWallProbeBombBlockCase(SamusPoseId.FacingRightNormalPose, false) })
        {
            var foreground = new RoomTerrainBlock[width * height];
            for (int y = 1; y < 6; y++)
                foreground[y * width + 4] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.BombableBlock);
            RoomLevelData level = CreateRoom(new(width, height), foreground, new RoomBlockBehavior[foreground.Length]);
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = pose };
            SamusKinematicsState state = samus.Kinematics;
            state.Position = state.Position.WithWholeX(58).WithFractionX(fraction).WithWholeY(48);
            state.Radius = new(5, 5);

            BlockMoveResult result = SamusBlockCollision.ProbeWallHorizontal(level, state, samus, SignedSixteenSixteen.FromParts(8, 0),
                RoomObjectFixtures.Unbound());
            string context = $"wall probe in pose ${(int)SamusPoseNumbers.Number(pose):X2}";
            AssertEqual(!breaks, result.Collided, $"{context} collision");
            AssertEqual(breaks ? fraction : (ushort)0xffff, state.Position.X.Fraction, $"{context} X fraction");
            AssertEqual((ushort)58, state.Position.X.Whole, $"{context} keeps whole-pixel X");
            AssertEqual(breaks, level.GetCollisionBlock(new(4, 3)).CollisionType != RoomCollisionType.BombableBlock,
                $"{context} bomb-block break");
        }
        VerifyWallProbeBombBlockStartsItsReaction();
        Console.WriteLine("Wall-probe bomb block: the wall-jump probe breaks bomb blocks for a screw attack, as movement does.");
    }

    /// <summary>
    /// A bomb block the wall-jump probe breaks starts its block reaction, as any other contact does, so the block runs its
    /// breakup and grows back (#1582); the probe as the grapple's wall jump calls it.
    /// </summary>
    private static void VerifyWallProbeBombBlockStartsItsReaction()
    {
        const int width = 8;
        const int height = 8;
        var foreground = new RoomTerrainBlock[width * height];
        for (int y = 1; y < 6; y++)
            foreground[y * width + 4] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.BombableBlock);
        RoomLevelData level = CreateRoom(new(width, height), foreground, new RoomBlockBehavior[foreground.Length]);
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.ScrewAttackRightPose,
        };
        SamusKinematicsState state = samus.Kinematics;
        state.Position = state.Position.WithWholeX(58).WithWholeY(48);
        state.Radius = new(5, 5);

        SamusBlockCollision.ProbeWallHorizontal(level, state, samus, SignedSixteenSixteen.FromParts(8, 0),
            roomObjects);
        AssertTrue(roomObjects.SlotTable.ActiveCount > 0, "the probe's broken bomb block starts its block reaction");
        for (int update = 0; update < 600 && roomObjects.SlotTable.ActiveCount != 0; update++)
            roomObjects.Step(level, new RoomObjectFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });
        AssertEqual(0, roomObjects.SlotTable.ActiveCount, "the bomb block's reaction runs to its end");
        AssertEqual(RoomCollisionType.BombableBlock, level.GetCollisionBlock(new(4, 3)).CollisionType,
            "the probe's broken bomb block grows back");
    }

    /// <summary>Runs the focused wall-probe bomb-block verification command.</summary>
    [VerificationCommand("--wall-probe-bomb-block")]
    internal static int WallProbeBombBlockCommand()
    {
        VerifyWallProbeBombBlock();
        return 0;
    }
}
