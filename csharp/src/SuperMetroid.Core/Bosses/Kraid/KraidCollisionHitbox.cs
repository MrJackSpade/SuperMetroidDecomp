namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One signed Kraid arm or foot rectangle and its reactions to Samus contact and to her projectiles.</summary>
/// <param name="Left">Left edge relative to the component origin, in room pixels.</param>
/// <param name="Top">Top edge relative to the component origin, in room pixels.</param>
/// <param name="Right">Right edge relative to the component origin, in room pixels.</param>
/// <param name="Bottom">Bottom edge relative to the component origin, in room pixels.</param>
/// <param name="Touch">The reaction when Samus touches the rectangle.</param>
/// <param name="Shot">The reaction when a Samus projectile hits the rectangle.</param>
internal readonly record struct KraidCollisionHitbox(int Left, int Top, int Right, int Bottom, KraidHitboxTouch Touch, KraidHitboxShot Shot);
