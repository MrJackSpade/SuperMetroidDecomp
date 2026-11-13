namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One Spore Spawn projectile frame: the animation whose poses it belongs to and its pose there.</summary>
/// <param name="Animation">The animation whose run of poses the frame belongs to.</param>
/// <param name="Phase">The pose within the run.</param>
internal readonly record struct SporeSpawnProjectileFrame(SporeSpawnProjectileSegment Animation, int Phase);
