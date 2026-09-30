namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed Phantoon BG2 presentation; only visual tilemap writes are editable.
/// Physical hitboxes and native frame timing remain engine-owned.
/// </summary>
public sealed class PhantoonBg2FrameCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => frames.ContentIdentity;

    private readonly EnemyBg2FrameCatalog frames;

    private PhantoonBg2FrameCatalog(EnemyBg2FrameCatalog frames) => this.frames = frames;

    internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
        frames.TryGet(pointer, out writes);

    public static PhantoonBg2FrameCatalog Load(Stream json) =>
        new(EnemyBg2FrameCatalog.Load(json, PhantoonBg2FrameDefinitions.Frames,
            PhantoonBg2FrameDefinitions.Version, "Phantoon"));
}
