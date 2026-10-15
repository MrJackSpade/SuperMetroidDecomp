namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>One Shaktool attack circle pose; its installed artwork key is confined to <see cref="Artwork.ShaktoolProjectileFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct ShaktoolProjectileFrame(int Pose);
