namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>One Ripper pose; its spritemap is confined to <see cref="Artwork.RipperFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct RipperFrame(int Pose);
