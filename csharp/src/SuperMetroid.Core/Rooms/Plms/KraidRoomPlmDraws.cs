using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.KraidRoomPlmDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The block and artwork names of Kraid's room's crumble and clear draws.</summary>
internal static class KraidRoomPlmDraws
{
    /// <summary>Recognizes a Kraid room draw and returns its calculated descriptor.</summary>
    internal static bool TryGet(PlmDraw request, out Draw draw)
    {
        bool found = PlmDraw.TryFrame(request, out KraidRoomDraw frame);
        draw = found ? new(frame) : default;
        return found;
    }

    /// <summary>Gets the one, fifteen, or twenty-two block span for a named Kraid draw.</summary>
    private static int CountFor(KraidRoomDraw draw) => draw switch
    {
        KraidRoomDraw.CrumbleFirst or KraidRoomDraw.CrumbleSecond or KraidRoomDraw.CrumbleThird or
        KraidRoomDraw.CeilingBackground1 or KraidRoomDraw.CeilingBackground2 or KraidRoomDraw.CeilingBackground3 or
        KraidRoomDraw.SpikeFirst or KraidRoomDraw.SpikeSecond => 1,
        KraidRoomDraw.ClearCeiling => 15,
        KraidRoomDraw.ClearSpikes => 22,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {draw}."),
    };

    /// <summary>Calculates a named crumble, background, or alternating clear block.</summary>
    /// <remarks>The ceiling clear starts with its unique left edge, then repeats
    /// background two/three. The spike clear repeats first/second columns eleven times.
    /// Blocks keep their collision kinds; editable visual overrides are applied later.</remarks>
    private static RoomTerrainBlock Terrain(KraidRoomDraw draw, int index) => draw switch
    {
        KraidRoomDraw.ClearCeiling => FixedBlock(index == 0 ? KraidRoomDraw.CeilingBackground1
            : (index & 1) != 0 ? KraidRoomDraw.CeilingBackground2 : KraidRoomDraw.CeilingBackground3),
        KraidRoomDraw.ClearSpikes => FixedBlock((index & 1) == 0 ? KraidRoomDraw.SpikeFirst : KraidRoomDraw.SpikeSecond),
        KraidRoomDraw.CrumbleFirst or KraidRoomDraw.CrumbleSecond or KraidRoomDraw.CrumbleThird or
        KraidRoomDraw.CeilingBackground1 or KraidRoomDraw.CeilingBackground2 or KraidRoomDraw.CeilingBackground3 or
        KraidRoomDraw.SpikeFirst or KraidRoomDraw.SpikeSecond => FixedBlock(draw),
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {draw}."),
    };

    /// <summary>The terrain of a single named crumble, background or spike block.</summary>
    private static RoomTerrainBlock FixedBlock(KraidRoomDraw draw) => FixedBlocks[draw];

    /// <summary>Resolves an ordinal artwork identifier to its calculated Kraid draw.</summary>
    internal static bool TryGetByVisualId(string id, out Draw draw)
    {
        foreach (KraidRoomDraw candidate in Enum.GetValues<KraidRoomDraw>())
            if (string.Equals(id, VisualId(candidate), StringComparison.Ordinal))
                return TryGet(PlmDraw.Of(candidate), out draw);
        draw = default;
        return false;
    }

    /// <summary>Gets the stable artwork identifier of a Kraid room draw.</summary>
    internal static string VisualId(PlmDraw request) =>
        VisualId(PlmDraw.FrameOf<KraidRoomDraw>(request, "Kraid room draw with a visual ID"));

    /// <summary>Maps a named ceiling or spike draw to its stable artwork identifier.</summary>
    private static string VisualId(KraidRoomDraw draw) => draw switch
    {
        KraidRoomDraw.CrumbleFirst => "crumble-first",
        KraidRoomDraw.CrumbleSecond => "crumble-second",
        KraidRoomDraw.CrumbleThird => "crumble-third",
        KraidRoomDraw.CeilingBackground1 => "ceiling-background-one",
        KraidRoomDraw.CeilingBackground2 => "ceiling-background-two",
        KraidRoomDraw.CeilingBackground3 => "ceiling-background-three",
        KraidRoomDraw.SpikeFirst => "spike-first",
        KraidRoomDraw.SpikeSecond => "spike-second",
        KraidRoomDraw.ClearCeiling => "clear-ceiling",
        KraidRoomDraw.ClearSpikes => "clear-spikes",
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {draw}."),
    };

    /// <summary>A horizontal, single-run Kraid draw evaluated without stored blocks.</summary>
    internal readonly record struct Draw(KraidRoomDraw Frame)
    {
        /// <summary>Gets the number of blocks in this single horizontal native run.</summary>
        internal int BlockCount => CountFor(Frame);
        /// <summary>Calculates the terrain of a block in this draw.</summary>
        internal RoomTerrainBlock BlockAt(int index) =>
            (uint)index < (uint)BlockCount ? Terrain(Frame, index) : throw new ArgumentOutOfRangeException(nameof(index));
    }

    /// <summary>Materializes temporary run records for asset import/export and inspection. Gameplay reads
    /// Draw.BlockAt directly; no generated draw cache exists.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (KraidRoomDraw frame in Enum.GetValues<KraidRoomDraw>())
            {
                var draw = new Draw(frame);
                var blocks = new RoomTerrainBlock[draw.BlockCount];
                for (int block = 0; block < blocks.Length; block++) blocks[block] = draw.BlockAt(block);
                yield return new(PlmDraw.Of(frame),
                    new RoomPlmShotBlockDrawDefinitions.Run[] { new(PlmRunDirection.Horizontal, blocks, new(0, 0)) });
            }
        }
    }
}
