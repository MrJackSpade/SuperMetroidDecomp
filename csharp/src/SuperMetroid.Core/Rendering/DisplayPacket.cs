using System.Collections.Immutable;
namespace SuperMetroid.Core.Rendering;

/// <summary>Closed, immutable display packet. It contains neither game objects nor backend handles.</summary>
public sealed class DisplayPacket
{
    /// <summary>Host publication identity and captured simulation counter; sequence and generation are validated as positive, while mailbox publication enforces ordering and generation membership.</summary>
    public RenderFrameIdentity Identity { get; }
    /// <summary>Immutable PPU memory and ordered layer composition when this is a layered packet; otherwise <see langword="null"/>.</summary>
    public LayeredScene? Layers { get; }
    /// <summary>Immutable Mode 7/backdrop and OBJ composition when this is a Mode 7 packet; otherwise <see langword="null"/>.</summary>
    public FloorScene? Mode7 { get; }
    /// <summary>RGBA color filling every output pixel when this is a solid-color packet; otherwise <see langword="null"/>. Outer brightness passes are still applied.</summary>
    public RgbaPixel? SolidColor { get; }
    /// <summary>Native visible output size of 256 pixels by 224 physical scanlines, including the HUD where present, before any host presentation scaling.</summary>
    public Extent16<ScreenPixels> Size { get; } = ScreenGeometry.ScreenSize;
    /// <summary>
    /// The fades the screen passes through after the scene's own brightness, in order. Do not multiply them into a single
    /// factor: each rounds its own result.
    /// </summary>
    public ImmutableArray<FadeLevel> Fades { get; }

    /// <summary>Creates a layered display packet, retaining the immutable composition and taking an owned copy of its additional ordered brightness passes; the other payload choices remain absent.</summary>
    /// <param name="identity">Publication identity with positive host sequence and generation.</param>
    /// <param name="layers">Complete immutable layered scene payload, including its scene brightness.</param>
    /// <param name="fades">The fades applied in order after the scene; empty or default for none.</param>
    public DisplayPacket(RenderFrameIdentity identity, LayeredScene layers,
        ImmutableArray<FadeLevel> fades = default)
        : this(identity, fades)
    {
        Ensure.NotNull(layers);
        Layers = layers;
    }

    /// <summary>Creates a Mode 7/OBJ display packet, retaining the immutable composition and taking an owned copy of its additional ordered brightness passes; the other payload choices remain absent.</summary>
    /// <param name="identity">Publication identity with positive host sequence and generation.</param>
    /// <param name="mode7">Complete immutable Mode 7/backdrop and OBJ payload, including its scene brightness.</param>
    /// <param name="fades">The fades applied in order after the scene; empty or default for none.</param>
    public DisplayPacket(RenderFrameIdentity identity, FloorScene mode7,
        ImmutableArray<FadeLevel> fades = default)
        : this(identity, fades)
    {
        Ensure.NotNull(mode7);
        Mode7 = mode7;
    }

    /// <summary>Creates a full-frame solid-color display packet and takes an owned copy of its ordered brightness passes; no PPU composition payload is retained.</summary>
    /// <param name="identity">Publication identity with positive host sequence and generation.</param>
    /// <param name="solidColor">RGBA value used to fill all 256-by-224 output pixels before brightness filtering.</param>
    /// <param name="fades">The fades applied in order after the scene; empty or default for none.</param>
    public DisplayPacket(RenderFrameIdentity identity, RgbaPixel solidColor,
        ImmutableArray<FadeLevel> fades = default)
        : this(identity, fades) => SolidColor = solidColor;

    /// <summary>Validates the host identity and keeps the fades.</summary>
    private DisplayPacket(RenderFrameIdentity identity, ImmutableArray<FadeLevel> fades)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(identity.Sequence);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(identity.Generation);
        Identity = identity;
        Fades = fades.IsDefault ? [] : fades;
    }
}
