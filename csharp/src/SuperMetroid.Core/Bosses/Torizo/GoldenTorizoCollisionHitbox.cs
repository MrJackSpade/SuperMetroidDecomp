namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One ordered touch/shot rectangle of a Torizo hitbox list.</summary>
/// <param name="Left">Left extent relative to the component origin.</param>
/// <param name="Top">Upper extent relative to the component origin.</param>
/// <param name="Right">Right extent relative to the component origin.</param>
/// <param name="Bottom">Lower extent relative to the component origin.</param>
/// <param name="Touch">How the rectangle reacts to Samus contact.</param>
/// <param name="Shot">How the rectangle reacts to a projectile.</param>
internal readonly record struct GoldenTorizoCollisionHitbox(
    int Left, int Top, int Right, int Bottom,
    TorizoHitboxTouch Touch, TorizoHitboxShot Shot);
