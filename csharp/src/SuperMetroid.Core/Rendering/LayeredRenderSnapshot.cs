namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Owned PPU memory and back-to-front layer insertions at native visible resolution.
/// Explicit operations include tile/OBJ insertion, captured color math and bounded scene windows.
/// </summary>
public sealed class LayeredRenderSnapshot
{
    private readonly RenderLayer[] layers;
    /// <summary>Immutable VRAM, CGRAM, and finalized OAM image retained by reference, not copied again by this packet.</summary>
    public PpuMemorySnapshot Memory { get; }
    /// <summary>Owned operation sequence in back-to-front application order on the 256-by-224-pixel visible viewport.</summary>
    public ReadOnlySpan<RenderLayer> Layers => layers;
    /// <summary>Raw $2101 OBSEL byte selecting OBJ character bases, name-table offset, and small/large size pair.</summary>
    public byte ObjectSelection { get; }
    /// <summary>Master brightness 0..15, applied after all layer operations; zero produces black.</summary>
    public byte Brightness { get; }

    /// <summary>Retains immutable memory and layer records while copying their ordering into packet-owned storage.</summary>
    /// <param name="memory">Complete captured PPU memory, independent of later simulation writes.</param>
    /// <param name="layers">Supported immutable operations in execution order; an empty sequence displays the backdrop, and a fused gameplay base must be first.</param>
    /// <param name="objectSelection">Unmodified OBSEL register byte; all byte values are retained.</param>
    /// <param name="brightness">Final master brightness, from 0 through 15 inclusive.</param>
    /// <exception cref="ArgumentNullException"><paramref name="memory"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> exceeds 15.</exception>
    /// <exception cref="ArgumentException">A layer is null or unsupported, has invalid geometry, priorities or color values, selects incompatible color math, or places a fused gameplay base after another operation.</exception>
    public LayeredRenderSnapshot(PpuMemorySnapshot memory, ReadOnlySpan<RenderLayer> layers,
        byte objectSelection, byte brightness)
    {
        ArgumentNullException.ThrowIfNull(memory);
        if (brightness > Hardware.SnesPpuLayout.MaximumMasterBrightness) throw new ArgumentOutOfRangeException(nameof(brightness));
        // Each accepted record is sealed and owns any array data. Copy the sequence so a scene
        // cannot rearrange a queued frame's priority ladder after publication.
        foreach (RenderLayer layer in layers)
        {
            if (layer is not (ObjPriorityRenderLayer or ObjRenderLayer or Bg4BppRenderLayer or Bg2BppRenderLayer or Bg2BppViewportRenderLayer or Mode7RenderLayer or FixedColorAddRenderLayer or OrdinaryGameplayRenderLayer or ScanlineColorAddRenderLayer or MessageBoxRenderLayer or Bg2BppColorMathRenderLayer or Mode7GameplayRenderLayer or BgSubscreenAddRenderLayer or WindowedSceneRenderLayer or GameplayColorMathRenderLayer))
                throw new ArgumentException("Unrecognized or null render layer.", nameof(layers));
            if (layer is FixedColorAddRenderLayer fixedColor && (fixedColor.Red > 31 || fixedColor.Green > 31 || fixedColor.Blue > 31))
                throw new ArgumentException("Fixed color components must be five-bit values.", nameof(layers));
            if (layer is ObjPriorityRenderLayer { Priority: > 3 })
                throw new ArgumentException("OBJ priority must be between zero and three.", nameof(layers));
            if (layer is Mode7RenderLayer { SubtractObjSubscreen: true, AddBg1Subscreen: true })
                throw new ArgumentException("Mode 7 cannot select two different color-math operations.", nameof(layers));
            if (layer is BgSubscreenAddRenderLayer { FourBpp: false, VerticalScroll: not 0 })
                throw new ArgumentException("Scrolled subscreen descriptors require the four-bit viewport sampler.", nameof(layers));
            if (layer is ObjPriorityRenderLayer { FixedColor: { } priorityColor } &&
                (priorityColor.Red > 31 || priorityColor.Green > 31 || priorityColor.Blue > 31))
                throw new ArgumentException("OBJ fixed color components must be five-bit values.", nameof(layers));
            if (layer is ObjRenderLayer { FixedColor: { } objColor } obj &&
                (obj.AddToScreen || objColor.Red > 31 || objColor.Green > 31 || objColor.Blue > 31))
                throw new ArgumentException("OBJ fixed color requires replacing OBJ and five-bit components.", nameof(layers));
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
            if (layers[i] is OrdinaryGameplayRenderLayer or Mode7GameplayRenderLayer or GameplayColorMathRenderLayer)
                throw new ArgumentException("The fused gameplay base must be the first operation.", nameof(layers));
        Memory = memory;
        this.layers = layers.ToArray();
        ObjectSelection = objectSelection;
        Brightness = brightness;
    }
}
