using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.KraidRoomObjectDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The blocks of Kraid's room's crumble and clear draws.</summary>
internal static class KraidRoomObjectDraws
{
    /// <summary>Recognizes a Kraid room draw and returns its calculated descriptor.</summary>
    internal static bool TryGet(RoomObjectDraw request, out Draw draw)
    {
        bool found = RoomObjectDraw.TryFrame(request, out KraidRoomDraw frame);
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

    /// <summary>A horizontal, single-run Kraid draw evaluated without stored blocks.</summary>
    internal readonly record struct Draw(KraidRoomDraw Frame)
    {
        /// <summary>Gets the number of blocks in this single horizontal run.</summary>
        internal int BlockCount => CountFor(Frame);
        /// <summary>Calculates the terrain of a block in this draw.</summary>
        internal RoomTerrainBlock BlockAt(int index) =>
            (uint)index < (uint)BlockCount ? Terrain(Frame, index) : throw new ArgumentOutOfRangeException(nameof(index));
    }
}
