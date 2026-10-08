using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // #1269: WallJump_Check's block probe ($94:967F) reaches the same horizontal bomb-block
    // reaction as movement, whose setup ($84:CE83) breaks the block for a screw-attacking
    // Samus and reports no collision. In the 100% movie a screw attack turning left beside
    // a bomb block therefore keeps its X fraction; treating the block as solid wrote $FFFF.
    private static void VerifyWallProbeBombBlock()
    {
        const int width = 8;
        const int height = 8;
        const ushort fraction = 0x2fff;
        var bus = new TestAddressSpace();

        foreach (var (pose, breaks) in new (byte, bool)[]
            { (SamusPoseIds.ScrewAttackRightPose, true), (SamusPoseIds.FacingRightNormalPose, false) })
        {
            var foreground = new ushort[width * height];
            for (int y = 1; y < 6; y++)
                foreground[y * width + 4] = (ushort)((int)RoomCollisionType.BombableBlock << 12);
            RoomLevelData level = CreateRoom(width, height, foreground, new byte[foreground.Length]);
            var samus = new SamusState { Pose = pose };
            SamusKinematicsState state = samus.Kinematics;
            state.XPosition = 58;
            state.XSubposition = fraction;
            state.YPosition = 48;
            state.XRadius = 5;
            state.YRadius = 5;

            BlockMoveResult result = SamusBlockCollision.ProbeWallHorizontal(bus, level, state, 8 << 16);
            string context = $"wall probe in pose ${pose:X2}";
            AssertEqual(!breaks, result.Collided, $"{context} collision");
            AssertEqual(breaks ? fraction : (ushort)0xffff, state.XSubposition, $"{context} X fraction");
            AssertEqual((ushort)58, state.XPosition, $"{context} keeps whole-pixel X");
            AssertEqual(breaks, level.GetCollisionBlock(4, 3).CollisionType != RoomCollisionType.BombableBlock,
                $"{context} bomb-block break");
        }
        Console.WriteLine("Wall-probe bomb block: the wall-jump probe breaks bomb blocks for a screw attack, as movement does.");
    }
}
