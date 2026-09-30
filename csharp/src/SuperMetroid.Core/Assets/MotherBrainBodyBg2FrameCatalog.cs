namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed BG2 half of Mother Brain's sixteen mixed body poses. Visual tile
/// references and ordered runs are editable; body position, scroll, instruction
/// timing, AI and collision geometry remain compiled gameplay state.
/// </summary>
public sealed class MotherBrainBodyBg2FrameCatalog
{
    public string ContentIdentity => frames.ContentIdentity;
    private readonly EnemyBg2FrameCatalog frames;

    private MotherBrainBodyBg2FrameCatalog(EnemyBg2FrameCatalog frames) => this.frames = frames;

    internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
        frames.TryGet(pointer, out writes);

    public static MotherBrainBodyBg2FrameCatalog Load(Stream json) => new(
        EnemyBg2FrameCatalog.Load(json, MotherBrainBodyVisualDefinitions.Bg2Frames,
            MotherBrainBodyVisualDefinitions.Bg2Version, "Mother Brain body"));
}
