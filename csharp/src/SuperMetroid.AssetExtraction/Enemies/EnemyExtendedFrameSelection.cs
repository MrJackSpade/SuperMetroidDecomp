using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The installed extended enemy frame compositions and the display choice made for each frame an enemy shows.</summary>
/// <param name="Frames">Each authored frame's compiled OAM components.</param>
/// <param name="DisplayFrames">Editable display choices keyed by the frame gameplay shows.</param>
internal sealed record EnemyExtendedFrameSelection(Dictionary<EnemyFrameId, EnemyExtendedDrawComponent[]> Frames,
    Dictionary<EnemyFrameId, EnemyFrameId> DisplayFrames);
