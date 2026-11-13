namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>What touching Samus does to an enemy projectile's animation.</summary>
internal enum EnemyProjectileSamusTouch
{
    /// <summary>The projectile keeps running its current animation.</summary>
    KeepsAnimation,
    /// <summary>The projectile starts its touch reaction: its own typed reaction, or the shared delete reaction.</summary>
    StartsTouchReaction,
}
