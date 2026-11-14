namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One Torizo projectile frame: the run of poses it belongs to and its pose there.</summary>
/// <param name="Kind">The run of poses the frame belongs to.</param>
/// <param name="Phase">The pose within the run.</param>
internal readonly record struct TorizoProjectileFrame(TorizoProjectileFrameKind Kind, int Phase);
