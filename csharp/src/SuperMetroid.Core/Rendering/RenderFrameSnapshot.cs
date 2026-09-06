using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Host identity, independent of the cartridge counter's wrap and debugger-state rewind.
/// Sequence increases for every published host frame; generation increases on load/reset.
/// </summary>
public readonly record struct RenderFrameIdentity(long Sequence, long Generation, ushort SimulationFrame);

/// <summary>Closed, immutable display packet. It contains neither game objects nor backend handles.</summary>
public sealed class RenderFrameSnapshot
{
    public RenderFrameIdentity Identity { get; }
    public LayeredRenderSnapshot? Layers { get; }
    public Mode7ObjRenderSnapshot? Mode7 { get; }
    public Rgba32? SolidColor { get; }
    public int Width { get; } = SnesPpuLayout.ScreenWidthPixels;
    public int Height { get; } = SnesPpuLayout.ScreenHeightPixels;
    private readonly byte[] brightnessPasses;

    /// <summary>
    /// Outer dispatcher fades in original order, after the scene's own brightness.
    /// Do not multiply these into a single factor: each pass has integer rounding.
    /// </summary>
    public ReadOnlySpan<byte> BrightnessPasses => brightnessPasses;

    public RenderFrameSnapshot(RenderFrameIdentity identity, LayeredRenderSnapshot layers,
        ReadOnlySpan<byte> brightnessPasses = default)
        : this(identity, brightnessPasses)
    {
        ArgumentNullException.ThrowIfNull(layers);
        Layers = layers;
    }

    public RenderFrameSnapshot(RenderFrameIdentity identity, Mode7ObjRenderSnapshot mode7,
        ReadOnlySpan<byte> brightnessPasses = default)
        : this(identity, brightnessPasses)
    {
        ArgumentNullException.ThrowIfNull(mode7);
        Mode7 = mode7;
    }

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
