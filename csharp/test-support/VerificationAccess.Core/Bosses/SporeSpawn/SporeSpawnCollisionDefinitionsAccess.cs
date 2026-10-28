
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Verification access to <see cref="SporeSpawnCollisionDefinitions"/> members production does not use.</summary>
internal static class SporeSpawnCollisionDefinitionsAccess
{
    /// <summary>Exposes the twelve compiled Spore Spawn head frames and the number of cartridge hitbox lists they name.</summary>
    extension(SporeSpawnCollisionDefinitions)
    {
        /// <summary>Twelve compiled head and movement collision frames.</summary>
        internal static int FrameCount => 12;

        /// <summary>Number of distinct cartridge hitbox lists the twelve frames' components name, including the mirrored trailing point that shares the active point's rectangles.</summary>
        internal static int ListCount => 12;
    }

    /// <summary>Reads Spore Spawn collision by native address.</summary>
    extension(SporeSpawnCollision)
    {
        /// <summary>Whether the frame at native bank-$A5 address <paramref name="frame"/> is one of Spore Spawn's.</summary>
        internal static bool IsFrame(ushort frame) =>
            SporeSpawnCollision.IsFrame(RoomEnemySlotFrameAccess.TryFrameAt(0xa5, frame));

        /// <summary>The components of the frame at native bank-$A5 address <paramref name="frame"/>.</summary>
        internal static ReadOnlySpan<SporeSpawnCollisionComponent> ComponentsAt(ushort frame) =>
            SporeSpawnCollision.ComponentsAt(RoomEnemySlotFrameAccess.TryFrameAt(0xa5, frame));
    }
}
