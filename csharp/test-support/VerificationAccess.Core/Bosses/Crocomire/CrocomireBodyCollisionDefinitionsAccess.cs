using SuperMetroid.Core.World.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification's native view of <see cref="CrocomireBodyCollisionDefinitions"/>: frames by bank-$A4 address.</summary>
internal static class CrocomireBodyCollisionDefinitionsAccess
{
    /// <summary>Native views of the catalog.</summary>
    extension(CrocomireBodyCollisionDefinitions)
    {
        /// <summary>Whether the living or defeated body frame at native address <paramref name="frame"/> has compiled collision.</summary>
        internal static bool HasFrame(ushort frame) => RoomEnemySlotFrameAccess.Shows(CrocomireBodyCollisionDefinitions.Bank, frame)
            ? CrocomireBodyCollisionDefinitions.HasFrame(EnemyFrames.Installed, RoomEnemySlotFrameAccess.FrameAt(CrocomireBodyCollisionDefinitions.Bank, frame))
            : CrocomireCorpseCollisionDefinitions.HasFrame(frame);

        /// <summary>The components of the living or defeated body frame at native address <paramref name="frame"/>.</summary>
        internal static ReadOnlySpan<CrocomireBodyCollisionComponent> ComponentsAt(ushort frame) =>
            RoomEnemySlotFrameAccess.Shows(CrocomireBodyCollisionDefinitions.Bank, frame)
                ? CrocomireBodyCollisionDefinitions.ComponentsAt(EnemyFrames.Installed, RoomEnemySlotFrameAccess.FrameAt(CrocomireBodyCollisionDefinitions.Bank, frame))
                : CrocomireCorpseCollisionDefinitions.ComponentsAt(frame);
    }
}
