namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed extended enemy frames, the ones with a sprite picture, and the display choice made for each frame an enemy shows.</summary>
/// <param name="Frames">Every authored frame.</param>
/// <param name="Pictured">The authored frames with at least one sprite piece, each pictured whole on its enemy's sheets.</param>
/// <param name="DisplayFrames">Editable display choices keyed by the frame gameplay shows.</param>
internal sealed record EnemyExtendedFrameDisplaySelection(HashSet<EnemyFrameId> Frames, HashSet<EnemyFrameId> Pictured,
    Dictionary<EnemyFrameId, EnemyFrameId> DisplayFrames);
