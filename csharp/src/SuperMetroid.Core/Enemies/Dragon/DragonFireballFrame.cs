namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>One Dragon fireball pose; its installed artwork key is confined to <see cref="Artwork.DragonFireballFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct DragonFireballFrame(int Pose);
