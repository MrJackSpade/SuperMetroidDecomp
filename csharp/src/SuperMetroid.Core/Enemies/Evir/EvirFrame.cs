namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>One Evir pose; its spritemap is confined to <see cref="Artwork.EvirFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct EvirFrame(int Pose);
