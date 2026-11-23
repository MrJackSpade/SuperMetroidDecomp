namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One enemy rectangle, its edges as distances from the enemy's origin, negative leftward and upward. What touching or
/// shooting it does belongs to the owning species, not to each rectangle.
/// </summary>
/// <param name="Left">Left edge relative to the component origin, in room pixels.</param>
/// <param name="Top">Top edge relative to the component origin, in room pixels.</param>
/// <param name="Right">Right edge relative to the component origin, in room pixels.</param>
/// <param name="Bottom">Bottom edge relative to the component origin, in room pixels.</param>
internal readonly partial record struct EnemyHitboxRectangle(int Left, int Top, int Right, int Bottom);
