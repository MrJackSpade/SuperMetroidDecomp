using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// An owned image of the PPU's memories at one display publication boundary.
/// Register values and composition commands belong to the containing frame, not here.
/// </summary>
/// <remarks>
/// Capture on the simulation owner, after its ordered DMA/OAM preparation. This is
/// not an atomic reader of concurrently changing hardware. Once captured, neither
/// later simulation writes nor software-render scratch buffers can change this image.
/// Complete images deliberately avoid dependencies on discarded intermediate frames.
/// </remarks>
public sealed class PpuMemorySnapshot
{
    private readonly byte[] vram;
    private readonly ushort[] cgram;
    private readonly byte[] oam;

    /// <summary>Physical VRAM bytes, including Mode 7's interleaved map/characters.</summary>
    public ReadOnlySpan<byte> Vram => vram;

    /// <summary>Native palette words; conversion to display colors is backend work.</summary>
    public ReadOnlySpan<ushort> Cgram => cgram;

    /// <summary>Contiguous low and high OAM tables in DMA upload order.</summary>
    public ReadOnlySpan<byte> Oam => oam;

    /// <summary>
    /// Existing software OBJ kernels inspect only finalized records. This host-model
    /// metadata is preserved separately from physical OAM so migration does not change
    /// hidden-sprite or large-object wrapping behaviour.
    /// </summary>
    public int ModeledSpriteCount { get; }

    /// <summary>
    /// Copies complete memory images. No caller-owned arrays or mutable hardware
    /// references survive construction, including when loading a serialized fixture.
    /// </summary>
    public PpuMemorySnapshot(ReadOnlySpan<byte> vram, ReadOnlySpan<ushort> cgram,
        ReadOnlySpan<byte> oam, int modeledSpriteCount = SnesPpuLayout.OamSpriteCount)
    {
        if ((uint)modeledSpriteCount > SnesPpuLayout.OamSpriteCount)
            throw new ArgumentOutOfRangeException(nameof(modeledSpriteCount));
        if (vram.Length != SnesPpuLayout.VramByteCount)
            throw new ArgumentException("A frame requires a complete VRAM image.", nameof(vram));
        if (cgram.Length != SnesPpuLayout.CgramColorCount)
            throw new ArgumentException("A frame requires a complete CGRAM image.", nameof(cgram));
        if (oam.Length != SnesPpuLayout.OamUploadByteCount)
            throw new ArgumentException("A frame requires both complete OAM tables.", nameof(oam));
        this.vram = vram.ToArray();
        this.cgram = cgram.ToArray();
        this.oam = oam.ToArray();
        ModeledSpriteCount = modeledSpriteCount;
    }

    /// <summary>Captures currently published memories without changing their owners.</summary>
    public static PpuMemorySnapshot Capture(SnesVram vram, SnesCgram cgram, OamBuffer oam)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(oam);
        Span<byte> payload = stackalloc byte[SnesPpuLayout.OamUploadByteCount];
        oam.LowTable.CopyTo(payload);
        oam.HighTable.CopyTo(payload[SnesPpuLayout.OamLowTableByteCount..]);
        return new(vram.Bytes, cgram.Colors, payload, oam.LastFinalizedSpriteCount);
    }
}
