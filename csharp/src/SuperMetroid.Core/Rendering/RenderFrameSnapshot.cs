using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Host identity, independent of the cartridge counter's wrap and debugger-state rewind.
/// Sequence increases for every published host frame; generation increases on load/reset.
/// </summary>
/// <param name="Sequence">Positive host publication sequence, kept monotonic across simulation counter wraps and load/reset generations.</param>
/// <param name="Generation">Positive host load/reset generation used to reject stale queued display packets.</param>
/// <param name="SimulationFrame">Captured native-width simulation frame counter, which may wrap or rewind and is not a publication-order key.</param>
public readonly record struct RenderFrameIdentity(long Sequence, long Generation, ushort SimulationFrame);

/// <summary>Closed, immutable display packet. It contains neither game objects nor backend handles.</summary>
public sealed class RenderFrameSnapshot
{
    /// <summary>Host publication identity and captured simulation counter; sequence and generation are validated as positive, while mailbox publication enforces ordering and generation membership.</summary>
    public RenderFrameIdentity Identity { get; }
    /// <summary>Immutable PPU memory and ordered layer composition when this is a layered packet; otherwise <see langword="null"/>.</summary>
    public LayeredRenderSnapshot? Layers { get; }
    /// <summary>Immutable Mode 7/backdrop and OBJ composition when this is a Mode 7 packet; otherwise <see langword="null"/>.</summary>
    public Mode7ObjRenderSnapshot? Mode7 { get; }
    /// <summary>RGBA color filling every output pixel when this is a solid-color packet; otherwise <see langword="null"/>. Outer brightness passes are still applied.</summary>
    public Rgba32? SolidColor { get; }
    /// <summary>Native visible output width of 256 pixels, before any host presentation scaling.</summary>
    public int Width { get; } = SnesPpuLayout.ScreenWidthPixels;
    /// <summary>Native visible output height of 224 physical scanlines, including the HUD where present.</summary>
    public int Height { get; } = SnesPpuLayout.ScreenHeightPixels;
    /// <summary>Owned ordered post-scene brightness levels, preserving per-pass rounding.</summary>
    private readonly byte[] brightnessPasses;

    /// <summary>
    /// Outer dispatcher fades in original order, after the scene's own brightness.
    /// Do not multiply these into a single factor: each pass has integer rounding.
    /// </summary>
    public ReadOnlySpan<byte> BrightnessPasses => brightnessPasses;

    /// <summary>Creates a layered display packet, retaining the immutable composition and taking an owned copy of its additional ordered brightness passes; the other payload choices remain absent.</summary>
    /// <param name="identity">Publication identity with positive host sequence and generation.</param>
    /// <param name="layers">Complete immutable layered scene payload, including its scene brightness.</param>
    /// <param name="brightnessPasses">Additional master-brightness levels 0..15, applied in order after scene rendering; an empty span adds no passes.</param>
    public RenderFrameSnapshot(RenderFrameIdentity identity, LayeredRenderSnapshot layers,
        ReadOnlySpan<byte> brightnessPasses = default)
        : this(identity, brightnessPasses)
    {
        ArgumentNullException.ThrowIfNull(layers);
        Layers = layers;
    }

    /// <summary>Creates a Mode 7/OBJ display packet, retaining the immutable composition and taking an owned copy of its additional ordered brightness passes; the other payload choices remain absent.</summary>
    /// <param name="identity">Publication identity with positive host sequence and generation.</param>
    /// <param name="mode7">Complete immutable Mode 7/backdrop and OBJ payload, including its scene brightness.</param>
    /// <param name="brightnessPasses">Additional master-brightness levels 0..15, applied in order after scene rendering; an empty span adds no passes.</param>
    public RenderFrameSnapshot(RenderFrameIdentity identity, Mode7ObjRenderSnapshot mode7,
        ReadOnlySpan<byte> brightnessPasses = default)
        : this(identity, brightnessPasses)
    {
        ArgumentNullException.ThrowIfNull(mode7);
        Mode7 = mode7;
    }

    /// <summary>Creates a full-frame solid-color display packet and takes an owned copy of its ordered brightness passes; no PPU composition payload is retained.</summary>
    /// <param name="identity">Publication identity with positive host sequence and generation.</param>
    /// <param name="solidColor">RGBA value used to fill all 256-by-224 output pixels before brightness filtering.</param>
    /// <param name="brightnessPasses">Master-brightness levels 0..15, applied successively with each pass's integer rounding; an empty span leaves the supplied color unchanged.</param>
    public RenderFrameSnapshot(RenderFrameIdentity identity, Rgba32 solidColor,
        ReadOnlySpan<byte> brightnessPasses = default)
        : this(identity, brightnessPasses) => SolidColor = solidColor;

    private RenderFrameSnapshot(RenderFrameIdentity identity, ReadOnlySpan<byte> brightnessPasses)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(identity.Sequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(identity.Generation);
        foreach (byte level in brightnessPasses)
            if (level > SnesPpuLayout.MaximumMasterBrightness)
                throw new ArgumentOutOfRangeException(nameof(brightnessPasses));
        Identity = identity;
        this.brightnessPasses = brightnessPasses.ToArray();
    }
}
