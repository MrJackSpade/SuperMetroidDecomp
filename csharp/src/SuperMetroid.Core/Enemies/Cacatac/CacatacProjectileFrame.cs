namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>One Cacatac spike pose; its installed artwork key is confined to <see cref="Artwork.CacatacProjectileFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct CacatacProjectileFrame(int Pose);
