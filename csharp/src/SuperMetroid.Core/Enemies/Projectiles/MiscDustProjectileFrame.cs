namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>One misc dust pose; its installed artwork key is confined to <see cref="Artwork.MiscDustProjectileFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct MiscDustProjectileFrame(int Pose);
