using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared bank-$A0 enemy movement primitives. These helpers retain 16.16 position math and
/// the room's real collision plane so actor translations do not grow their own tile probes.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Ports the behavior used by <c>MoveEnemyRightBy_14_12_IgnoreSlopes</c> at $A0:C6AB.
    /// Non-square slopes are passable in this mode; solid-family blocks and horizontal BTS
    /// extension chains stop the actor and align its hitbox to the contacted block edge.
    /// </summary>
    private static bool MoveEnemyHorizontallyIgnoringNonSquareSlopes(
        RoomLevelData level,
        RoomEnemySlot slot,
        int displacement)
    {
        if (displacement == 0)
            return false;

        uint oldPosition = ((uint)slot.XPosition << 16) | slot.XSubposition;
        uint newPosition = unchecked(oldPosition + (uint)displacement);
        ushort targetCenter = unchecked((ushort)(newPosition >> 16));
        bool movingLeft = displacement < 0;
        ushort targetEdge = movingLeft
            ? unchecked((ushort)(targetCenter - slot.XRadius))
            : unchecked((ushort)(targetCenter + slot.XRadius - 1));

        int firstRow = unchecked((ushort)(slot.YPosition - slot.YRadius)) >> 4;
        int lastRow = unchecked((ushort)(slot.YPosition + slot.YRadius - 1)) >> 4;
        int column = targetEdge >> 4;
        bool collided = false;
        for (int row = firstRow; row <= lastRow; row++)
        {
            if (EnemyHorizontalProbeIsSolid(level, column, row))
            {
                collided = true;
                break;
            }
        }

        if (!collided)
        {
            slot.XPosition = targetCenter;
            slot.XSubposition = unchecked((ushort)newPosition);
            return false;
        }

        if (movingLeft)
        {
            slot.XPosition = unchecked((ushort)(((column << 4) | 0x0f) + slot.XRadius + 1));
            slot.XSubposition = 0;
        }
        else
        {
            slot.XPosition = unchecked((ushort)((column << 4) - slot.XRadius));
            slot.XSubposition = 0xffff;
        }
        return true;
    }

    private static bool EnemyHorizontalProbeIsSolid(RoomLevelData level, int blockX, int blockY)
    {
        // Native room coordinates are unsigned and valid enemy populations stay within the
        // allocation. Treating an escaped host coordinate as solid prevents an invalid array
        // access while matching the physical room boundary that should have stopped it.
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            return true;
        }

        RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);
        return block.CollisionType switch
        {
            // Type 1 is a slope. The $C6AB entry explicitly ignores non-square slopes;
            // square-slope half selection is irrelevant to Ripper's Parlor wall path.
            0x0 or 0x1 or 0x2 or 0x3 or 0x4 or 0x6 or 0x7 => false,

            // Type 5 is a horizontal extension. Its signed BTS byte redirects the probe to
            // another block in the same row until a concrete collision category is found.
            0x5 when block.Behavior != 0 =>
                EnemyHorizontalProbeIsSolid(level, blockX + unchecked((sbyte)block.Behavior), blockY),
            0x5 => false,

            // Type D is a vertical extension in native collision. A horizontal scan follows
            // the signed row displacement before dispatching the referenced block.
            0xd when block.Behavior != 0 =>
                EnemyHorizontalProbeIsSolid(level, blockX, blockY + unchecked((sbyte)block.Behavior)),
            0xd => false,

            // Solid, door, spike, grapple, and bomb-block families all set carry in the
            // common enemy horizontal dispatcher. Their special side effects are owned by
            // later actor/PLM integrations; their geometry is already unambiguous here.
            0x8 or 0x9 or 0xa or 0xb or 0xc or 0xe or 0xf => true,
            _ => throw new InvalidDataException(
                $"Enemy horizontal collision type ${block.CollisionType:X1} is invalid."),
        };
    }
}
