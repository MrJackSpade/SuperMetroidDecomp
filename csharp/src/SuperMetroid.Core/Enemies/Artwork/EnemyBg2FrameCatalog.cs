namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Validated, named extended-enemy BG2 presentation shared by Phantoon and
/// Draygon. The native instruction selector still determines the frame identity.
/// </summary>
internal sealed class EnemyBg2FrameCatalog
{
    /// <summary>Canonical frames retain command order, destinations and each tile-run boundary.</summary>
    internal string ContentIdentity => EnemyBg2Frames.ContentIdentity(frames);

    /// <summary>Validated ordered BG2 writes keyed by the extended frame that selects them.</summary>
    internal readonly Dictionary<EnemyFrameId, EnemyBg2TilemapWrite[]> frames;

    /// <summary>Owns the validated writes for one enemy frame family.</summary>
    internal EnemyBg2FrameCatalog(Dictionary<EnemyFrameId, EnemyBg2TilemapWrite[]> frames) =>
        this.frames = frames;

    /// <summary>Every frame's ordered tilemap writes.</summary>
    internal IEnumerable<KeyValuePair<EnemyFrameId, EnemyBg2TilemapWrite[]>> Frames => frames;

    /// <summary>Retrieves the ordered tilemap writes <paramref name="frame"/> selects, if present.</summary>
    internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
        EnemyBg2Frames.TryGet(frames, frame, out writes);
}
