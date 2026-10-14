namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Complete native restoration words for linked Samus-contact crumble blocks.
/// The type-B parent and type-5/type-D children are physical room mutations;
/// artwork replacement may only alter their visual block composition.
/// </summary>
internal static class RoomPlmContactCrumbleRestoreDrawDefinitions
{
    /// <summary>One linked crumble shape's orientation and whether it occupies a second run.</summary>
    internal readonly record struct Draw(ushort Pointer, bool Vertical, bool Square)
    {
        /// <summary>Square restoration has two rows; horizontal and vertical shapes use one run.</summary>
        internal int RunCount => Square ? 2 : 1;

        /// <summary>Rebuilds parent type B and linked child type 5 or D while keeping the common crumble visual tile.</summary>
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

    /// <summary>Recognizes only the three authored linked contact-crumble draw pointers.</summary>
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

    /// <summary>Creates temporary draw-list views for artwork import and export; runtime uses calculated descriptors directly.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            yield return Export(new((ushort)ContactCrumbleRestoreDraw.Horizontal, false, false));
            yield return Export(new((ushort)ContactCrumbleRestoreDraw.Vertical, true, false));
            yield return Export(new((ushort)ContactCrumbleRestoreDraw.Square, false, true));
        }
    }

    /// <summary>Encodes each shape's horizontal or vertical two-block run for existing draw-list consumers.</summary>
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
