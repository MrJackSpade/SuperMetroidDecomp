namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Frame lists and counts of <see cref="EnemyProjectilePresentationFrameDefinitions"/> that only verification read.</summary>
internal static class EnemyProjectilePresentationFrameDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="EnemyProjectilePresentationFrameDefinitions"/>.</summary>
    extension(EnemyProjectilePresentationFrameDefinitions)
    {
        /// <summary>The generic death explosions' frames, which install first; the drops' frames follow them.</summary>
        internal static int DeathFrameCount => 31;
    }
}
