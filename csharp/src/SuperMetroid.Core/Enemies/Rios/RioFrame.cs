namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One Rio pose; its spritemap is confined to <see cref="Artwork.RioFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct RioFrame(int Pose);
