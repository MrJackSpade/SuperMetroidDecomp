using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification's native view of <see cref="CeresSteamCollisionDefinitions"/>: frames and hitbox lists by bank-$A6 address.</summary>
internal static class CeresSteamCollisionDefinitionsAccess
{
    /// <summary>$A6:F25A, the shared empty list that starts the contiguous hitbox lists.</summary>
    private const ushort FirstList = 0xf25a;

    /// <summary>The frames whose plume rectangles make up the native lists after the shared empty one, in native order.</summary>
    private static readonly EnemyFrameId[] PlumeFrames =
    [
        EnemyFrameId.CeresSteamUp,
        EnemyFrameId.CeresSteamUpSecond,
        EnemyFrameId.CeresSteamUpThird,
        EnemyFrameId.CeresSteamUpFourth,
        EnemyFrameId.CeresSteamUpFifth,
        EnemyFrameId.CeresSteamLeft,
        EnemyFrameId.CeresSteamLeftSecond,
        EnemyFrameId.CeresSteamLeftThird,
        EnemyFrameId.CeresSteamLeftFourth,
        EnemyFrameId.CeresSteamLeftFifth,
        EnemyFrameId.CeresSteamDown,
        EnemyFrameId.CeresSteamDownSecond,
        EnemyFrameId.CeresSteamDownThird,
        EnemyFrameId.CeresSteamDownFourth,
        EnemyFrameId.CeresSteamDownFifth,
        EnemyFrameId.CeresSteamRight,
        EnemyFrameId.CeresSteamRightSecond,
        EnemyFrameId.CeresSteamRightThird,
        EnemyFrameId.CeresSteamRightFourth,
        EnemyFrameId.CeresSteamRightFifth,
    ];

    /// <summary>The 21 native hitbox lists: the shared empty list the hidden poses select, then each plume frame's.</summary>
    private static readonly EnemyHitboxRectangle[][] Lists =
        [[], .. PlumeFrames.Select(frame => CeresSteamCollisionDefinitions.Frames.Of(frame).Single().Hitboxes.ToArray())];

    /// <summary>The hitbox lists laid over their native run.</summary>
    private static readonly CollisionRecordRuns<EnemyHitboxRectangle> NativeLists =
        CollisionRecordRuns<EnemyHitboxRectangle>.HitboxLists(new CollisionRecordRun<EnemyHitboxRectangle>(FirstList, Lists));

    /// <summary>Native views of the catalog.</summary>
    extension(CeresSteamCollisionDefinitions)
    {
        /// <summary>Bank $A6, which holds the steam's code and frames.</summary>
        internal static byte Bank => 0xa6;

        /// <summary>Native roots of the compiled Ceres steam hitbox lists.</summary>
        internal static IEnumerable<ushort> HitboxPointers => NativeLists.Pointers;

        /// <summary>The components of the steam frame at native address <paramref name="frame"/>.</summary>
        internal static CeresSteamCollisionComponent[] ComponentsAt(ushort frame) =>
            RoomEnemySlotFrameAccess.Shows(CeresSteamCollisionDefinitions.Bank, frame)
                ? CeresSteamCollisionDefinitions.Frames.Of(RoomEnemySlotFrameAccess.FrameAt(CeresSteamCollisionDefinitions.Bank, frame))
                : throw new InvalidDataException($"Ceres steam frame $A6:{frame:X4} has no compiled collision.");

        /// <summary>The hitbox list at native address <paramref name="list"/>.</summary>
        internal static ReadOnlySpan<EnemyHitboxRectangle> HitboxesAt(ushort list) =>
            NativeLists.TryGet(list, out EnemyHitboxRectangle[] hitboxes)
                ? hitboxes
                : throw new InvalidDataException($"Ceres steam hitbox list $A6:{list:X4} is not compiled.");
    }

    /// <summary>Native views of a component.</summary>
    extension(CeresSteamCollisionComponent self)
    {
        /// <summary>The native address of the component's hitbox list.</summary>
        internal ushort HitboxPointer =>
            NativeLists.PointerAt(Array.FindIndex(Lists, list => list.SequenceEqual(self.Hitboxes)));
    }
}
