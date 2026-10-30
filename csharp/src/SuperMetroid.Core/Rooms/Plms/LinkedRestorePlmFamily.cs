using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One family of linked block restorations: bomb blocks or Samus-contact crumble blocks. Each family has three
/// restoration draws, horizontal, vertical and square, that rebuild a parent block and its linked children with one
/// appearance. The families differ only in the parent's collision kind and that appearance; the horizontal- and
/// vertical-extension children and their links are the same. The shape shares the compiled PLM row/column representation with shot blocks.
/// </summary>
internal sealed class LinkedRestorePlmFamily
{
    /// <summary>Linked bomb blocks: a bombable parent.</summary>
    internal static LinkedRestorePlmFamily BombBlock { get; } = new(
        RoomCollisionType.BombableBlock, RoomPlmVisualBlockDefinitions.CollisionBombParent, new Dictionary<PlmRestoreShape, PlmDraw>
        {
            [PlmRestoreShape.Horizontal] = PlmDraw.Of(BombBlockRestoreDraw.Horizontal),
            [PlmRestoreShape.Vertical] = PlmDraw.Of(BombBlockRestoreDraw.Vertical),
            [PlmRestoreShape.Square] = PlmDraw.Of(BombBlockRestoreDraw.Square),
        });

    /// <summary>Linked Samus-contact crumble blocks: a special-block parent. Artwork replacement may only alter their visual composition.</summary>
    internal static LinkedRestorePlmFamily ContactCrumble { get; } = new(
        RoomCollisionType.SpecialBlock, RoomPlmVisualBlockDefinitions.ContactCrumbleParent, new Dictionary<PlmRestoreShape, PlmDraw>
        {
            [PlmRestoreShape.Horizontal] = PlmDraw.Of(ContactCrumbleRestoreDraw.Horizontal),
            [PlmRestoreShape.Vertical] = PlmDraw.Of(ContactCrumbleRestoreDraw.Vertical),
            [PlmRestoreShape.Square] = PlmDraw.Of(ContactCrumbleRestoreDraw.Square),
        });

    /// <summary>Both families, bomb blocks first, in their export order.</summary>
    internal static IReadOnlyList<LinkedRestorePlmFamily> Families { get; } = [BombBlock, ContactCrumble];

    /// <summary>The parent block's collision type.</summary>
    private readonly RoomCollisionType _parentCollision;

    /// <summary>The appearance every restored cell shows.</summary>
    private readonly RoomBlockAppearance _appearance;

    /// <summary>The restoration draw of each shape.</summary>
    private readonly IReadOnlyDictionary<PlmRestoreShape, PlmDraw> _draws;

    /// <summary>Defines a family by its parent collision kind, appearance and the restoration draw of each shape.</summary>
    private LinkedRestorePlmFamily(RoomCollisionType parentCollision, RoomBlockAppearance appearance, IReadOnlyDictionary<PlmRestoreShape, PlmDraw> draws)
    {
        _parentCollision = parentCollision;
        _appearance = appearance;
        _draws = draws;
    }

    /// <summary>One restoration draw with the geometry needed to reconstruct its parent and linked children.</summary>
    /// <param name="Id">The restoration draw.</param>
    /// <param name="Shape">Whether the blocks lie in a row, a column or a two-by-two square.</param>
    /// <param name="Family">The family whose parent collision kind and appearance the blocks carry.</param>
    internal readonly record struct Draw(PlmDraw Id, PlmRestoreShape Shape, LinkedRestorePlmFamily Family)
    {
        /// <summary>Whether the restored blocks stack in one column.</summary>
        internal bool Vertical => Shape == PlmRestoreShape.Vertical;

        /// <summary>Whether the restoration is the two-by-two square.</summary>
        internal bool Square => Shape == PlmRestoreShape.Square;

        /// <summary>Square restoration has two rows; horizontal and vertical shapes use one run.</summary>
        internal int RunCount => Square ? 2 : 1;

        /// <summary>Builds the parent or linked-child block at a position within the draw shape.</summary>
        internal RoomTerrainBlock BlockAt(int run, int block)
        {
            if ((uint)run >= RunCount)
                throw new ArgumentOutOfRangeException(nameof(run));
            if ((uint)block >= 2)
                throw new ArgumentOutOfRangeException(nameof(block));
            int row = Vertical ? block : run;
            int column = Vertical ? 0 : block;
            // Every cell shows the family's appearance. Children link left on the parent's row and up on later rows;
            // only the top-left cell carries the parent's collision kind.
            RoomCollisionType collision = row == 0 && column == 0 ? Family._parentCollision
                : row == 0 ? RoomCollisionType.HorizontalExtension : RoomCollisionType.VerticalExtension;
            return new RoomTerrainBlock(Family._appearance, collision);
        }
    }

    /// <summary>Maps one of this family's three restoration draws to its horizontal, vertical or square shape.</summary>
    internal bool TryDescribe(PlmDraw request, out Draw draw)
    {
        foreach (KeyValuePair<PlmRestoreShape, PlmDraw> shape in _draws)
        {
            if (shape.Value == request)
            {
                draw = new(request, shape.Key, this);
                return true;
            }
        }
        draw = default;
        return false;
    }

    /// <summary>Describes a restoration draw of either family.</summary>
    internal static bool TryDescribeAny(PlmDraw request, out Draw draw)
    {
        foreach (LinkedRestorePlmFamily family in Families)
        {
            if (family.TryDescribe(request, out draw))
                return true;
        }
        draw = default;
        return false;
    }

    /// <summary>Exports temporary draw descriptions for artwork import and export; runtime uses calculated descriptors directly.</summary>
    internal IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (PlmRestoreShape shape in Enum.GetValues<PlmRestoreShape>())
                yield return Export(new(_draws[shape], shape, this));
        }
    }

    /// <summary>Converts a calculated restoration shape into the importer-facing linked draw-list format.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList Export(Draw draw)
    {
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
            runs[run] = new(draw.Vertical ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal,
                new[] { draw.BlockAt(run, 0), draw.BlockAt(run, 1) },
                new(0, run + 1 < runs.Length ? (sbyte)1 : (sbyte)0));
        return new(draw.Id, runs);
    }
}
