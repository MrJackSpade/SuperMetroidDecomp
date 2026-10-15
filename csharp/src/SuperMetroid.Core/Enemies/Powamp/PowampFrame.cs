namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>One Powamp pose; its spritemap is confined to <see cref="Artwork.PowampFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct PowampFrame(int Pose);
