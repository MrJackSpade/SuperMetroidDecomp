using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A picture written over a room's layer as a frame shows it (#1466, #1514): its pre-coloured picture, as its part behind and
/// its part in front (#1634), and the layer pixel its top-left corner lies on. It covers the layer over its whole size.
/// </summary>
public readonly record struct PlacedWrittenPicture
{
    /// <summary>Places <paramref name="picture"/> with its corner on <paramref name="corner"/>.</summary>
    /// <param name="picture">The picture for the values the room's appearance inputs take now.</param>
    /// <param name="corner">The layer pixel, from the layer's top-left, the picture's corner lies on.</param>
    /// <exception cref="InvalidDataException">Neither part of the picture is installed, so it has no size.</exception>
    public PlacedWrittenPicture(LayeredPicture picture, Position16<PlanePixels> corner)
    {
        Picture = Ensure.NotNull(picture);
        if (!picture.ShowsSomething(out Extent16<ImagePixels> size))
            throw new InvalidDataException("A written picture installs neither of its parts.");
        Size = size;
        Corner = corner;
    }

    /// <summary>The picture for the values the room's appearance inputs take now.</summary>
    public LayeredPicture Picture { get; }

    /// <summary>The layer pixel, from the layer's top-left, the picture's corner lies on.</summary>
    public Position16<PlanePixels> Corner { get; }

    /// <summary>The width and height the picture covers the layer over.</summary>
    public Extent16<ImagePixels> Size { get; }
}
