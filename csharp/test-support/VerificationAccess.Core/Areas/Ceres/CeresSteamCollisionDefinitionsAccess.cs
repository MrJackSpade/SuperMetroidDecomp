using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification's native view of <see cref="CeresSteamCollisionDefinitions"/>: frames and hitbox lists by bank-$A6 address.</summary>
internal static class CeresSteamCollisionDefinitionsAccess
{
    /// <summary>$A6:F25A, the shared empty list that starts the contiguous hitbox lists.</summary>
    private const ushort FirstList = 0xf25a;

    /// <summary>The hitbox lists laid over their native run.</summary>
    private static readonly CollisionRecordRuns<CeresSteamCollisionHitbox> NativeLists =
        CollisionRecordRuns<CeresSteamCollisionHitbox>.HitboxLists(new CollisionRecordRun<CeresSteamCollisionHitbox>(FirstList, CeresSteamCollisionDefinitions.Lists));

    /// <summary>Native views of the catalog.</summary>
    extension(CeresSteamCollisionDefinitions)
    {
        /// <summary>Native roots of the compiled Ceres steam hitbox lists.</summary>
        internal static IEnumerable<ushort> HitboxPointers => NativeLists.Pointers;

        /// <summary>The components of the steam frame at native address <paramref name="frame"/>.</summary>
        internal static CeresSteamCollisionComponent[] ComponentsAt(ushort frame) =>
            RoomEnemySlotFrameAccess.Shows(CeresSteamCollisionDefinitions.Bank, frame)
                ? CeresSteamCollisionDefinitions.Frames.Of(RoomEnemySlotFrameAccess.FrameAt(CeresSteamCollisionDefinitions.Bank, frame))
                : throw new InvalidDataException($"Ceres steam frame $A6:{frame:X4} has no compiled collision.");

        /// <summary>The hitbox list at native address <paramref name="list"/>.</summary>
        internal static ReadOnlySpan<CeresSteamCollisionHitbox> HitboxesAt(ushort list) =>
            NativeLists.TryGet(list, out CeresSteamCollisionHitbox[] hitboxes)
                ? hitboxes
                : throw new InvalidDataException($"Ceres steam hitbox list $A6:{list:X4} is not compiled.");
    }

    /// <summary>Native views of a component.</summary>
    extension(CeresSteamCollisionComponent self)
    {
        /// <summary>The native address of the component's hitbox list.</summary>
        internal ushort HitboxPointer =>
            NativeLists.PointerAt(Array.FindIndex(CeresSteamCollisionDefinitions.Lists, list => ReferenceEquals(list, self.Hitboxes)));
    }
}
