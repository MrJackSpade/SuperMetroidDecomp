using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One family of linked block restorations: bomb blocks or Samus-contact crumble blocks. Each family has three native
/// level-word restoration lists, horizontal, vertical and square, that rebuild a parent block and its linked children
/// with one visual tile. The families differ only in the parent's collision type and that tile; the type-5 and type-D
/// children and their links are the same. The shape shares the compiled PLM row/column representation with shot blocks.
/// </summary>
internal sealed class RoomPlmLinkedRestoreDefinition
{
    /// <summary>Linked bomb blocks: a type-F parent.</summary>
    internal static RoomPlmLinkedRestoreDefinition BombBlock { get; } = new(
        0xf000, RoomPlmVisualBlockDefinitions.CollisionBombParent, new Dictionary<PlmRestoreShape, ushort>
        {
            [PlmRestoreShape.Horizontal] = (ushort)BombBlockRestoreDraw.Horizontal,
            [PlmRestoreShape.Vertical] = (ushort)BombBlockRestoreDraw.Vertical,
            [PlmRestoreShape.Square] = (ushort)BombBlockRestoreDraw.Square,
        });

    /// <summary>Linked Samus-contact crumble blocks: a type-B parent. Artwork replacement may only alter their visual composition.</summary>
    internal static RoomPlmLinkedRestoreDefinition ContactCrumble { get; } = new(
        0xb000, RoomPlmVisualBlockDefinitions.ContactCrumbleParent, new Dictionary<PlmRestoreShape, ushort>
        {
            [PlmRestoreShape.Horizontal] = (ushort)ContactCrumbleRestoreDraw.Horizontal,
            [PlmRestoreShape.Vertical] = (ushort)ContactCrumbleRestoreDraw.Vertical,
            [PlmRestoreShape.Square] = (ushort)ContactCrumbleRestoreDraw.Square,
        });

    /// <summary>Both families, bomb blocks first, in their export order.</summary>
    internal static IReadOnlyList<RoomPlmLinkedRestoreDefinition> Families { get; } = [BombBlock, ContactCrumble];

    /// <summary>The parent block's collision type, in the level word's high nibble.</summary>
    private readonly ushort _parentCollision;

    /// <summary>The visual block every restored cell shows.</summary>
    private readonly ushort _visualBlock;

    /// <summary>The restoration pointer of each shape.</summary>
    private readonly IReadOnlyDictionary<PlmRestoreShape, ushort> _pointers;

    /// <summary>Defines a family by its parent collision type, visual tile and the restoration pointer of each shape.</summary>
    private RoomPlmLinkedRestoreDefinition(ushort parentCollision, ushort visualBlock, IReadOnlyDictionary<PlmRestoreShape, ushort> pointers)
    {
        _parentCollision = parentCollision;
        _visualBlock = visualBlock;
        _pointers = pointers;
    }

    /// <summary>One native restoration pointer with the geometry needed to reconstruct its parent and linked children.</summary>
    /// <param name="Pointer">The bank-$84 restoration list.</param>
    /// <param name="Shape">Whether the blocks lie in a row, a column or a two-by-two square.</param>
    /// <param name="Family">The family whose parent collision type and tile the words carry.</param>
    internal readonly record struct Draw(ushort Pointer, PlmRestoreShape Shape, RoomPlmLinkedRestoreDefinition Family)
    {
        /// <summary>Whether the restored blocks stack in one column.</summary>
        internal bool Vertical => Shape == PlmRestoreShape.Vertical;

        /// <summary>Whether the restoration is the two-by-two square.</summary>
        internal bool Square => Shape == PlmRestoreShape.Square;

        /// <summary>Square restoration has two rows; horizontal and vertical shapes use one run.</summary>
        internal int RunCount => Square ? 2 : 1;

        /// <summary>Builds the parent or linked-child level word at a position within the draw shape.</summary>
        internal ushort WordAt(int run, int block)
        {
            if ((uint)run >= RunCount)
                throw new ArgumentOutOfRangeException(nameof(run));
            if ((uint)block >= 2)
                throw new ArgumentOutOfRangeException(nameof(block));
            int row = Vertical ? block : run;
            int column = Vertical ? 0 : block;
            // Every cell shows the family's tile. Children link left on the parent's row and up on later rows; only
            // the top-left cell carries the parent's collision type.
            int collision = row == 0 && column == 0 ? Family._parentCollision : row == 0 ? 0x5000 : 0xd000;
            return (ushort)(collision | Family._visualBlock);
        }
    }

    /// <summary>Maps one of this family's three restoration pointers to its horizontal, vertical or square shape.</summary>
    internal bool TryDescribe(ushort pointer, out Draw draw)
    {
        foreach (KeyValuePair<PlmRestoreShape, ushort> shape in _pointers)
        {
            if (shape.Value == pointer)
            {
                draw = new(pointer, shape.Key, this);
                return true;
            }
        }
        draw = default;
        return false;
    }

    /// <summary>Describes a restoration pointer of either family.</summary>
    internal static bool TryDescribeAny(ushort pointer, out Draw draw)
    {
        foreach (RoomPlmLinkedRestoreDefinition family in Families)
        {
            if (family.TryDescribe(pointer, out draw))
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
                yield return Export(new(_pointers[shape], shape, this));
        }
    }

    /// <summary>Converts a calculated restoration shape into the importer-facing linked draw-list format.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList Export(Draw draw)
    {
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
            runs[run] = new(draw.Vertical ? (ushort)0x8002 : (ushort)2,
                new ushort[] { draw.WordAt(run, 0), draw.WordAt(run, 1) },
                new(0, run + 1 < runs.Length ? (sbyte)1 : (sbyte)0));
        return new(draw.Pointer, runs);
    }
}
