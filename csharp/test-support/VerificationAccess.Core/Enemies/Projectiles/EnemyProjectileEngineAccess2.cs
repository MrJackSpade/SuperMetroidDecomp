namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Verification access to <see cref="EnemyProjectileEngine"/> members production does not use.</summary>
internal static class EnemyProjectileEngineAccess2
{
    /// <summary>Exposes <see cref="EnemyProjectileEngine"/> state that only verification reads.</summary>
    extension(EnemyProjectileEngine self)
    {
        /// <summary>The latest projectile of <paramref name="place"/>, live or gone, or null while the room has spawned none into it.</summary>
        /// <param name="place">The zero-based enemy-projectile place, zero for the last place spawns search.</param>
        public EnemyProjectile? LatestInPlace(int place) => self.ProjectilesInPlaces[Ensure.BetweenInclusive(0, EnemyProjectilePlaces.Capacity - 1, place)];
    }
}
