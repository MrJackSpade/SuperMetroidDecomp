using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What a frame paints with at one display publication boundary (#1403, #1464, #1512): the backdrop, a gameplay frame's
/// shading and its whole-frame sprite draws, each naming the shading it shows in. The layers and effects that use them
/// belong to the containing frame, not here.
/// </summary>
/// <remarks>
/// Capture on the simulation owner. Once captured, later simulation writes cannot change it.
/// Sprites are whole-frame draws only; a frontend screen's sprites become draws when it is captured
/// (the extraction pipeline's ObjCharacterImage), and are drawn by its scene's sprite layers (#1514).
/// </remarks>
public sealed class FramePaint
{
    /// <summary>
    /// Keeps a frontend screen's backdrop (#1514): the screen shows <paramref name="backdrop"/> where nothing is drawn, and
    /// its sprites are drawn by its scene's sprite layers, each at its place among the screen's layers.
    /// </summary>
    /// <param name="backdrop">The solid colour the screen shows where nothing is drawn.</param>
    public FramePaint(RgbColor backdrop)
        : this(backdrop, ImmutableArray<SpriteDraw>.Empty)
    {
    }

    /// <summary>
    /// Keeps a frontend screen's backdrop and the sprite draws a turning floor's blend composes itself (#1512, #1514): the
    /// screen shows <paramref name="backdrop"/> where nothing is drawn, and each of its pictures and sprite draws names the
    /// shading it shows in, so the frame holds no colours an owner shares. A screen whose sprites show at a place among
    /// its layers publishes them in sprite layers instead.
    /// </summary>
    /// <param name="backdrop">The solid colour the screen shows where nothing is drawn.</param>
    /// <param name="spriteDraws">The draws, frontmost first, each naming its shading.</param>
    /// <exception cref="ArgumentException">The draw list is missing, holds a missing draw, or holds a draw that names no shading.</exception>
    public FramePaint(RgbColor backdrop, ImmutableArray<SpriteDraw> spriteDraws)
    {
        Check(spriteDraws);
        foreach (SpriteDraw draw in spriteDraws)
        {
            if (draw is not ImageSpriteDraw { Shading: not null })
                throw new ArgumentException("A frontend screen's sprite draw names the shading it shows in.", nameof(spriteDraws));
        }
        Backdrop = backdrop;
        SpriteDraws = spriteDraws;
    }

    /// <summary>
    /// Keeps a gameplay frame's shading and its whole-frame sprite draws (#1512): its background pictures show their
    /// groups' shadings, its backdrop is the shading's, and each sprite draw names its shading, which also shows the
    /// display's fade, unless the fade spares Samus and the draw is hers. An escape timer draw shows its pictures'
    /// group's shading after its own, as a door's fade keeps it partly lit like the HUD. Each draw sits whole at its
    /// frame's depth among the room's layers (#1514); among the draws at one depth, an earlier one lies in front.
    /// </summary>
    /// <param name="gameplay">How the frame's background pictures are coloured, and its backdrop.</param>
    /// <param name="spriteDraws">The draws, frontmost first, each naming its shading.</param>
    /// <exception cref="ArgumentException">
    /// The draw list is missing, holds a missing draw, a draw that names no shading, or a draw of a frame that is not empty
    /// and has no depth among the room's layers.
    /// </exception>
    public FramePaint(GameplayShading gameplay, ImmutableArray<SpriteDraw> spriteDraws)
    {
        Ensure.NotNull(gameplay);
        Check(spriteDraws);
        var draws = ImmutableArray.CreateBuilder<SpriteDraw>(spriteDraws.Length);
        foreach (SpriteDraw draw in spriteDraws)
        {
            if (draw is not ImageSpriteDraw { Shading: { } shading } image)
                throw new ArgumentException("A gameplay sprite draw names the shading it shows in.", nameof(spriteDraws));
            if (image.Depth is null && !image.Image.IsEmpty)
                throw new ArgumentException($"Gameplay sprite draw {image} shows a frame with no depth among the room's layers.", nameof(spriteDraws));
            draws.Add(image with { Shading = Shown(image, shading, gameplay) });
        }
        Gameplay = gameplay;
        Backdrop = gameplay.Backdrop;
        SpriteDraws = draws.MoveToImmutable();
    }

    /// <summary>Keeps <paramref name="paint"/>'s draws with <paramref name="backdrop"/> and <paramref name="gameplay"/>'s shading.</summary>
    private FramePaint(FramePaint paint, RgbColor backdrop, GameplayShading? gameplay)
    {
        Gameplay = gameplay;
        Backdrop = backdrop;
        SpriteDraws = paint.SpriteDraws;
    }

    /// <summary>The colour shown where nothing is drawn.</summary>
    public RgbColor Backdrop { get; }

    /// <summary>How a gameplay frame's background pictures are coloured (#1512); null for a frontend screen's frame.</summary>
    public GameplayShading? Gameplay { get; }

    /// <summary>
    /// The displayed whole-frame sprite draws in emission order, frontmost first, each naming the shading it shows in. A
    /// gameplay frame's draws each sit at their depth among the room's layers (#1514), the emission order deciding only
    /// between draws at one depth.
    /// </summary>
    public ImmutableArray<SpriteDraw> SpriteDraws { get; }

    /// <summary>This frame with the backdrop shown in <paramref name="backdrop"/>, as X-ray shows its own.</summary>
    public FramePaint WithBackdrop(RgbColor backdrop) => new(this, backdrop, Gameplay?.WithBackdrop(backdrop));

    /// <summary>
    /// The shading a gameplay sprite draw naming <paramref name="shading"/> shows under <paramref name="gameplay"/>: an
    /// escape timer draw's group shading after its own, any other draw's own under the display's fade.
    /// </summary>
    private static PictureShading Shown(ImageSpriteDraw draw, PictureShading shading, GameplayShading gameplay) =>
        draw.Source.Kind == SpriteSourceKind.EscapeTimer
            ? gameplay.OfEscapeTimer(shading)
            : gameplay.OfSprite(shading, SamusColourDefinitions.Sources.Contains(draw.Source.Kind));

    /// <summary>Checks that a draw list is present and holds no missing draw.</summary>
    /// <exception cref="ArgumentException">The draw list is missing or holds a missing draw.</exception>
    private static void Check(ImmutableArray<SpriteDraw> spriteDraws)
    {
        if (spriteDraws.IsDefault)
            throw new ArgumentException("A frame requires a draw list.", nameof(spriteDraws));
        foreach (SpriteDraw draw in spriteDraws)
            if (draw is null)
                throw new ArgumentException("Each sprite draw needs an entry.", nameof(spriteDraws));
    }
}
