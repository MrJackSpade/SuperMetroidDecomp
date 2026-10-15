namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>One Stoke projectile pose; its installed artwork key is confined to <see cref="Artwork.StokeProjectileFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct StokeProjectileFrame(int Pose);
