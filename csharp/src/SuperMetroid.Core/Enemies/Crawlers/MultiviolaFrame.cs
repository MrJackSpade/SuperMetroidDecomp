namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One Multiviola pose; its spritemap is confined to <see cref="Artwork.MultiviolaFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct MultiviolaFrame(int Pose);
