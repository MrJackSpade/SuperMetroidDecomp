namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>One Mochtroid pose; its spritemap is confined to <see cref="Artwork.MochtroidFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct MochtroidFrame(int Pose);
