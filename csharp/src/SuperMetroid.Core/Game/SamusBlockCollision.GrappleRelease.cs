using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public static partial class SamusBlockCollision
{
    /// <summary>
    /// Native PostGrappleCollisionDetection ($90:EF22), using the vertical dispatcher
    /// at $94:84CD. Call before replacing the connected pose and its collision radius.
    /// </summary>
    public static void EjectAfterGrapple(ISnesAddressSpace bus, RoomLevelData level, SamusKinematicsState state)
    {
        // Native horizontal scans only populate scratch distances: this routine never
        // applies them. These probes do not trigger PLMs, doors, damage, or enemies.
        int up = GrappleVerticalOverlap(bus, level, state, down: true);
        int down = GrappleVerticalOverlap(bus, level, state, down: false);
        if (up == 0 || down != 0) return;
        state.Position -= new Offset16<RoomPixels>(0, unchecked((short)up));
        // Tall bodies may overlap two rows. Retail repeats once, not until clear.
        if (state.Radius.Y >= 16)
            state.Position = state.Position.WithWholeY(unchecked((ushort)(state.Position.Y.Whole - GrappleVerticalOverlap(bus, level, state, down: true))));
    }

    private static int GrappleVerticalOverlap(ISnesAddressSpace bus, RoomLevelData level, SamusKinematicsState state, bool down)
    {
        int boundary = unchecked((ushort)(state.Position.Y.Whole + (down ? state.Radius.Y - 1 : -state.Radius.Y)));
        int left = unchecked((ushort)(state.Position.X.Whole - state.Radius.X));
        int right = unchecked((ushort)(state.Position.X.Whole + state.Radius.X - 1));
        int firstColumn = left >> 4;
        int lastColumn = right >> 4;
        int maximum = 0;
        for (int column = firstColumn; column <= lastColumn; column++)
        {
            RoomCollisionBlock block = level.GetCollisionBlockOrPrefilledSolid(column, boundary >> 4);
            int depth;
            // The post-grapple table treats even vertical extensions as solid, without
            // following BTS. Ordinary movement's dispatcher is not interchangeable.
            if (block.CollisionType >= RoomCollisionType.SolidBlock)
                depth = boundary & 15;
            else if (block.CollisionType != RoomCollisionType.Slope)
                continue;
            else if (block.Bts.IsNonSquareSlope)
            {
                if (column != state.Position.X.Whole >> 4) continue;
                if (block.Bts.SlopeFlipsVertically == down)
                    depth = boundary & 15;
                else
                {
                    int difference = SamusSlopePhysics.ReadAlignmentHeight(bus, block.Bts, state.Position.X.Whole) - (boundary & 15) - 1;
                    depth = down ? (difference <= 0 ? ~difference : -1) : (difference >= 0 ? difference : -1);
                }
            }
            else
            {
                int quadrant = 4 * block.Bts.SlopeShape + (block.Bts.SlopeOrientation ^ ((boundary & 8) >> 2));
                bool selected = SquareSlopeDefinitions.ReadSamusQuadrant(quadrant) != 0;
                bool collides;
                if (column == lastColumn)
                    collides = (right & 8) == 0 ? selected : selected || SquareSlopeDefinitions.ReadSamusQuadrant(quadrant ^ 1) != 0;
                else if (column != firstColumn || (left & 8) == 0)
                    collides = selected || SquareSlopeDefinitions.ReadSamusQuadrant(quadrant ^ 1) != 0;
                else
                    collides = SquareSlopeDefinitions.ReadSamusQuadrant(quadrant ^ 1) != 0;
                depth = collides ? (down ? boundary & 7 : (boundary & 7) ^ 7) : -1;
            }
            maximum = Math.Max(maximum, depth + 1);
        }
        return maximum;
    }
}
