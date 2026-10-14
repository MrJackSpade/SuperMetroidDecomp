namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>One Spark pose; its spritemap is confined to <see cref="Artwork.SparkFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct SparkFrame(int Pose);
