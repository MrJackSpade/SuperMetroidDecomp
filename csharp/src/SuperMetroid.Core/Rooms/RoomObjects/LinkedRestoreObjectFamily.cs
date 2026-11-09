using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One family of linked block restorations: bomb blocks or Samus-contact crumble blocks. Each family has three
/// restoration draws, horizontal, vertical and square, that rebuild a parent block and its linked children. The families
/// differ only in the parent's collision kind; the horizontal- and vertical-extension children and their links are the
/// same. What each restoration shows is its installed picture (#1557). The shape shares the PLM row/column
/// representation with shot blocks.
/// </summary>
internal sealed class LinkedRestoreObjectFamily
{
    /// <summary>Linked bomb blocks: a bombable parent.</summary>
    internal static LinkedRestoreObjectFamily BombBlock { get; } = new(
        RoomCollisionType.BombableBlock, new Dictionary<RoomObjectRestoreShape, RoomObjectDraw>
        {
            [RoomObjectRestoreShape.Horizontal] = RoomObjectDraw.Of(BombBlockRestoreDraw.Horizontal),
            [RoomObjectRestoreShape.Vertical] = RoomObjectDraw.Of(BombBlockRestoreDraw.Vertical),
            [RoomObjectRestoreShape.Square] = RoomObjectDraw.Of(BombBlockRestoreDraw.Square),
        });

    /// <summary>Linked Samus-contact crumble blocks: a special-block parent.</summary>
    internal static LinkedRestoreObjectFamily ContactCrumble { get; } = new(
        RoomCollisionType.SpecialBlock, new Dictionary<RoomObjectRestoreShape, RoomObjectDraw>
        {
            [RoomObjectRestoreShape.Horizontal] = RoomObjectDraw.Of(ContactCrumbleRestoreDraw.Horizontal),
            [RoomObjectRestoreShape.Vertical] = RoomObjectDraw.Of(ContactCrumbleRestoreDraw.Vertical),
            [RoomObjectRestoreShape.Square] = RoomObjectDraw.Of(ContactCrumbleRestoreDraw.Square),
        });

    /// <summary>Both families, bomb blocks first, in their export order.</summary>
    internal static IReadOnlyList<LinkedRestoreObjectFamily> Families { get; } = [BombBlock, ContactCrumble];

    /// <summary>The parent block's collision type.</summary>
    private readonly RoomCollisionType _parentCollision;


    /// <summary>The restoration draw of each shape.</summary>
    private readonly IReadOnlyDictionary<RoomObjectRestoreShape, RoomObjectDraw> _draws;

    /// <summary>Defines a family by its parent collision kind and the restoration draw of each shape.</summary>
    private LinkedRestoreObjectFamily(RoomCollisionType parentCollision, IReadOnlyDictionary<RoomObjectRestoreShape, RoomObjectDraw> draws)
    {
        _parentCollision = parentCollision;
        _draws = draws;
    }

    /// <summary>One restoration draw's geometry, from which its parent and linked children are reconstructed.</summary>
    /// <param name="Shape">Whether the blocks lie in a row, a column or a two-by-two square.</param>
    /// <param name="Family">The family whose parent collision kind the parent block carries.</param>
    internal readonly record struct Draw(RoomObjectRestoreShape Shape, LinkedRestoreObjectFamily Family)
    {
        /// <summary>Whether the restored blocks stack in one column.</summary>
        internal bool Vertical => Shape == RoomObjectRestoreShape.Vertical;

        /// <summary>Whether the restoration is the two-by-two square.</summary>
        internal bool Square => Shape == RoomObjectRestoreShape.Square;

        /// <summary>Square restoration has two rows; horizontal and vertical shapes use one run.</summary>
        internal int RunCount => Square ? 2 : 1;

        /// <summary>How the parent or linked-child cell at a position within the draw shape collides.</summary>
        internal RoomCollisionType CollisionAt(int run, int block)
        {
            if ((uint)run >= RunCount)
                throw new ArgumentOutOfRangeException(nameof(run));
            if ((uint)block >= 2)
                throw new ArgumentOutOfRangeException(nameof(block));
            int row = Vertical ? block : run;
            int column = Vertical ? 0 : block;
            // Children link left on the parent's row and up on later rows; only the top-left cell carries the parent's
            // collision kind.
            return row == 0 && column == 0 ? Family._parentCollision
                : row == 0 ? RoomCollisionType.HorizontalExtension : RoomCollisionType.VerticalExtension;
        }
    }

    /// <summary>Maps one of this family's three restoration draws to its horizontal, vertical or square shape.</summary>
    internal bool TryDescribe(RoomObjectDraw request, out Draw draw)
    {
        foreach (KeyValuePair<RoomObjectRestoreShape, RoomObjectDraw> shape in _draws)
        {
            if (shape.Value == request)
            {
                draw = new(shape.Key, this);
                return true;
            }
        }
        draw = default;
        return false;
    }

    /// <summary>Describes a restoration draw of either family.</summary>
    internal static bool TryDescribeAny(RoomObjectDraw request, out Draw draw)
    {
        foreach (LinkedRestoreObjectFamily family in Families)
        {
            if (family.TryDescribe(request, out draw))
                return true;
        }
        draw = default;
        return false;
    }
}
