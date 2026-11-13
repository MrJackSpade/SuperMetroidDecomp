using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The artwork a room effect shows on the effect overlay, as the frame's animations select it (#1706): a liquid's surface
/// strip and body (<see cref="LiquidArtwork"/>), or an atmosphere's repeating picture (<see cref="AtmosphereArtwork"/>).
/// </summary>
public abstract record RoomEffectArtwork;

/// <summary>
/// A liquid's artwork (#1706): the strip its surface shows and the body below it, which repeats down from the strip's last
/// line. The strip's origin is the pixel on the liquid's surface line, so the strip may show rows above the surface.
/// </summary>
public sealed record LiquidArtwork : RoomEffectArtwork
{
    /// <summary>Checks that both pictures install a part and that the strip's two parts place the surface alike.</summary>
    /// <param name="surface">The surface strip, its origin on the surface line.</param>
    /// <param name="body">The body, which repeats down below the strip.</param>
    /// <exception cref="InvalidDataException">A picture installs neither part, or the strip's parts place the surface differently.</exception>
    public LiquidArtwork(LayeredPicture surface, LayeredPicture body)
    {
        Surface = Ensure.NotNull(surface);
        Body = Ensure.NotNull(body);
        SurfaceSize = surface.Size ?? throw new InvalidDataException("A liquid's surface strip installs neither of its parts.");
        BodySize = body.Size ?? throw new InvalidDataException("A liquid's body installs neither of its parts.");
        if (surface.Behind is { } behind && surface.InFront is { } inFront && behind.Origin != inFront.Origin)
            throw new InvalidDataException("A liquid's surface strip places its surface differently in its two parts.");
        SurfaceRow = (surface.Behind ?? surface.InFront ?? throw new InvalidDataException("A liquid's surface strip installs neither of its parts.")).Origin.Y;
    }

    /// <summary>The surface strip, its origin on the surface line.</summary>
    public LayeredPicture Surface { get; }

    /// <summary>The body, which repeats down below the strip.</summary>
    public LayeredPicture Body { get; }

    /// <summary>The strip's width and height.</summary>
    public Extent16<ImagePixels> SurfaceSize { get; }

    /// <summary>The body's width and height: it repeats every body height down and every body width across.</summary>
    public Extent16<ImagePixels> BodySize { get; }

    /// <summary>The strip's row that lies on the surface line.</summary>
    public int SurfaceRow { get; }
}

/// <summary>An atmosphere's artwork (#1706): one picture, repeating across and down at its size.</summary>
public sealed record AtmosphereArtwork : RoomEffectArtwork
{
    /// <summary>Checks that the picture installs a part.</summary>
    /// <param name="picture">The repeating picture.</param>
    /// <exception cref="InvalidDataException">The picture installs neither part.</exception>
    public AtmosphereArtwork(LayeredPicture picture)
    {
        Picture = Ensure.NotNull(picture);
        Size = picture.Size ?? throw new InvalidDataException("An atmosphere's picture installs neither of its parts.");
    }

    /// <summary>The repeating picture.</summary>
    public LayeredPicture Picture { get; }

    /// <summary>The picture's width and height, at which it repeats across and down.</summary>
    public Extent16<ImagePixels> Size { get; }
}
