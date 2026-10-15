namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>One Cacatac pose; its spritemap is confined to <see cref="Artwork.CacatacFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct CacatacFrame(int Pose);
