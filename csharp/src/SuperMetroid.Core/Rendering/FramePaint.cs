using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
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
        Check(spriteDraws);
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
        Check(spriteDraws);
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

    /// <summary>
    /// Keeps a gameplay frame's shading and its whole-frame sprite draws (#1512): its background pictures show their
    /// groups' shadings, its backdrop is the shading's, and each sprite draw names its shading, which also shows the
    /// display's fade, unless the fade spares Samus and the draw is hers. An escape timer draw shows its pictures'
    /// group's shading after its own, as a door's fade keeps it partly lit like the HUD.
    /// </summary>
    /// <param name="gameplay">How the frame's background pictures are coloured, and its backdrop.</param>
    /// <param name="spriteDraws">The draws, frontmost first, each naming its shading.</param>
    /// <exception cref="ArgumentException">The draw list is missing, holds a missing draw, or holds a draw that names no shading.</exception>
    public FramePaint(GameplayShading gameplay, ImmutableArray<SpriteDraw> spriteDraws)
    {
        Ensure.NotNull(gameplay);
        Check(spriteDraws);
        var draws = ImmutableArray.CreateBuilder<SpriteDraw>(spriteDraws.Length);
        foreach (SpriteDraw draw in spriteDraws)
        {
            if (draw is not ImageSpriteDraw { Shading: { } shading } image)
                throw new ArgumentException("A gameplay sprite draw names the shading it shows in.", nameof(spriteDraws));
            draws.Add(image with { Shading = Shown(image, shading, gameplay) });
        }
        Gameplay = gameplay;
        Backgrounds = BackgroundColours.ScreenBackdrop(gameplay.Backdrop);
        SpriteDraws = draws.MoveToImmutable();
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
