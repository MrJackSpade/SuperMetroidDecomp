namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One Crocomire collision rectangle and its touch and shot reactions, independent of its editable artwork.</summary>
/// <param name="Left">Left edge, relative to the component origin.</param>
/// <param name="Top">Top edge, relative to the component origin.</param>
/// <param name="Right">Right edge, relative to the component origin.</param>
/// <param name="Bottom">Bottom edge, relative to the component origin.</param>
/// <param name="Touch">How the rectangle reacts to Samus's touch.</param>
/// <param name="Shot">How the rectangle reacts to a Samus projectile.</param>
internal readonly record struct CrocomireBodyCollisionHitbox(
    int Left, int Top, int Right, int Bottom, CrocomireHitboxTouch Touch, CrocomireHitboxShot Shot);
