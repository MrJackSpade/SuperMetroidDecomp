using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What a frame paints with at one display publication boundary (#1403, #1464): the colours its owners show and its
/// whole-frame sprite draws. The layers and effects that use them belong to the containing frame, not here.
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
        Check(spriteDraws);
        Backgrounds = backgrounds;
        SpriteDraws = DrawColours.ResolveAll(spriteDraws, backgrounds.Sprites);
    }

    /// <summary>
    /// Keeps a gameplay frame's shading and its whole-frame sprite draws (#1512): its background pictures show their
    /// groups' shadings, its backdrop is the shading's, and each sprite draw that names its shading also shows the
    /// display's fade, unless the fade spares Samus and the draw is hers.
    /// </summary>
    /// <param name="gameplay">How the frame's background pictures are coloured, and its backdrop.</param>
    /// <param name="sprites">The sprite sets, for colouring the sprite draws that name no shading.</param>
    /// <param name="spriteDraws">The draws, frontmost first.</param>
    /// <exception cref="ArgumentException">The draw list is missing or holds a missing draw.</exception>
    public FramePaint(GameplayShading gameplay, ShownColourSets sprites, ImmutableArray<SpriteDraw> spriteDraws)
    {
        Ensure.NotNull(gameplay);
        Ensure.NotNull(sprites);
        Check(spriteDraws);
        Gameplay = gameplay;
        Backgrounds = new BackgroundColours(gameplay.Backdrop, sprites);
        ImmutableArray<SpriteDraw> draws = gameplay.Fade is null ? spriteDraws
            : ImmutableArray.CreateRange(spriteDraws, draw => Faded(draw, gameplay));
        SpriteDraws = DrawColours.ResolveAll(draws, sprites);
    }

    /// <summary>Keeps <paramref name="paint"/>'s draws with <paramref name="backgrounds"/>' colours and <paramref name="gameplay"/>'s shading.</summary>
    private FramePaint(FramePaint paint, BackgroundColours backgrounds, GameplayShading? gameplay)
    {
        Gameplay = gameplay;
        Backgrounds = backgrounds;
        SpriteDraws = paint.SpriteDraws;
    }

    /// <summary>What the frame's background art shows: its backdrop and each ink's colour (#1408).</summary>
    public BackgroundColours Backgrounds { get; }

    /// <summary>How a gameplay frame's background pictures are coloured (#1512); null for a frontend screen's frame.</summary>
    public GameplayShading? Gameplay { get; }

    /// <summary>The displayed whole-frame sprite draws in emission order, frontmost first, each with its colours resolved.</summary>
    public ImmutableArray<SpriteDraw> SpriteDraws { get; }

    /// <summary>This frame with the backdrop shown in <paramref name="backdrop"/>, as X-ray shows its own.</summary>
    public FramePaint WithBackdrop(RgbColor backdrop) =>
        new(this, Backgrounds.WithBackdrop(backdrop), Gameplay?.WithBackdrop(backdrop));

    /// <summary>A shaded sprite draw under <paramref name="gameplay"/>'s fade of the display; a draw that names no shading is left alone.</summary>
    private static SpriteDraw Faded(SpriteDraw draw, GameplayShading gameplay) =>
        draw is ImageSpriteDraw { Shading: { } shading } image
            ? image with { Shading = gameplay.OfSprite(shading, SamusColourDefinitions.Sources.Contains(draw.Source.Kind)) }
            : draw;

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
