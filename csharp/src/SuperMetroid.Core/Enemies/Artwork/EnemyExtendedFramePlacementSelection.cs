namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed extended enemy frames' component placements and the display choice made for each frame an enemy shows.</summary>
/// <param name="Frames">Each authored frame's component placements, in component order.</param>
/// <param name="DisplayFrames">Editable display choices keyed by the frame gameplay shows.</param>
internal sealed record EnemyExtendedFramePlacementSelection(Dictionary<EnemyFrameId, EnemyExtendedComponentPlacement[]> Frames,
    Dictionary<EnemyFrameId, EnemyFrameId> DisplayFrames);
