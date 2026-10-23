using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What a frame paints with at one display publication boundary (#1403): its 256 colours and its whole-frame sprite
/// draws. The layers and effects that use them belong to the containing frame, not here.
/// </summary>
/// <remarks>
/// Capture on the simulation owner. Once captured, neither later simulation writes nor software-render scratch buffers
/// can change it. Sprites are whole-frame draws only; a frontend screen's OAM storage is converted when it is captured
/// (the extraction pipeline's ObjCharacterImage).
/// </remarks>
public sealed class FramePaint
{
    /// <summary>The frame's colours, in palette order.</summary>
    private readonly Bgr555[] colors;

    /// <summary>The draws as a plain array; debugger state (a captured frontend display) cannot hold immutable collections.</summary>
    private readonly SpriteDraw[] spriteDraws;

    /// <summary>Copies the colours and keeps the frame's whole-frame sprite draws.</summary>
    /// <exception cref="ArgumentException">There are not 256 colours, or the draw list is missing or holds a missing draw.</exception>
    public FramePaint(ReadOnlySpan<Bgr555> colors, ImmutableArray<SpriteDraw> spriteDraws)
    {
        if (spriteDraws.IsDefault)
            throw new ArgumentException("A frame requires a draw list.", nameof(spriteDraws));
        foreach (SpriteDraw draw in spriteDraws)
            if (draw is null)
                throw new ArgumentException("Each sprite draw needs an entry.", nameof(spriteDraws));
        Ensure.LengthEqual(colors, ScreenPalette.ColorCount);
        this.colors = colors.ToArray();
        this.spriteDraws = ImmutableCollectionsMarshal.AsArray(DrawColours.ResolveAll(spriteDraws, colors)) ?? throw new ArgumentException(
            "A frame requires a draw list.", nameof(spriteDraws));
    }

    /// <summary>The frame's 256 colours, in palette order; conversion to display colours is backend work.</summary>
    public ReadOnlySpan<Bgr555> Colors => colors;

    /// <summary>The displayed whole-frame sprite draws in emission order, frontmost first, each with its colours resolved.</summary>
    public ImmutableArray<SpriteDraw> SpriteDraws => ImmutableCollectionsMarshal.AsImmutableArray(spriteDraws);

    /// <summary>What gameplay paints with: its palette now and the sprites it displays.</summary>
    public static FramePaint Capture(IPaletteColors palette, SpriteFrameBuffer sprites)
    {
        Ensure.NotNull(palette);
        Ensure.NotNull(sprites);
        return new(palette.Colors, sprites.Draws);
    }
}
