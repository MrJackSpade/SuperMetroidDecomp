using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Installed visual compositions for extended enemy frames as the import pipeline holds them (#1463): each component's
/// OAM parts, which the enemy sheet composer composes the components' pictures from, and the placements gameplay reads.
/// Hitbox records and callback pointers remain application-owned and never come from this JSON.
/// </summary>
public sealed class EnemyExtendedFrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => EnemyExtendedFrames.ContentIdentity(this);

    /// <summary>The compiled frame components and display bindings.</summary>
    internal readonly EnemyExtendedFrameSelection selection;

    /// <summary>Retains validated visual components and same-family display bindings, and the placements gameplay reads from them.</summary>
    internal EnemyExtendedFrameCatalog(
        Dictionary<EnemyFrameId, EnemyExtendedDrawComponent[]> frames,
        Dictionary<EnemyFrameId, EnemyFrameId> displayFrames)
    {
        selection = new(frames, displayFrames);
        Placements = EnemyExtendedFrames.Placements(selection);
    }

    /// <summary>Each frame's component placements and the display bindings, as gameplay draws the components' pictures.</summary>
    public EnemyExtendedFramePlacementCatalog Placements { get; }

    /// <summary>Maps the frame gameplay shows to the editable draw components displayed for it.</summary>
    internal bool TryGetDisplay(EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) =>
        EnemyExtendedFrames.TryGetDisplay(this, frame, out components);
}
