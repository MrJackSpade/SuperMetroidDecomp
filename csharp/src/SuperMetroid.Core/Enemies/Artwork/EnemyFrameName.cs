namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One installed enemy frame and the name its artwork document gives it: a frame's picture, or the half of one a boss's body shows.</summary>
internal readonly partial record struct EnemyFrameName(EnemyFrameId Frame, string Name);
