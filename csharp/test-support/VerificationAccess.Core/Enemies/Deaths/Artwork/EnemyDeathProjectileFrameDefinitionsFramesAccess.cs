using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths.Artwork;

/// <summary>Frame lists and counts of <see cref="EnemyDeathProjectileFrameDefinitions"/> that only verification read.</summary>
internal static class EnemyDeathProjectileFrameDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="EnemyDeathProjectileFrameDefinitions"/>.</summary>
    extension(EnemyDeathProjectileFrameDefinitions)
    {
        /// <summary>The poses, in installed artwork order.</summary>
        internal static int PoseCount => 47;
    }
}
