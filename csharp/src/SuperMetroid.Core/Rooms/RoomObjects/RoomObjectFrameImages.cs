namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of one room object's frames (#1557), keyed by the frame's identity. The import pictures every
/// frame the object draws; there is no compiled copy to fall back on.
/// </summary>
/// <typeparam name="TFrame">The object's frame identity.</typeparam>
internal sealed class RoomObjectFrameImages<TFrame> where TFrame : struct, Enum
{
    /// <summary>The picture of each frame.</summary>
    private readonly IReadOnlyDictionary<TFrame, RoomObjectFrameImage> images;

    /// <summary>Holds the pictures of the frames, which the import has checked cover every frame the object draws.</summary>
    /// <param name="images">The picture of each frame, owned by these pictures.</param>
    internal RoomObjectFrameImages(IReadOnlyDictionary<TFrame, RoomObjectFrameImage> images) =>
        this.images = Ensure.NotNull(images);

    /// <summary>Every frame's picture.</summary>
    internal IReadOnlyDictionary<TFrame, RoomObjectFrameImage> All => images;

    /// <summary>The picture of <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException">No picture of <paramref name="frame"/> is installed.</exception>
    internal RoomObjectFrameImage Image(TFrame frame) =>
        images.TryGetValue(frame, out RoomObjectFrameImage? image)
            ? image
            : throw new InvalidDataException($"No installed picture of {typeof(TFrame).Name} {frame}.");
}
