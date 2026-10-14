namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One crawler pose; its spritemap is confined to <see cref="Artwork.CrawlerFrameDefinitions"/>.</summary>
/// <param name="Pose">The pose, in ascending spritemap order.</param>
internal readonly record struct CrawlerFrame(int Pose);
