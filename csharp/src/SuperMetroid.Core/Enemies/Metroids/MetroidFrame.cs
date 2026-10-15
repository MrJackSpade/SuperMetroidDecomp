namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>One Metroid pose; its spritemap is confined to <see cref="Artwork.MetroidFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct MetroidFrame(int Pose);
