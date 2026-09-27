namespace SuperMetroid.Core.Assets;

/// <summary>Editable BG2 writes for Crocomire's mixed fight-body frames.</summary>
public sealed class CrocomireBg2FrameCatalog
{
    private readonly EnemyBg2FrameCatalog frames;

    private CrocomireBg2FrameCatalog(EnemyBg2FrameCatalog frames) => this.frames = frames;

    internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
        frames.TryGet(pointer, out writes);

    public static CrocomireBg2FrameCatalog Load(Stream json) =>
        new(EnemyBg2FrameCatalog.Load(json, CrocomireBg2FrameDefinitions.Frames,
            CrocomireBg2FrameDefinitions.Version, "Crocomire"));
}
