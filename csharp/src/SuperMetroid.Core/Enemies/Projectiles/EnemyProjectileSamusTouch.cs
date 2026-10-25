namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>What touching Samus does to an enemy projectile's program.</summary>
internal enum EnemyProjectileSamusTouch
{
    /// <summary>The projectile keeps running its current program.</summary>
    KeepsProgram,
    /// <summary>The projectile starts its touch reaction: its own typed reaction, or the shared delete reaction.</summary>
    StartsTouchReaction,
}
