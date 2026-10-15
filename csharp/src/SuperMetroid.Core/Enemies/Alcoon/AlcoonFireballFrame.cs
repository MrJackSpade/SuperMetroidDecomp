namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One Alcoon fireball pose; its installed artwork key is confined to <see cref="Artwork.AlcoonFireballFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct AlcoonFireballFrame(int Pose);
