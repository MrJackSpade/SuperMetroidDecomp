namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>One Shaktool pose; its spritemap is confined to <see cref="Artwork.ShaktoolFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct ShaktoolFrame(int Pose);
