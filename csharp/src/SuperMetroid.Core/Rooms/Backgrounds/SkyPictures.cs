namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The scrolling sky as the background shows it in a frame (#1347, #1403, #1514): its image and the pictures of the cells the
/// room rebound (#1392), repeating every <see cref="SpanPixels"/> down and 256 across. Every band of the span shows one of
/// its sky's pages (#1611).
/// </summary>
public sealed class SkyPictures
{
    /// <summary>Pixels down the span the sky covers before it repeats.</summary>
    public const int SpanPixels = 2048;

    /// <summary>Holds the sky's image.</summary>
    public SkyPictures(RoomBackgroundImagePlane image)
    {
        Image = Ensure.NotNull(image);
    }

    /// <summary>The sky's image and its rebound cells' pictures.</summary>
    public RoomBackgroundImagePlane Image { get; }
}
