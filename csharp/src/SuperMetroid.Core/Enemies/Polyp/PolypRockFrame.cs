namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>One Polyp lava rock pose; its installed artwork key is confined to <see cref="Artwork.PolypRockFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct PolypRockFrame(int Pose);
