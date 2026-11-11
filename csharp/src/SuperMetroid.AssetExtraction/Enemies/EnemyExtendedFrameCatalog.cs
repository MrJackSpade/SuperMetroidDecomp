using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Installed visual compositions for extended enemy frames as the import pipeline holds them (#1463): each piece's
/// placement and OAM parts, which the enemy sheet composer composes each frame's one whole picture from (#1513), and the
/// display bindings, which gameplay reads with every other installed frame's (<see cref="InstalledEnemyFrameDisplays"/>).
/// Hitbox records and callback pointers remain application-owned and never come from this JSON.
/// </summary>
public sealed class EnemyExtendedFrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => EnemyExtendedFrames.ContentIdentity(this);

    /// <summary>The compiled frame components and display bindings.</summary>
    internal readonly EnemyExtendedFrameSelection selection;

    /// <summary>Retains validated visual components and same-family display bindings.</summary>
    internal EnemyExtendedFrameCatalog(
        Dictionary<EnemyFrameId, EnemyExtendedDrawComponent[]> frames,
        Dictionary<EnemyFrameId, EnemyFrameId> displayFrames) =>
        selection = new(frames, displayFrames);

    /// <summary>Maps the frame gameplay shows to the editable draw components displayed for it.</summary>
    internal bool TryGetDisplay(EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) =>
        EnemyExtendedFrames.TryGetDisplay(this, frame, out components);
}
