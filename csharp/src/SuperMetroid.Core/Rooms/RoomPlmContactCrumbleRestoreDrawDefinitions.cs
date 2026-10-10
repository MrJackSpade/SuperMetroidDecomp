namespace SuperMetroid.Core.Rooms;

/// <summary>The three bank-$84 linked contact-crumble restoration draw lists.</summary>
internal enum ContactCrumbleRestoreDraw : ushort
{
    /// <summary>Horizontal parent and child restoration at $84:A4A1.</summary>
    Horizontal = 0xa4a1,
    /// <summary>Vertical parent and child restoration at $84:A4A9.</summary>
    Vertical = 0xa4a9,
    /// <summary>Four-block parent and children restoration at $84:A4B1.</summary>
    Square = 0xa4b1,
}

/// <summary>
/// Complete native restoration words for linked Samus-contact crumble blocks.
/// The type-B parent and type-5/type-D children are physical room mutations;
/// artwork replacement may only alter their visual block composition.
/// </summary>
internal static class RoomPlmContactCrumbleRestoreDrawDefinitions
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
            // Every cell shows the same crumble tile. Children link left on the
            // parent's row and up on later rows; only the top-left cell is type B.
            int collision = row == 0 && column == 0 ? 0xb000 : row == 0 ? 0x5000 : 0xd000;
            return (ushort)(collision | RoomPlmVisualBlockIndexes.ContactCrumbleParent);
        }
    }

    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        if (!Enum.IsDefined((ContactCrumbleRestoreDraw)pointer))
        {
            draw = default;
            return false;
        }
        draw = (ContactCrumbleRestoreDraw)pointer switch
        {
            ContactCrumbleRestoreDraw.Horizontal => new(pointer, false, false),
            ContactCrumbleRestoreDraw.Vertical => new(pointer, true, false),
            ContactCrumbleRestoreDraw.Square => new(pointer, false, true),
            _ => throw new InvalidOperationException($"Undefined {nameof(ContactCrumbleRestoreDraw)} {pointer:X4}."),
        };
        return true;
    }

    // Temporary draw DTOs support existing artwork import/export consumers.
    // Runtime draws the calculated descriptor directly; no generated cache remains.
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            yield return Export(new((ushort)ContactCrumbleRestoreDraw.Horizontal, false, false));
            yield return Export(new((ushort)ContactCrumbleRestoreDraw.Vertical, true, false));
            yield return Export(new((ushort)ContactCrumbleRestoreDraw.Square, false, true));
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
