namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>One falling spark pose; its installed artwork key is confined to <see cref="Artwork.FallingSparkFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in installed artwork order.</param>
internal readonly record struct FallingSparkFrame(int Pose);
