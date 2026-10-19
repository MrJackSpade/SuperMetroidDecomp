using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Verification's native view of <see cref="MaridiaLargeSnailCollisionDefinitions"/>: frames and hitbox lists by bank-$A2 address.</summary>
internal static class MaridiaLargeSnailCollisionDefinitionsAccess
{
    /// <summary>$A2:D034, Hitbox_Oum_FacingLeft_0, the first of the contiguous hitbox lists.</summary>
    private const ushort FirstList = 0xd034;

    /// <summary>The hitbox lists laid over their native run.</summary>
    private static readonly CollisionRecordRuns<MaridiaLargeSnailCollisionHitbox> NativeLists =
        CollisionRecordRuns<MaridiaLargeSnailCollisionHitbox>.HitboxLists(
            new CollisionRecordRun<MaridiaLargeSnailCollisionHitbox>(FirstList, MaridiaLargeSnailCollisionDefinitions.Lists));

    /// <summary>Native views of the catalog.</summary>
    extension(MaridiaLargeSnailCollisionDefinitions)
    {
        /// <summary>Native roots for the Oum collision hitbox lists.</summary>
        internal static IEnumerable<ushort> HitboxPointers => NativeLists.Pointers;

        /// <summary>Whether <paramref name="frame"/> is one of Oum's frame addresses.</summary>
        internal static bool HasFrame(ushort frame) => MaridiaLargeSnailCollisionDefinitions.FramePointers.Contains(frame);

        /// <summary>The native address of the hitbox list the frame at <paramref name="frame"/> selects.</summary>
        internal static ushort HitboxListAt(ushort frame) => HasFrame(frame)
            ? NativeLists.PointerAt(Array.IndexOf(MaridiaLargeSnailCollisionDefinitions.Lists,
                MaridiaLargeSnailCollisionDefinitions.Frames.Of(RoomEnemySlotFrameAccess.FrameAt(MaridiaLargeSnailCollisionDefinitions.Bank, frame))))
            : throw new InvalidDataException($"Oum frame $A2:{frame:X4} has no compiled collision.");

        /// <summary>The hitbox list at native address <paramref name="list"/>.</summary>
        internal static ReadOnlySpan<MaridiaLargeSnailCollisionHitbox> HitboxesAt(ushort list) =>
            NativeLists.TryGet(list, out MaridiaLargeSnailCollisionHitbox[] hitboxes)
                ? hitboxes
                : throw new InvalidDataException($"Oum hitbox list $A2:{list:X4} is not compiled.");
    }
}
