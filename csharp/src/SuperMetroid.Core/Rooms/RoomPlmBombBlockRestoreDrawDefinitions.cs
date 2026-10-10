namespace SuperMetroid.Core.Rooms;

/// <summary>The three bank-$84 linked bomb-block restoration draw lists.</summary>
internal enum BombBlockRestoreDraw : ushort
{
    /// <summary>Horizontal parent and child restoration at $84:A4C7.</summary>
    Horizontal = 0xa4c7,
    /// <summary>Vertical parent and child restoration at $84:A4CF.</summary>
    Vertical = 0xa4cf,
    /// <summary>Four-block parent and child restoration at $84:A4D7.</summary>
    Square = 0xa4d7,
}

/// <summary>
/// Complete native level-word restoration lists for linked bomb blocks. Their
/// shape shares the compiled PLM row/column representation with shot blocks,
/// while the distinct type-F parent and type-5/type-D children stay immutable.
/// </summary>
internal static class RoomPlmBombBlockRestoreDrawDefinitions
{
    internal readonly record struct Draw(ushort Pointer, bool Vertical, bool Square)
    {
        internal int RunCount => Square ? 2 : 1;

        internal ushort WordAt(int run, int block)
        {
            if ((uint)run >= RunCount || (uint)block >= 2)
                throw new IndexOutOfRangeException();
            int row = Vertical ? block : run;
            int column = Vertical ? 0 : block;
            // Every cell shows the same bomb tile. Children link left on the
            // parent's row and up on later rows; only the top-left cell is type F.
            int collision = row == 0 && column == 0 ? 0xf000 : row == 0 ? 0x5000 : 0xd000;
            return (ushort)(collision | RoomPlmVisualBlockIndexes.CollisionBombParent);
        }
    }

    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        if (!Enum.IsDefined((BombBlockRestoreDraw)pointer))
        {
            draw = default;
            return false;
        }
        draw = (BombBlockRestoreDraw)pointer switch
        {
            BombBlockRestoreDraw.Horizontal => new(pointer, false, false),
            BombBlockRestoreDraw.Vertical => new(pointer, true, false),
            BombBlockRestoreDraw.Square => new(pointer, false, true),
            _ => throw new InvalidOperationException($"Undefined {nameof(BombBlockRestoreDraw)} {pointer:X4}."),
        };
        return true;
    }

    // Temporary draw DTOs support existing artwork import/export consumers.
    // Runtime draws the calculated descriptor directly; no generated cache remains.
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            yield return Export(new((ushort)BombBlockRestoreDraw.Horizontal, false, false));
            yield return Export(new((ushort)BombBlockRestoreDraw.Vertical, true, false));
            yield return Export(new((ushort)BombBlockRestoreDraw.Square, false, true));
        }
    }

    private static RoomPlmShotBlockDrawDefinitions.DrawList Export(Draw draw)
    {
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
            runs[run] = new(draw.Vertical ? (ushort)0x8002 : (ushort)2,
                new ushort[] {draw.WordAt(run, 0), draw.WordAt(run, 1)},
                0, run + 1 < runs.Length ? (sbyte)1 : (sbyte)0);
        return new(draw.Pointer, runs);
    }
}
