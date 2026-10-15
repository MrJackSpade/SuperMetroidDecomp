namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One Nuclear Waffle body pose; its installed artwork key is confined to <see cref="Artwork.NuclearWaffleProjectileFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct NuclearWaffleProjectileFrame(int Pose);
