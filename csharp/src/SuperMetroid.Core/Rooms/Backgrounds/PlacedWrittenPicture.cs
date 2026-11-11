using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A picture written over a room's layer as a frame shows it (#1466, #1514): its pre-coloured picture, as its part behind and
/// its part in front (#1634), and the layer pixel its top-left corner lies on. It covers the layer over its whole size.
/// </summary>
/// <param name="Picture">The picture for the values the room's appearance inputs take now.</param>
/// <param name="Corner">The layer pixel, from the layer's top-left, the picture's corner lies on.</param>
public readonly record struct PlacedWrittenPicture(LayeredPicture Picture, Position16<PlanePixels> Corner)
{
    /// <summary>The width and height the picture covers the layer over.</summary>
    /// <exception cref="InvalidDataException">Neither part of the picture is installed, so it has no size.</exception>
    public Extent16<ImagePixels> Size => Picture.Size ?? throw new InvalidDataException("A written picture installs neither of its parts.");
}
