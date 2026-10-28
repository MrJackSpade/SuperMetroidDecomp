using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification's native view of <see cref="CrocomireBodyCollision"/>: frames by bank-$A4 address.</summary>
internal static class CrocomireBodyCollisionDefinitionsAccess
{
    /// <summary>Native views of the catalog.</summary>
    extension(CrocomireBodyCollision)
    {
        /// <summary>Native bank containing Crocomire's body frames and hitboxes.</summary>
        internal static byte Bank => 0xa4;

        /// <summary>Whether the living or defeated body frame at native address <paramref name="frame"/> has compiled collision.</summary>
        internal static bool HasFrame(ushort frame) =>
            CrocomireBodyCollision.HasFrame(RoomEnemySlotFrameAccess.TryFrameAt(CrocomireBodyCollision.Bank, frame));

        /// <summary>The components of the living or defeated body frame at native address <paramref name="frame"/>.</summary>
        internal static ReadOnlySpan<CrocomireBodyCollisionComponent> ComponentsAt(ushort frame) =>
            CrocomireBodyCollision.ComponentsAt(RoomEnemySlotFrameAccess.TryFrameAt(CrocomireBodyCollision.Bank, frame));
    }
}
