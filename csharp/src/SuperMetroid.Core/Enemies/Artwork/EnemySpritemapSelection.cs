namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed enemy OAM compositions and the display choice made for each frame an enemy shows.</summary>
/// <param name="Frames">Compiled OAM parts of each authored frame.</param>
/// <param name="DisplayFrames">Visual-only mapping from each frame an enemy shows to the frame displayed for it.</param>
internal sealed record EnemySpritemapSelection(Dictionary<EnemyFrameId, EnemySpritemapParts> Frames,
    Dictionary<EnemyFrameId, EnemyFrameId> DisplayFrames);
