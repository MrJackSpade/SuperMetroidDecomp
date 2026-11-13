using SuperMetroid.Core.Enemies.Projectiles.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Tells which enemy projectile frames draw nothing.</summary>
internal static class EnemyProjectileEmptyFrames
{
    /// <summary>
    /// Whether a projectile showing <paramref name="frame"/> draws nothing: the blank frame, or a program frame showing the
    /// empty projectile picture (#1513).
    /// </summary>
    internal static bool DrawsNothing(EnemyProjectileFrameId frame) =>
        frame == EnemyProjectileFrameId.Blank || EnemyProjectileEmptyFrameDefinitions.EmptyFrames.Contains(frame);
}
