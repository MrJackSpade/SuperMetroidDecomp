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
    /// <summary>The draws as a plain array; debugger state (a captured frontend display) cannot hold immutable collections.</summary>
    private readonly SpriteDraw[] spriteDraws;

    /// <summary>Keeps the colour each ink shows and the frame's whole-frame sprite draws.</summary>
    /// <param name="colours">The colour each ink shows: the background art's, and the sprite rows' for draws no owner coloured.</param>
    /// <param name="spriteDraws">The draws, frontmost first.</param>
    /// <exception cref="ArgumentException">There are not 256 colours, or the draw list is missing or holds a missing draw.</exception>
    public FramePaint(ReadOnlySpan<Bgr555> colours, ImmutableArray<SpriteDraw> spriteDraws)
        : this(new BackgroundColours(colours), spriteDraws)
    {
    }

    /// <summary>Keeps the background art's colours and the frame's whole-frame sprite draws.</summary>
    /// <param name="backgrounds">The colour each ink shows: the background art's, and the sprite rows' for draws no owner coloured.</param>
    /// <param name="spriteDraws">The draws, frontmost first.</param>
    /// <exception cref="ArgumentException">The draw list is missing or holds a missing draw.</exception>
    public FramePaint(BackgroundColours backgrounds, ImmutableArray<SpriteDraw> spriteDraws)
    {
        Ensure.NotNull(backgrounds);
        if (spriteDraws.IsDefault)
            throw new ArgumentException("A frame requires a draw list.", nameof(spriteDraws));
        foreach (SpriteDraw draw in spriteDraws)
            if (draw is null)
                throw new ArgumentException("Each sprite draw needs an entry.", nameof(spriteDraws));
        Backgrounds = backgrounds;
        this.spriteDraws = ImmutableCollectionsMarshal.AsArray(DrawColours.ResolveAll(spriteDraws, backgrounds.AllColours))
            ?? throw new ArgumentException("A frame requires a draw list.", nameof(spriteDraws));
    }

    /// <summary>What the frame's background art shows: its backdrop and each ink's colour (#1408).</summary>
    public BackgroundColours Backgrounds { get; }

    /// <summary>The displayed whole-frame sprite draws in emission order, frontmost first, each with its colours resolved.</summary>
    public ImmutableArray<SpriteDraw> SpriteDraws => ImmutableCollectionsMarshal.AsImmutableArray(spriteDraws);
}
