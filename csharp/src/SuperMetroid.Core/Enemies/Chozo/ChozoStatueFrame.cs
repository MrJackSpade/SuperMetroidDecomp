namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One Chozo statue pose; its spritemap is confined to <see cref="Artwork.ChozoStatueFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct ChozoStatueFrame(int Pose);
