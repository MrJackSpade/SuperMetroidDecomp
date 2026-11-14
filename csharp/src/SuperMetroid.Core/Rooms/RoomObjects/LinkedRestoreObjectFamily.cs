using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One family of linked block restorations: bomb blocks or Samus-contact crumble blocks. Each family has three
/// restoration draws, horizontal, vertical and square, that rebuild a parent block and its linked children. The families
/// differ only in the parent's collision kind; the horizontal- and vertical-extension children and their links are the
/// same. What each restoration shows is its installed picture (#1557); what it changes is its block shape (#1862).
/// </summary>
internal sealed class LinkedRestoreObjectFamily
{
    /// <summary>The blocks linked to the parent in each restoration: the one right of it, below it, or both and the one diagonally between.</summary>
    private static readonly Dictionary<RoomObjectRestoreShape, Offset16<RoomBlocks>[]> Children = new()
    {
        [RoomObjectRestoreShape.Horizontal] = [new(1, 0)],
        [RoomObjectRestoreShape.Vertical] = [new(0, 1)],
        [RoomObjectRestoreShape.Square] = [new(1, 0), new(0, 1), new(1, 1)],
    };

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

    /// <summary>The block shape each restoration rebuilds: the family's parent with its children linked to it.</summary>
    private readonly Dictionary<RoomObjectRestoreShape, RoomBlockShape> _shapes;

    /// <summary>The restoration draw of each shape.</summary>
    private readonly IReadOnlyDictionary<RoomObjectRestoreShape, RoomObjectDraw> _draws;

    /// <summary>Defines a family by its parent collision kind and the restoration draw of each shape.</summary>
    private LinkedRestoreObjectFamily(RoomCollisionType parentCollision, IReadOnlyDictionary<RoomObjectRestoreShape, RoomObjectDraw> draws)
    {
        _shapes = Children.ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Linked(parentCollision, pair.Value));
        _draws = draws;
    }

    /// <summary>One restoration draw's geometry, from which its parent and linked children are reconstructed.</summary>
    /// <param name="Shape">Whether the blocks lie in a row, a column or a two-by-two square.</param>
    /// <param name="Family">The family whose parent collision kind the parent block carries.</param>
    internal readonly record struct Draw(RoomObjectRestoreShape Shape, LinkedRestoreObjectFamily Family)
    {
        /// <summary>
        /// The blocks the restoration rebuilds and how each collides: the top-left block carries the family's parent
        /// collision, and the others link to it.
        /// </summary>
        internal RoomBlockShape Blocks => Family._shapes[Shape];
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
