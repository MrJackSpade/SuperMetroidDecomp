namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Verification's composition lookups through <see cref="EnemyProjectileFrames"/>.</summary>
internal static class EnemyProjectileFramesAccess
{
    /// <summary>The lookups, read as members of <see cref="EnemyProjectileFrames"/>.</summary>
    extension(EnemyProjectileFrames)
    {
        /// <summary>Whether <paramref name="frame"/> draws a bank-$8D composition rather than an installed program frame.</summary>
        internal static bool IsComposition(EnemyProjectileFrameId frame) => EnemyProjectileFrames.Compositions.ContainsKey(frame);

        /// <summary>The bank-$8D composition that composition frame <paramref name="frame"/> draws.</summary>
        internal static ushort Composition(EnemyProjectileFrameId frame) => EnemyProjectileFrames.Compositions.TryGetValue(frame, out ushort spritemap)
            ? spritemap
            : throw new ArgumentOutOfRangeException(nameof(frame), frame, "This projectile frame is an installed program frame.");
    }
}
