
namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Verification access to <see cref="SporeSpawnCollisionDefinitions"/> members production does not use.</summary>
internal static class SporeSpawnCollisionDefinitionsAccess
{
    /// <summary>The twelve native hitbox list identities.</summary>
    private static readonly ushort[] ListPointers =
    [
        (ushort)SporeSpawnHitboxList.ClosedHead,
        (ushort)SporeSpawnHitboxList.OpenHead,
        (ushort)SporeSpawnHitboxList.ExtendedHead,
        (ushort)SporeSpawnHitboxList.MovingHead0,
        (ushort)SporeSpawnHitboxList.MovingHead1,
        (ushort)SporeSpawnHitboxList.MovingHead2,
        (ushort)SporeSpawnHitboxList.MovingHead3,
        (ushort)SporeSpawnHitboxList.TrailingShotPoint,
        (ushort)SporeSpawnHitboxList.MirroredTrailingShotPoint,
        (ushort)SporeSpawnHitboxList.TrailingDudPoint,
        (ushort)SporeSpawnHitboxList.MovingHead4,
        (ushort)SporeSpawnHitboxList.MovingHead5,
    ];

    /// <summary>Exposes the twelve compiled Spore Spawn head frames and named hitbox-list roots.</summary>
    extension(SporeSpawnCollisionDefinitions)
    {
        /// <summary>Twelve compiled head and movement collision frames.</summary>
        internal static int FrameCount => 12;

        /// <summary>Number of named Spore Spawn hitbox roots in the native roster.</summary>
        internal static int ListCount => ListPointers.Length;
    }
}
