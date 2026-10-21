namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One downward gate pose; its installed artwork key is confined to <see cref="SuperMetroid.Core.Enemies.Projectiles.Artwork.DownwardGateProjectileFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct DownwardGateProjectileFrame(int Pose);
