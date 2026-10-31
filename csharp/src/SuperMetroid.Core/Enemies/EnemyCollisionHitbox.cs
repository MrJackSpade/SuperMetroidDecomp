namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One rectangle of an enemy's extended hitboxes, independent of its editable artwork: its signed bounds relative to the
/// component origin, and the reactions the walker selects when Samus touches it or her weapon hits it.
/// </summary>
/// <param name="Left">Left edge relative to the component origin, in room pixels.</param>
/// <param name="Top">Top edge relative to the component origin, in room pixels.</param>
/// <param name="Right">Right edge relative to the component origin, in room pixels.</param>
/// <param name="Bottom">Bottom edge relative to the component origin, in room pixels.</param>
/// <param name="Touch">The reaction when Samus touches the rectangle.</param>
/// <param name="Shot">The reaction when a Samus projectile or bomb hits the rectangle.</param>
internal readonly record struct EnemyCollisionHitbox(
    int Left, int Top, int Right, int Bottom, EnemyReaction Touch, EnemyReaction Shot);
