using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>One installed program frame of the enemy-projectile artwork file, by the name the file keys it under.</summary>
internal readonly record struct EnemyProjectilePresentationFrameDefinition(string Name)
{
    /// <summary>The projectile frame the installed program frame shows.</summary>
    internal EnemyProjectileFrameId Frame => EnemyProjectileFrames.Identity(Name);
}
