namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One Alcoon pose; its spritemap is confined to <see cref="Artwork.AlcoonFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct AlcoonFrame(int Pose);
