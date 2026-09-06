namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Owned PPU memory and back-to-front layer insertions at native visible resolution.
/// This shape models ordinary tile/OBJ composition, not main/subscreen color math.
/// </summary>
public sealed class LayeredRenderSnapshot
{
    private readonly RenderLayer[] layers;
    public PpuMemorySnapshot Memory { get; }
    public ReadOnlySpan<RenderLayer> Layers => layers;
    public byte ObjectSelection { get; }
    public byte Brightness { get; }

    public LayeredRenderSnapshot(PpuMemorySnapshot memory, ReadOnlySpan<RenderLayer> layers,
        byte objectSelection, byte brightness)
    {
        ArgumentNullException.ThrowIfNull(memory);
        if (brightness > Hardware.SnesPpuLayout.MaximumMasterBrightness) throw new ArgumentOutOfRangeException(nameof(brightness));
        // Each accepted record is sealed and value-only. Copy the sequence so a scene
        // cannot rearrange a queued frame's priority ladder after publication.
        foreach (RenderLayer layer in layers)
        {
            if (layer is not (ObjPriorityRenderLayer or ObjRenderLayer or Bg4BppRenderLayer or Bg2BppRenderLayer))
                throw new ArgumentException("Unrecognized or null render layer.", nameof(layers));
            if (layer is ObjPriorityRenderLayer { Priority: > 3 })
                throw new ArgumentException("OBJ priority must be between zero and three.", nameof(layers));
        }
        Memory = memory;
        this.layers = layers.ToArray();
        ObjectSelection = objectSelection;
        Brightness = brightness;
    }
}
