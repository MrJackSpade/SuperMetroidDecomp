namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>One Kago bug pose; its installed artwork key is confined to <see cref="Artwork.KagoBugProjectileFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct KagoBugProjectileFrame(int Pose);
