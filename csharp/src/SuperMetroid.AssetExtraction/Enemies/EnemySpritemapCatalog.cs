using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Installed enemy OAM compositions keyed by native visual identity, which the import pipeline composes the enemy sheets'
/// pictures from (#1463).
/// </summary>
public sealed class EnemySpritemapCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => EnemySpritemaps.ContentIdentity(this);

    /// <summary>The compiled frame compositions and display selections.</summary>
    internal readonly EnemySpritemapSelection selection;

    /// <summary>Retains validated frame compositions and display selections.</summary>
    internal EnemySpritemapCatalog(Dictionary<EnemyFrameId, EnemySpritemapParts> frames,
        Dictionary<EnemyFrameId, EnemyFrameId> displayFrames) =>
        selection = new(frames, displayFrames);

    /// <summary>
    /// Resolves an editable presentation binding without changing the frame retained by enemy AI, hitbox selection, or
    /// instruction timing.
    /// </summary>
    /// <param name="frame">The frame gameplay shows.</param>
    /// <param name="parts">Installed OAM composition selected by the binding, or <see cref="EnemySpritemapParts.Empty"/> when no bound frame is installed.</param>
    /// <returns>True when both a display binding and its selected composition exist.</returns>
    public bool TryGetDisplay(EnemyFrameId frame, out EnemySpritemapParts parts) => EnemySpritemaps.TryGetDisplay(this, frame, out parts);
}
