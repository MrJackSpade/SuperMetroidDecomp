using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Where each enemy-projectile composition frame is stored (<see cref="EnemyProjectileCompositionDefinitions"/>).</summary>
internal static class EnemyProjectileCompositionSources
{
    /// <summary>The bank-$8D spritemap composition frame <paramref name="frame"/> is stored at.</summary>
    internal static ushort Spritemap(EnemyProjectileFrameId frame) => frame == EnemyProjectileFrameId.Blank
        ? EnemyProjectileCompositionDefinitions.BlankSpritemap
        : EnemyProjectileCompositionDefinitions.Spritemaps.ContainsKey(frame)
            ? EnemyProjectileCompositionDefinitions.Spritemaps[frame]
            : throw new ArgumentOutOfRangeException(nameof(frame), frame, "This projectile frame is not a bank-$8D composition.");
}
