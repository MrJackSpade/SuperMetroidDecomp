namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed extended enemy frames as gameplay draws them (#1463): each frame's component placements and the frame
/// displayed for each frame an enemy shows. The components' pictures are the installed enemy sheets' frames; the
/// compositions they were composed from belong to the import pipeline. Hitbox records and callback pointers remain
/// application-owned.
/// </summary>
public sealed class EnemyExtendedFramePlacementCatalog
{
    /// <summary>The component placements and display bindings.</summary>
    internal readonly EnemyExtendedFramePlacementSelection selection;

    /// <summary>Retains the placements and same-family display bindings the import pipeline validated.</summary>
    internal EnemyExtendedFramePlacementCatalog(
        Dictionary<EnemyFrameId, EnemyExtendedComponentPlacement[]> frames,
        Dictionary<EnemyFrameId, EnemyFrameId> displayFrames) =>
        selection = new(frames, displayFrames);

    /// <summary>Maps the frame gameplay shows to the component placements displayed for it.</summary>
    internal bool TryGetDisplay(EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedComponentPlacement> components) =>
        EnemyExtendedFramePlacements.TryGetDisplay(this, frame, out components);

    /// <summary>The frame displayed for <paramref name="frame"/>, whose BG2 half accompanies the displayed components.</summary>
    internal EnemyFrameId GetDisplayFrame(EnemyFrameId frame) => EnemyExtendedFramePlacements.GetDisplayFrame(this, frame);
}
