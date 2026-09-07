namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Owned PPU memory and back-to-front layer insertions at native visible resolution.
/// Explicit operations include tile/OBJ insertion, captured color math and bounded scene windows.
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
        // Each accepted record is sealed and owns any array data. Copy the sequence so a scene
        // cannot rearrange a queued frame's priority ladder after publication.
        foreach (RenderLayer layer in layers)
        {
            if (layer is not (ObjPriorityRenderLayer or ObjRenderLayer or Bg4BppRenderLayer or Bg2BppRenderLayer or Bg2BppViewportRenderLayer or Mode7RenderLayer or FixedColorAddRenderLayer or OrdinaryGameplayRenderLayer or ScanlineColorAddRenderLayer or MessageBoxRenderLayer or Bg2BppColorMathRenderLayer or Mode7GameplayRenderLayer or BgSubscreenAddRenderLayer or WindowedSceneRenderLayer or XrayWindowRenderLayer))
                throw new ArgumentException("Unrecognized or null render layer.", nameof(layers));
            if (layer is FixedColorAddRenderLayer fixedColor && (fixedColor.Red > 31 || fixedColor.Green > 31 || fixedColor.Blue > 31))
                throw new ArgumentException("Fixed color components must be five-bit values.", nameof(layers));
            if (layer is ObjPriorityRenderLayer { Priority: > 3 })
                throw new ArgumentException("OBJ priority must be between zero and three.", nameof(layers));
            if (layer is Bg4BppRenderLayer bg4 &&
                (bg4.MapWidthTiles is not (32 or 64) || bg4.MapHeightTiles is not (32 or 64)))
                throw new ArgumentException("BGSC geometry must be 32 or 64 tiles on each axis.", nameof(layers));
            if (layer is BgSubscreenAddRenderLayer { MainCoverage: { } coverage } &&
                (coverage.MapWidthTiles is not (32 or 64) || coverage.MapHeightTiles is not (32 or 64)))
                throw new ArgumentException("Coverage BGSC geometry must be 32 or 64 tiles on each axis.", nameof(layers));
            if (layer is Bg2BppRenderLayer bg2 && bg2.RowCount !=
                Hardware.SnesPpuLayout.ScreenHeightPixels / Hardware.SnesPpuLayout.BackgroundTileSizePixels)
                throw new ArgumentException("This full-frame 2-bpp operation requires exactly the visible tile rows.", nameof(layers));
        }
        for (int i = 1; i < layers.Length; i++)
            if (layers[i] is OrdinaryGameplayRenderLayer or Mode7GameplayRenderLayer)
                throw new ArgumentException("The fused gameplay base must be the first operation.", nameof(layers));
        Memory = memory;
        this.layers = layers.ToArray();
        ObjectSelection = objectSelection;
        Brightness = brightness;
    }
}
