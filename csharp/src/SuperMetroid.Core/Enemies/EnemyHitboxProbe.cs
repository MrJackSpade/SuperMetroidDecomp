namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The colliding projectile's or Samus's edges as <c>$A0:9A5A/$9B7F</c> compare them, whether the walker selects the shot
/// (strict) or touch (inclusive) callback (#633), and whether the room has installed enemy artwork.
/// </summary>
internal readonly record struct EnemyHitboxProbe(
    ushort Left, ushort Right, ushort Top, ushort Bottom, bool SelectShotCallback, bool ArtworkInstalled)
{
    /// <summary>
    /// The shared literal `$A0:9A5A/$9B7F` rectangle test. The shot and touch walkers have different strict/inclusive
    /// boundaries in the cartridge.
    /// </summary>
    internal bool Overlaps(ushort left, ushort top, ushort right, ushort bottom) =>
        SelectShotCallback
            ? !EnemyMath.IsNegative16(Right - left) &&
              EnemyMath.IsNegative16(Left - right) &&
              !EnemyMath.IsNegative16(Bottom - top) &&
              EnemyMath.IsNegative16(Top - bottom)
            : EnemyMath.IsNegative16(left - Right) &&
              !EnemyMath.IsNegative16(right - Left) &&
              EnemyMath.IsNegative16(top - Bottom) &&
              !EnemyMath.IsNegative16(bottom - Top);
}
