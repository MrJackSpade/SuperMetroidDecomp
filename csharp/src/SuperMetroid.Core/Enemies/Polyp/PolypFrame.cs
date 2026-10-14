namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>One Polyp pose; its spritemap is confined to <see cref="Artwork.PolypFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct PolypFrame(int Pose);
