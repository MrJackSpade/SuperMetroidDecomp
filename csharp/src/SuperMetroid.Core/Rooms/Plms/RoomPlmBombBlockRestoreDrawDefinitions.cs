using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Complete native level-word restoration lists for linked bomb blocks. Their
/// shape shares the compiled PLM row/column representation with shot blocks,
/// while the distinct type-F parent and type-5/type-D children stay immutable.
/// </summary>
internal static class RoomPlmBombBlockRestoreDrawDefinitions
{
    /// <summary>One native restoration pointer with the geometry needed to reconstruct its parent and linked children.</summary>
    internal readonly record struct Draw(ushort Pointer, PlmRestoreShape Shape)
    {
        /// <summary>Whether the restored blocks stack in one column.</summary>
        internal bool Vertical => Shape == PlmRestoreShape.Vertical;

        /// <summary>Whether the restoration is the two-by-two square.</summary>
        internal bool Square => Shape == PlmRestoreShape.Square;

        /// <summary>Square restoration needs a second row; linear variants fit one run.</summary>
        internal int RunCount => Square ? 2 : 1;

        /// <summary>Builds the bomb tile's parent or linked-child collision word at a position within the draw shape.</summary>
        internal ushort WordAt(int run, int block)
        {
            if ((uint)run >= RunCount)
                throw new ArgumentOutOfRangeException(nameof(run));
            if ((uint)block >= 2)
                throw new ArgumentOutOfRangeException(nameof(block));
            int row = Vertical ? block : run;
            int column = Vertical ? 0 : block;
            // Every cell shows the same bomb tile. Children link left on the
            // parent's row and up on later rows; only the top-left cell is type F.
            int collision = row == 0 && column == 0 ? 0xf000 : row == 0 ? 0x5000 : 0xd000;
            return (ushort)(collision | RoomPlmVisualBlockDefinitions.CollisionBombParent);
        }
    }

    /// <summary>Maps one of the three native restoration pointers to its horizontal, vertical, or square shape.</summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        if (!Enum.IsDefined((BombBlockRestoreDraw)pointer))
        {
            draw = default;
            return false;
        }
        draw = (BombBlockRestoreDraw)pointer switch
        {
            BombBlockRestoreDraw.Horizontal => new(pointer, PlmRestoreShape.Horizontal),
            BombBlockRestoreDraw.Vertical => new(pointer, PlmRestoreShape.Vertical),
            BombBlockRestoreDraw.Square => new(pointer, PlmRestoreShape.Square),
            _ => throw new InvalidOperationException($"Undefined {nameof(BombBlockRestoreDraw)} {pointer:X4}."),
        };
        return true;
    }

    /// <summary>Exports temporary draw descriptions for artwork import and export; runtime uses calculated descriptors directly.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            yield return Export(new((ushort)BombBlockRestoreDraw.Horizontal, PlmRestoreShape.Horizontal));
            yield return Export(new((ushort)BombBlockRestoreDraw.Vertical, PlmRestoreShape.Vertical));
            yield return Export(new((ushort)BombBlockRestoreDraw.Square, PlmRestoreShape.Square));
        }
    }

    /// <summary>Converts a calculated restoration shape into the importer-facing linked draw-list format.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList Export(Draw draw)
    {
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
            runs[run] = new(draw.Vertical ? (ushort)0x8002 : (ushort)2,
                new ushort[] {draw.WordAt(run, 0), draw.WordAt(run, 1)},
                new(0, run + 1 < runs.Length ? (sbyte)1 : (sbyte)0));
        return new(draw.Pointer, runs);
    }
}
