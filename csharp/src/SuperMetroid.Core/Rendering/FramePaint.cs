using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What a frame paints with at one display publication boundary (#1403, #1464): the backdrop, the colours its owners show
/// and its whole-frame sprite draws. The layers and effects that use them belong to the containing frame, not here.
/// </summary>
/// <remarks>
/// Capture on the simulation owner. Once captured, later simulation writes cannot change it.
/// Sprites are whole-frame draws only; a frontend screen's sprites become draws when it is captured
/// (the extraction pipeline's ObjCharacterImage).
/// </remarks>
public sealed class FramePaint
{
    /// <summary>Keeps the colours the frame's images show and the frame's whole-frame sprite draws.</summary>
    /// <param name="backgrounds">The colours each owner shows: the background sets', and the sprite sets' for draws no owner coloured.</param>
    /// <param name="spriteDraws">The draws, frontmost first.</param>
    /// <exception cref="ArgumentException">The draw list is missing or holds a missing draw.</exception>
    public FramePaint(BackgroundColours backgrounds, ImmutableArray<SpriteDraw> spriteDraws)
    {
        Ensure.NotNull(backgrounds);
        RequireDraws(spriteDraws);
        Backgrounds = backgrounds;
        SpriteDraws = DrawColours.ResolveAll(spriteDraws, backgrounds.Sprites);
    }

    /// <summary>
    /// Keeps a frontend screen's backdrop and sprite draws (#1512): the screen shows <paramref name="backdrop"/> where nothing
    /// is drawn, and each of its pictures and sprite draws names the shading it shows in, so the frame holds no colours an
    /// owner shares.
    /// </summary>
    /// <param name="backdrop">The solid colour the screen shows where nothing is drawn.</param>
    /// <param name="spriteDraws">The draws, frontmost first, each naming its shading.</param>
    /// <exception cref="ArgumentException">The draw list is missing, holds a missing draw, or holds a draw that names no shading.</exception>
    public FramePaint(RgbColor backdrop, ImmutableArray<SpriteDraw> spriteDraws)
    {
        RequireDraws(spriteDraws);
        foreach (SpriteDraw draw in spriteDraws)
        {
            if (draw is not ImageSpriteDraw { Shading: not null })
                throw new ArgumentException("A frontend screen's sprite draw names the shading it shows in.", nameof(spriteDraws));
        }
        Backgrounds = BackgroundColours.ScreenBackdrop(backdrop);
        SpriteDraws = spriteDraws;
    }

    /// <summary>The colour shown where nothing is drawn.</summary>
    public RgbColor Backdrop => Backgrounds.Backdrop;

    /// <summary>What the frame's background art shows: its backdrop and each ink's colour (#1408).</summary>
    public BackgroundColours Backgrounds { get; }

    /// <summary>The displayed whole-frame sprite draws in emission order, frontmost first, each with its colours resolved.</summary>
    public ImmutableArray<SpriteDraw> SpriteDraws { get; }

    /// <summary>Requires a draw list without missing draws.</summary>
    /// <exception cref="ArgumentException">The draw list is missing or holds a missing draw.</exception>
    private static void RequireDraws(ImmutableArray<SpriteDraw> spriteDraws)
    {
        if (spriteDraws.IsDefault)
            throw new ArgumentException("A frame requires a draw list.", nameof(spriteDraws));
        foreach (SpriteDraw draw in spriteDraws)
            if (draw is null)
                throw new ArgumentException("Each sprite draw needs an entry.", nameof(spriteDraws));
    }
}
