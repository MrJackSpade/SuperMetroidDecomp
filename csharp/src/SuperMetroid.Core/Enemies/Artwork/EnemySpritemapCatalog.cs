namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Installed enemy OAM compositions keyed by native visual identity.</summary>
public sealed class EnemySpritemapCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => EnemySpritemaps.ContentIdentity(this);

    /// <summary>Upper bound on sprite pieces accepted from one editable enemy composition.</summary>
    internal const int MaximumParts = 128;
    /// <summary>Character-sheet width in eight-pixel tiles for composition tile references.</summary>
    internal const int TileColumns = 16;
    /// <summary>Character-sheet height in eight-pixel tiles for composition tile references.</summary>
    internal const int TileRows = 32;

    /// <summary>Compiled OAM parts of each authored frame.</summary>
    internal readonly Dictionary<EnemyFrameId, EnemySpritemapParts> frames;
    /// <summary>Visual-only mapping from each frame an enemy shows to the frame displayed for it.</summary>
    internal readonly Dictionary<EnemyFrameId, EnemyFrameId> displayFrames;

    /// <summary>Retains validated frame compositions and display selections.</summary>
    internal EnemySpritemapCatalog(Dictionary<EnemyFrameId, EnemySpritemapParts> frames,
        Dictionary<EnemyFrameId, EnemyFrameId> displayFrames) =>
        (this.frames, this.displayFrames) = (frames, displayFrames);

    /// <summary>
    /// Resolves an editable presentation binding without changing the frame retained by enemy AI, hitbox selection, or
    /// instruction timing.
    /// </summary>
    /// <param name="frame">The frame gameplay shows.</param>
    /// <param name="parts">Installed OAM composition selected by the binding, or <see cref="EnemySpritemapParts.Empty"/> when no bound frame is installed.</param>
    /// <returns>True when both a display binding and its selected composition exist.</returns>
    public bool TryGetDisplay(EnemyFrameId frame, out EnemySpritemapParts parts) => EnemySpritemaps.TryGetDisplay(this, frame, out parts);
}
