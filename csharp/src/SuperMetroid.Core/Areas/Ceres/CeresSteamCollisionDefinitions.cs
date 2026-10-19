using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Bank-$A6 physical collision for all four Ceres steam directions. Each of the
/// 28 extended frames has one zero-offset component. Its OAM pointer is deliberately
/// absent: installed sprite edits cannot change the damaging plume geometry.
/// </summary>
internal static class CeresSteamCollisionDefinitions
{
    /// <summary>Native bank of the extended maps at $A6:F142-$F250.</summary>
    internal const byte Bank = 0xa6;


    /// <summary>First ten-byte extended frame, ExtendedSpritemap_CeresSteam_Up_0 at $A6:F142.</summary>
    private const ushort FirstFrame = 0xf142;
    /// <summary>
    /// The 21 hitbox lists: the shared empty list selected by both hidden poses, then five plume rectangles per
    /// direction. Each direction's rectangles are fitted to its drawing and are not exact rotations of another direction.
    /// </summary>
    internal static readonly CeresSteamCollisionHitbox[][] Lists =
    [
        [],
        [new(-8, -16, 7, -1)],
        [new(-8, -23, 7, -2)],
        [new(-8, -32, 7, -8)],
        [new(-8, -40, 7, -16)],
        [new(-8, -40, 6, -24)],
        [new(-16, -8, -1, 7)],
        [new(-24, -8, -2, 7)],
        [new(-32, -7, -9, 7)],
        [new(-40, -9, -17, 5)],
        [new(-40, -11, -26, 2)],
        [new(-8, 0, 7, 14)],
        [new(-8, 0, 7, 23)],
        [new(-8, 8, 7, 31)],
        [new(-8, 15, 7, 39)],
        [new(-8, 23, 6, 38)],
        [new(0, -8, 15, 7)],
        [new(1, -8, 23, 7)],
        [new(9, -8, 31, 7)],
        [new(18, -9, 38, 5)],
        [new(25, -11, 40, 3)],
    ];

    /// <summary>
    /// The 28 steam frames, seven poses per direction, each with one zero-offset component: the first five poses select
    /// their direction's plume list, the two hidden poses the empty list.
    /// </summary>
    internal static EnemyFrameCollision<CeresSteamCollisionComponent> Frames { get; } = new()
    {
        { EnemyFrameId.CeresSteamOamF142, [new(Offset16<RoomPixels>.Zero, Lists[1])] },
        { EnemyFrameId.CeresSteamOamF14C, [new(Offset16<RoomPixels>.Zero, Lists[2])] },
        { EnemyFrameId.CeresSteamOamF156, [new(Offset16<RoomPixels>.Zero, Lists[3])] },
        { EnemyFrameId.CeresSteamOamF160, [new(Offset16<RoomPixels>.Zero, Lists[4])] },
        { EnemyFrameId.CeresSteamOamF16A, [new(Offset16<RoomPixels>.Zero, Lists[5])] },
        { EnemyFrameId.CeresSteamOamF174, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamOamF17E, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamOamF188, [new(Offset16<RoomPixels>.Zero, Lists[6])] },
        { EnemyFrameId.CeresSteamOamF192, [new(Offset16<RoomPixels>.Zero, Lists[7])] },
        { EnemyFrameId.CeresSteamOamF19C, [new(Offset16<RoomPixels>.Zero, Lists[8])] },
        { EnemyFrameId.CeresSteamOamF1A6, [new(Offset16<RoomPixels>.Zero, Lists[9])] },
        { EnemyFrameId.CeresSteamOamF1B0, [new(Offset16<RoomPixels>.Zero, Lists[10])] },
        { EnemyFrameId.CeresSteamOamF1BA, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamOamF1C4, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamOamF1CE, [new(Offset16<RoomPixels>.Zero, Lists[11])] },
        { EnemyFrameId.CeresSteamOamF1D8, [new(Offset16<RoomPixels>.Zero, Lists[12])] },
        { EnemyFrameId.CeresSteamOamF1E2, [new(Offset16<RoomPixels>.Zero, Lists[13])] },
        { EnemyFrameId.CeresSteamOamF1EC, [new(Offset16<RoomPixels>.Zero, Lists[14])] },
        { EnemyFrameId.CeresSteamOamF1F6, [new(Offset16<RoomPixels>.Zero, Lists[15])] },
        { EnemyFrameId.CeresSteamOamF200, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamOamF20A, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamOamF214, [new(Offset16<RoomPixels>.Zero, Lists[16])] },
        { EnemyFrameId.CeresSteamOamF21E, [new(Offset16<RoomPixels>.Zero, Lists[17])] },
        { EnemyFrameId.CeresSteamOamF228, [new(Offset16<RoomPixels>.Zero, Lists[18])] },
        { EnemyFrameId.CeresSteamOamF232, [new(Offset16<RoomPixels>.Zero, Lists[19])] },
        { EnemyFrameId.CeresSteamOamF23C, [new(Offset16<RoomPixels>.Zero, Lists[20])] },
        { EnemyFrameId.CeresSteamOamF246, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamOamF250, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
    };

    /// <summary>Twenty-eight contiguous extended-frame pointers in native direction and pose order.</summary>
    internal static FrameSequence FramePointers { get; } = new();

    /// <summary>Enumerates the twenty-eight extended-frame addresses without storing a pointer array.</summary>
    internal sealed class FrameSequence : IReadOnlyList<ushort>
    {
        /// <summary>Number of steam frames across four directions and seven poses.</summary>
        public int Count => 28;
        /// <summary>Alias for the frame count used by existing indexing callers.</summary>
        public int Length => Count;
        /// <summary>Returns an indexed frame pointer at a ten-byte native stride.</summary>
        public ushort this[int index] => (uint)index < Count
            ? (ushort)(FirstFrame + 10 * index) : throw new IndexOutOfRangeException();
        /// <summary>Yields all frame pointers in native order.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Exposes the same frame order through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
