namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One Spore Spawn collision rectangle and its touch and shot reactions, independent of OAM artwork.</summary>
/// <param name="Left">Left edge, relative to the component origin.</param>
/// <param name="Top">Top edge, relative to the component origin.</param>
/// <param name="Right">Right edge, relative to the component origin.</param>
/// <param name="Bottom">Bottom edge, relative to the component origin.</param>
/// <param name="Touch">How the rectangle reacts to Samus's touch.</param>
/// <param name="Shot">How the rectangle reacts to a Samus projectile.</param>
internal readonly record struct SporeSpawnCollisionHitbox(
    int Left, int Top, int Right, int Bottom, SporeSpawnHitboxTouch Touch, SporeSpawnHitboxShot Shot);
