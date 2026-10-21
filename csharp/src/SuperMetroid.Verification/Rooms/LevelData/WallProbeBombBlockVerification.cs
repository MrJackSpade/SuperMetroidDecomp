using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
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
            var foreground = new ushort[width * height];
            for (int y = 1; y < 6; y++)
                foreground[y * width + 4] = (int)RoomCollisionType.BombableBlock << 12;
            RoomLevelData level = CreateRoom(new(width, height), foreground, new byte[foreground.Length]);
            var samus = new SamusState { Pose = pose };
            SamusKinematicsState state = samus.Kinematics;
            state.Position = state.Position.WithWholeX(58).WithFractionX(fraction).WithWholeY(48);
            state.Radius = new(5, 5);

            BlockMoveResult result = SamusBlockCollision.ProbeWallHorizontal(level, state, samus, 8 << 16);
            string context = $"wall probe in pose ${(int)pose:X2}";
            AssertEqual(!breaks, result.Collided, $"{context} collision");
            AssertEqual(breaks ? fraction : (ushort)0xffff, state.Position.X.Fraction, $"{context} X fraction");
            AssertEqual((ushort)58, state.Position.X.Whole, $"{context} keeps whole-pixel X");
            AssertEqual(breaks, level.GetCollisionBlock(new(4, 3)).CollisionType != RoomCollisionType.BombableBlock,
                $"{context} bomb-block break");
        }
        Console.WriteLine("Wall-probe bomb block: the wall-jump probe breaks bomb blocks for a screw attack, as movement does.");
    }

    /// <summary>Runs the focused wall-probe bomb-block verification command.</summary>
    [VerificationCommand("--wall-probe-bomb-block")]
    internal static int WallProbeBombBlockCommand()
    {
        VerifyWallProbeBombBlock();
        return 0;
    }
}
