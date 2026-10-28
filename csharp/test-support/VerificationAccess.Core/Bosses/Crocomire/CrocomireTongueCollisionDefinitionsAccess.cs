using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification's native view of <see cref="CrocomireTongueCollision"/>: frames by their bank-$A4 address.</summary>
internal static class CrocomireTongueCollisionDefinitionsAccess
{
    /// <summary>Native views of the catalog.</summary>
    extension(CrocomireTongueCollision)
    {
        /// <summary>Whether the frame at native address <paramref name="frame"/> is one of the tongue's.</summary>
        internal static bool HasFrame(ushort frame) =>
            CrocomireTongueCollision.HasFrame(RoomEnemySlotFrameAccess.TryFrameAt(CrocomireTongueCollisionDefinitionsConstants.Bank, frame));

        /// <summary>The component of the tongue frame at native address <paramref name="frame"/>.</summary>
        internal static CrocomireTongueCollisionComponent ComponentAt(ushort frame) =>
            CrocomireTongueCollision.ComponentAt(RoomEnemySlotFrameAccess.TryFrameAt(CrocomireTongueCollisionDefinitionsConstants.Bank, frame));
    }
}
