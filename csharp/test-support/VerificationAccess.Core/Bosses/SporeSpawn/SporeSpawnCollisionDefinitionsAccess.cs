
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
}
