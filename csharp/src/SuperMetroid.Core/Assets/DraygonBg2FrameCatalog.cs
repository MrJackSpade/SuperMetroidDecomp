namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed Draygon BG2 tilemap presentation. The $A5 instruction selector,
/// collision geometry, and callback records are not editable visual data.
/// </summary>
public sealed class DraygonBg2FrameCatalog
{
    private readonly EnemyBg2FrameCatalog frames;

    private DraygonBg2FrameCatalog(EnemyBg2FrameCatalog frames) => this.frames = frames;

    internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
        frames.TryGet(pointer, out writes);

    public static DraygonBg2FrameCatalog Load(Stream json) =>
        new(EnemyBg2FrameCatalog.Load(json, DraygonBg2FrameDefinitions.Frames,
            DraygonBg2FrameDefinitions.Version, "Draygon"));
}
