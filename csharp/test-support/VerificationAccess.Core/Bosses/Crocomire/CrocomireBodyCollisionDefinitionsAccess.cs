using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification's native view of <see cref="CrocomireBodyCollisionDefinitions"/>: frames by bank-$A4 address.</summary>
internal static class CrocomireBodyCollisionDefinitionsAccess
{
    /// <summary>Native views of the catalog.</summary>
    extension(CrocomireBodyCollisionDefinitions)
    {
        /// <summary>Native bank containing Crocomire's body frames and hitboxes.</summary>
        internal static byte Bank => 0xa4;

        /// <summary>Whether the living or defeated body frame at native address <paramref name="frame"/> has compiled collision.</summary>
        internal static bool HasFrame(ushort frame) =>
            CrocomireBodyCollisionDefinitions.HasFrame(RoomEnemySlotFrameAccess.TryFrameAt(CrocomireBodyCollisionDefinitions.Bank, frame));

        /// <summary>The components of the living or defeated body frame at native address <paramref name="frame"/>.</summary>
        internal static ReadOnlySpan<CrocomireBodyCollisionComponent> ComponentsAt(ushort frame) =>
            CrocomireBodyCollisionDefinitions.ComponentsAt(RoomEnemySlotFrameAccess.TryFrameAt(CrocomireBodyCollisionDefinitions.Bank, frame));
    }
}
