namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The scrolling sky as the background shows it in a frame (#1347, #1403, #1514): its image, which repeats down its span and
/// across, and the pictures of the cells the room rebound (#1392).
/// </summary>
public sealed class SkyPictures
{
    /// <summary>Holds the sky's image.</summary>
    public SkyPictures(RoomBackgroundImagePlane image)
    {
        Image = Ensure.NotNull(image);
    }

    /// <summary>The sky's image and its rebound cells' pictures.</summary>
    public RoomBackgroundImagePlane Image { get; }
}
