using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// A liquid's artwork (#1706): the strip its surface shows and the body below it, which repeats down from the strip's last
/// line. The strip's origin is the pixel on the liquid's surface line, so the strip may show rows above the surface.
/// </summary>
public sealed record LiquidArtwork : RoomEffectArtwork
{
    /// <summary>Checks that the strip's two parts place the surface alike.</summary>
    /// <param name="surface">The surface strip, its origin on the surface line.</param>
    /// <param name="body">The body, which repeats down below the strip.</param>
    /// <exception cref="InvalidDataException">The strip's parts place the surface differently.</exception>
    public LiquidArtwork(ShownLayeredPicture surface, ShownLayeredPicture body)
    {
        Surface = Ensure.NotNull(surface);
        Body = Ensure.NotNull(body);
        SurfaceSize = surface.Size;
        BodySize = body.Size;
        if (surface is TwoPartPicture both && both.BehindPart.Origin != both.InFrontPart.Origin)
            throw new InvalidDataException("A liquid's surface strip places its surface differently in its two parts.");
        SurfaceRow = surface.FirstPart.Origin.Y;
    }

    /// <summary>The surface strip, its origin on the surface line.</summary>
    public ShownLayeredPicture Surface { get; }

    /// <summary>The body, which repeats down below the strip.</summary>
    public ShownLayeredPicture Body { get; }

    /// <summary>The strip's width and height.</summary>
    public Extent16<ImagePixels> SurfaceSize { get; }

    /// <summary>The body's width and height: it repeats every body height down and every body width across.</summary>
    public Extent16<ImagePixels> BodySize { get; }

    /// <summary>The strip's row that lies on the surface line.</summary>
    public int SurfaceRow { get; }
}
