using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What the room effect's overlay shows (#1514): a rectangle of a picture laid at a place in the overlay, which repeats
/// every <see cref="Repeat"/> pixels across and down. Nothing shows outside the rectangle; the backend owns the repeat.
/// </summary>
public sealed record OverlayPicture
{
    /// <summary>Checks that the rectangle lies on the picture and, placed, within the overlay's repeat.</summary>
    /// <param name="picture">The picture.</param>
    /// <param name="source">The picture pixel the rectangle's top-left corner shows.</param>
    /// <param name="size">The rectangle's size.</param>
    /// <param name="place">The overlay pixel the rectangle's top-left corner lies on.</param>
    /// <param name="repeat">The overlay's size before it repeats across and down.</param>
    /// <exception cref="ArgumentException">The rectangle leaves the picture or the repeat, or either is empty.</exception>
    public OverlayPicture(SpriteSheetFrame picture, Position16<ImagePixels> source, Extent16<OverlayPixels> size,
        Position16<OverlayPixels> place, Extent16<OverlayPixels> repeat)
    {
        Ensure.NotNull(picture);
        if (size.X == 0 || size.Y == 0 || source.X + size.X > picture.Size.X || source.Y + size.Y > picture.Size.Y)
            throw new ArgumentException($"A {size.X}x{size.Y} rectangle from ({source.X}, {source.Y}) does not lie on its {picture.Size.X}x{picture.Size.Y} picture.", nameof(size));
        if (place.X + size.X > repeat.X || place.Y + size.Y > repeat.Y)
            throw new ArgumentException($"A {size.X}x{size.Y} rectangle at ({place.X}, {place.Y}) leaves the {repeat.X}x{repeat.Y} overlay it repeats in.", nameof(repeat));
        Picture = picture;
        Source = source;
        Size = size;
        Place = place;
        Repeat = repeat;
    }

    /// <summary>The picture.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The picture pixel the rectangle's top-left corner shows.</summary>
    public Position16<ImagePixels> Source { get; }

    /// <summary>The rectangle's size.</summary>
    public Extent16<OverlayPixels> Size { get; }

    /// <summary>The overlay pixel the rectangle's top-left corner lies on.</summary>
    public Position16<OverlayPixels> Place { get; }

    /// <summary>The overlay's size before it repeats across and down.</summary>
    public Extent16<OverlayPixels> Repeat { get; }
}
