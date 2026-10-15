namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>One Tripper or Kamer pose; its spritemap is confined to <see cref="Artwork.PlatformFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct PlatformFrame(int Pose);
