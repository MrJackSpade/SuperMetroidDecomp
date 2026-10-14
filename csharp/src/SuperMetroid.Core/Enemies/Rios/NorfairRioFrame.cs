namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One Norfair Rio pose; its spritemap is confined to <see cref="Artwork.NorfairRioFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct NorfairRioFrame(int Pose);
