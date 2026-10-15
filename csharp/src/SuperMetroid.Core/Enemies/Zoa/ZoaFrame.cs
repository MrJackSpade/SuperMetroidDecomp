namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>One Zoa pose; its spritemap is confined to <see cref="Artwork.ZoaFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct ZoaFrame(int Pose);
