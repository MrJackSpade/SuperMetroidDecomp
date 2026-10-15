namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>One fly-family pose; its spritemap is confined to <see cref="Artwork.FlyFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct FlyFrame(int Pose);
