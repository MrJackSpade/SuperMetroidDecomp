namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>One Boulder pose; its spritemap is confined to <see cref="Artwork.BoulderFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct BoulderFrame(int Pose);
