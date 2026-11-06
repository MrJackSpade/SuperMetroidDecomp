namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One installed enemy frame and the name its artwork document gives it: a composition, or the background half of an extended frame.</summary>
internal readonly record struct EnemySpritemapDefinition(EnemyFrameId Frame, string Name);
