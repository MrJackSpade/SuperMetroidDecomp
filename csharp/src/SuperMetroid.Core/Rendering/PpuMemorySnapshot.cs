using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// An owned image of the PPU's memories and the frame's sprite draws at one display publication boundary.
/// Register values and composition commands belong to the containing frame, not here.
/// </summary>
/// <remarks>
/// Capture on the simulation owner, after its ordered DMA preparation. This is not an atomic reader of concurrently
/// changing hardware. Once captured, neither later simulation writes nor software-render scratch buffers can change
/// this image. Complete images deliberately avoid dependencies on discarded intermediate frames. Sprites are whole-frame
/// draws only; a frontend screen's OAM storage is converted when it is captured (<see cref="FrontendOamDraws"/>).
/// </remarks>
[PpuLayoutBoundary("codec: immutable VRAM/CGRAM render snapshot")]
public sealed class PpuMemorySnapshot
{
    /// <summary>Complete captured VRAM bytes, including Mode 7's interleaved planes.</summary>
    private readonly byte[] vram;
    /// <summary>Captured native CGRAM words before backend display-color conversion.</summary>
    private readonly Bgr555[] cgram;

    /// <summary>Physical VRAM bytes, including Mode 7's interleaved map/characters.</summary>
    public ReadOnlySpan<byte> Vram => vram;

    /// <summary>Native palette words; conversion to display colors is backend work.</summary>
    public ReadOnlySpan<Bgr555> Cgram => cgram;

    /// <summary>The draws as a plain array; debugger state (a captured frontend display) cannot hold immutable collections.</summary>
    private readonly SpriteDraw[] spriteDraws;

    /// <summary>The displayed whole-frame sprite draws in emission order, frontmost first.</summary>
    public ImmutableArray<SpriteDraw> SpriteDraws => ImmutableCollectionsMarshal.AsImmutableArray(spriteDraws);

    /// <summary>Copies complete memory images and keeps the frame's whole-frame sprite draws.</summary>
    public PpuMemorySnapshot(ReadOnlySpan<byte> vram, ReadOnlySpan<Bgr555> cgram, ImmutableArray<SpriteDraw> spriteDraws)
        : this(vram.ToArray(), cgram.ToArray(), spriteDraws)
    {
    }

    /// <summary>
    /// Takes ownership of arrays nothing will write again. Capture passes
    /// <see cref="SnesVram.CaptureImage"/>, so snapshots of an unchanged VRAM share one image.
    /// </summary>
    private PpuMemorySnapshot(byte[] vram, Bgr555[] cgram, ImmutableArray<SpriteDraw> spriteDraws)
    {
        if (spriteDraws.IsDefault)
            throw new ArgumentException("A frame requires a draw list.", nameof(spriteDraws));
        foreach (SpriteDraw draw in spriteDraws)
            if (draw is null)
                throw new ArgumentException("Each sprite draw needs an entry.", nameof(spriteDraws));
        Ensure.LengthEqual<byte>(vram, SnesPpuLayout.VramByteCount);
        Ensure.LengthEqual<Bgr555>(cgram, SnesPpuLayout.CgramColorCount);
        this.vram = vram;
        this.cgram = cgram;
        this.spriteDraws = ImmutableCollectionsMarshal.AsArray(spriteDraws) ?? throw new ArgumentException(
            "A frame requires a draw list.", nameof(spriteDraws));
    }

    /// <summary>
    /// Captures gameplay's memories with its background character artwork composed over the VRAM image (#1350): each
    /// binding shows over the words VRAM has not written since it was made.
    /// </summary>
    public static PpuMemorySnapshot Capture(SnesVram vram, IPaletteColors cgram, SpriteFrameBuffer sprites, BgArtworkBindings backgrounds)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.NotNull(sprites);
        Ensure.NotNull(backgrounds);
        byte[] image = vram.CaptureImage().ToArray();
        backgrounds.ComposeOver(image);
        return new(image, cgram.Colors.ToArray(), sprites.Draws);
    }

    /// <summary>Captures a frontend screen's VRAM and CGRAM with the sprite entries it publishes.</summary>
    /// <param name="vram">The screen's VRAM.</param>
    /// <param name="cgram">The screen's CGRAM.</param>
    /// <param name="draws">The screen's sprite entries, frontmost first.</param>
    public static PpuMemorySnapshot CaptureFrontend(SnesVram vram, SnesCgram cgram, ImmutableArray<SpriteDraw> draws)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        return new(vram.CaptureImage(), cgram.Colors.ToArray(), draws);
    }
}
