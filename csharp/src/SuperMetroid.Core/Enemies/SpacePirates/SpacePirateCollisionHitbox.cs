namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Engine-owned signed collision bounds and touch/shot reactions.</summary>
/// <param name="Left">Left contact extent relative to its component origin.</param>
/// <param name="Top">Upper contact extent relative to its component origin.</param>
/// <param name="Right">Right contact extent relative to its component origin.</param>
/// <param name="Bottom">Lower contact extent relative to its component origin.</param>
/// <param name="Touch">How the rectangle reacts when Samus touches it.</param>
/// <param name="Shot">How the rectangle reacts when a Samus projectile hits it.</param>
internal readonly record struct SpacePirateCollisionHitbox(
    int Left, int Top, int Right, int Bottom, SpacePirateHitboxTouch Touch, SpacePirateHitboxShot Shot);
