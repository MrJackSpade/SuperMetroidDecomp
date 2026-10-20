namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One signed enemy rectangle, its edges stored as distances from its component origin exactly as the cartridge stores
/// them. What touching or shooting it does belongs to the owning species, not to each rectangle.
/// </summary>
/// <param name="Left">Left edge relative to the component origin, in room pixels.</param>
/// <param name="Top">Top edge relative to the component origin, in room pixels.</param>
/// <param name="Right">Right edge relative to the component origin, in room pixels.</param>
/// <param name="Bottom">Bottom edge relative to the component origin, in room pixels.</param>
internal readonly record struct EnemyHitboxRectangle(short Left, short Top, short Right, short Bottom);
