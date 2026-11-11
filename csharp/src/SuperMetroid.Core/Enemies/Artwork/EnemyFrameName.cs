namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One installed enemy frame and the name its artwork document gives it: a frame composed of one picture or of placed pieces, or the boss-body half of one.</summary>
internal readonly record struct EnemyFrameName(EnemyFrameId Frame, string Name);
