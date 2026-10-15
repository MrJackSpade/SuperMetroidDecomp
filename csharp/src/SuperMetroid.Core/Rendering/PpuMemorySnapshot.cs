using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

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
[PpuLayoutBoundary("codec: immutable VRAM/CGRAM/OAM render snapshot")]
public sealed class PpuMemorySnapshot
{
    /// <summary>Complete captured VRAM bytes, including Mode 7's interleaved planes.</summary>
    private readonly byte[] vram;
    /// <summary>Captured native CGRAM words before backend display-color conversion.</summary>
    private readonly Bgr555[] cgram;
    /// <summary>Captured low and high OAM tables in DMA upload order.</summary>
    private readonly byte[] oam;

    /// <summary>Physical VRAM bytes, including Mode 7's interleaved map/characters.</summary>
    public ReadOnlySpan<byte> Vram => vram;

    /// <summary>Native palette words; conversion to display colors is backend work.</summary>
    public ReadOnlySpan<Bgr555> Cgram => cgram;

    /// <summary>Contiguous low and high OAM tables in DMA upload order.</summary>
    public ReadOnlySpan<byte> Oam => oam;

    /// <summary>
    /// Existing software OBJ kernels inspect only finalized records. This host-model
    /// metadata is preserved separately from physical OAM so migration does not change
    /// hidden-sprite or large-object wrapping behaviour.
    /// </summary>
    public int ModeledSpriteCount { get; }

    /// <summary>The draws as a plain array; debugger state (a captured frontend display) cannot hold immutable collections.</summary>
    private readonly OrderedSpriteDraw[] spriteDraws;

    /// <summary>The displayed whole-frame sprite draws, each placed among the OAM sprites by its ordinal.</summary>
    public ImmutableArray<OrderedSpriteDraw> SpriteDraws => ImmutableCollectionsMarshal.AsImmutableArray(spriteDraws);

    /// <summary>
    /// Copies complete memory images. No caller-owned arrays or mutable hardware
    /// references survive construction, including when loading a serialized fixture.
    /// </summary>
    public PpuMemorySnapshot(ReadOnlySpan<byte> vram, ReadOnlySpan<Bgr555> cgram,
        ReadOnlySpan<byte> oam, int modeledSpriteCount = SnesPpuLayout.OamSpriteCount)
        : this(vram.ToArray(), cgram.ToArray(), oam.ToArray(), modeledSpriteCount, [])
    {
    }

    /// <summary>Copies complete memory images and keeps the frame's whole-frame sprite draws.</summary>
    public PpuMemorySnapshot(ReadOnlySpan<byte> vram, ReadOnlySpan<Bgr555> cgram,
        ReadOnlySpan<byte> oam, int modeledSpriteCount, ImmutableArray<OrderedSpriteDraw> spriteDraws)
        : this(vram.ToArray(), cgram.ToArray(), oam.ToArray(), modeledSpriteCount, spriteDraws)
    {
    }

    /// <summary>
    /// Takes ownership of arrays nothing will write again. Capture passes
    /// <see cref="SnesVram.CaptureImage"/>, so snapshots of an unchanged VRAM share one image.
    /// </summary>
    private PpuMemorySnapshot(byte[] vram, Bgr555[] cgram, byte[] oam, int modeledSpriteCount,
        ImmutableArray<OrderedSpriteDraw> spriteDraws)
    {
        if ((uint)modeledSpriteCount > SnesPpuLayout.OamSpriteCount)
            throw new ArgumentOutOfRangeException(nameof(modeledSpriteCount));
        if (spriteDraws.IsDefault)
            throw new ArgumentException("A frame requires a draw list.", nameof(spriteDraws));
        foreach (OrderedSpriteDraw draw in spriteDraws)
            if ((uint)draw.OamOrdinal > SnesPpuLayout.OamSpriteCount || draw.Draw is null)
                throw new ArgumentException("Each sprite draw needs an OAM ordinal of 0..128 and an entry.", nameof(spriteDraws));
        if (vram.Length != SnesPpuLayout.VramByteCount)
            throw new ArgumentException("A frame requires a complete VRAM image.", nameof(vram));
        if (cgram.Length != SnesPpuLayout.CgramColorCount)
            throw new ArgumentException("A frame requires a complete CGRAM image.", nameof(cgram));
        if (oam.Length != SnesPpuLayout.OamUploadByteCount)
            throw new ArgumentException("A frame requires both complete OAM tables.", nameof(oam));
        this.vram = vram;
        this.cgram = cgram;
        this.oam = oam;
        ModeledSpriteCount = modeledSpriteCount;
        this.spriteDraws = ImmutableCollectionsMarshal.AsArray(spriteDraws) ?? throw new ArgumentException(
            "A frame requires a draw list.", nameof(spriteDraws));
    }

    /// <summary>Captures currently published memories without changing their owners.</summary>
    public static PpuMemorySnapshot Capture(SnesVram vram, SnesCgram cgram, OamBuffer oam)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.NotNull(oam);
        Span<byte> payload = stackalloc byte[SnesPpuLayout.OamUploadByteCount];
        oam.LowTable.CopyTo(payload);
        oam.HighTable.CopyTo(payload[SnesPpuLayout.OamLowTableByteCount..]);
        return new(vram.CaptureImage(), cgram.Colors.ToArray(), payload.ToArray(), oam.LastFinalizedSpriteCount, []);
    }

    /// <summary>Captures currently published memories and the displayed frame's whole-frame sprite draws.</summary>
    public static PpuMemorySnapshot Capture(SnesVram vram, SnesCgram cgram, SpriteFrameBuffer sprites)
    {
        Ensure.NotNull(sprites);
        PpuMemorySnapshot memories = Capture(vram, cgram, sprites.Oam);
        return new(memories.vram, memories.cgram, memories.oam, memories.ModeledSpriteCount, sprites.Draws);
    }
}
